using System;
using System.IO;
using System.Security.Cryptography;

namespace StrmAssistant.Common
{
    internal static class PluginUpdateSecurity
    {
        public static string BuildDownloadUrl(string assetUrl, string proxyUrl)
        {
            if (!Uri.IsWellFormedUriString(assetUrl, UriKind.Absolute))
                throw new ArgumentException("Invalid plugin download URL", nameof(assetUrl));

            return string.IsNullOrWhiteSpace(proxyUrl)
                ? assetUrl
                : $"{proxyUrl.TrimEnd('/')}/{assetUrl.TrimStart('/')}";
        }

        public static bool ShouldAttachGitHubToken(string requestUrl)
        {
            return Uri.TryCreate(requestUrl, UriKind.Absolute, out var uri) &&
                   string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(uri.Host, "api.github.com", StringComparison.OrdinalIgnoreCase);
        }

        public static string ValidatePluginPayload(Stream stream, string expectedDigest)
        {
            if (stream is null || !stream.CanRead || !stream.CanSeek || stream.Length < 4096)
                throw new InvalidDataException("Downloaded plugin is empty or unexpectedly small");

            stream.Position = 0;
            var first = stream.ReadByte();
            var second = stream.ReadByte();
            if (first != 'M' || second != 'Z')
                throw new InvalidDataException("Downloaded file is not a valid PE/DLL payload");

            stream.Position = 0;
            string actualDigest;
            using (var sha256 = SHA256.Create())
            {
                actualDigest = BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", string.Empty)
                    .ToLowerInvariant();
            }

            if (!string.IsNullOrWhiteSpace(expectedDigest))
            {
                var expected = expectedDigest.Trim();
                const string prefix = "sha256:";
                if (expected.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    expected = expected.Substring(prefix.Length);

                if (!string.Equals(actualDigest, expected, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Downloaded plugin SHA-256 does not match the release asset digest");
            }

            stream.Position = 0;
            return actualDigest;
        }
    }
}
