using StrmAssistant.Common;
using System;
using System.IO;
using System.Security.Cryptography;
using Xunit;

namespace StrmAssistant.CompatibilityTests
{
    public class PluginUpdateSecurityTests
    {
        [Fact]
        public void Token_IsOnlyAttachedToOfficialGitHubApi()
        {
            Assert.True(PluginUpdateSecurity.ShouldAttachGitHubToken(
                "https://api.github.com/repos/huhengbo/StrmAssistant/releases/latest"));
            Assert.False(PluginUpdateSecurity.ShouldAttachGitHubToken(
                "https://github.com/huhengbo/StrmAssistant/releases/download/v1/plugin.dll"));
            Assert.False(PluginUpdateSecurity.ShouldAttachGitHubToken(
                "https://example-proxy.invalid/https://github.com/plugin.dll"));
            Assert.False(PluginUpdateSecurity.ShouldAttachGitHubToken(
                "http://api.github.com/repos/huhengbo/StrmAssistant/releases/latest"));
        }

        [Fact]
        public void ProxyUrl_DoesNotChangeTheOriginalAssetWhenProxyIsEmpty()
        {
            const string asset = "https://github.com/huhengbo/StrmAssistant/releases/download/v1/plugin.dll";
            Assert.Equal(asset, PluginUpdateSecurity.BuildDownloadUrl(asset, string.Empty));
            Assert.Equal("https://proxy.example/https://github.com/huhengbo/StrmAssistant/releases/download/v1/plugin.dll",
                PluginUpdateSecurity.BuildDownloadUrl(asset, "https://proxy.example/"));
        }

        [Fact]
        public void ValidatePluginPayload_VerifiesPeHeaderAndDigest()
        {
            var bytes = new byte[4096];
            bytes[0] = (byte)'M';
            bytes[1] = (byte)'Z';
            for (var i = 2; i < bytes.Length; i++) bytes[i] = (byte)(i % 251);

            string expected;
            using (var sha256 = SHA256.Create())
            {
                expected = BitConverter.ToString(sha256.ComputeHash(bytes)).Replace("-", string.Empty)
                    .ToLowerInvariant();
            }

            using var stream = new MemoryStream(bytes);
            var actual = PluginUpdateSecurity.ValidatePluginPayload(stream, "sha256:" + expected);

            Assert.Equal(expected, actual);
            Assert.Equal(0, stream.Position);
        }

        [Fact]
        public void ValidatePluginPayload_RejectsDigestMismatch()
        {
            var bytes = new byte[4096];
            bytes[0] = (byte)'M';
            bytes[1] = (byte)'Z';

            using var stream = new MemoryStream(bytes);
            Assert.Throws<InvalidDataException>(() =>
                PluginUpdateSecurity.ValidatePluginPayload(stream, "sha256:deadbeef"));
        }

        [Fact]
        public void ValidatePluginPayload_RejectsNonPeContent()
        {
            using var stream = new MemoryStream(new byte[4096]);
            Assert.Throws<InvalidDataException>(() => PluginUpdateSecurity.ValidatePluginPayload(stream, null));
        }
    }
}
