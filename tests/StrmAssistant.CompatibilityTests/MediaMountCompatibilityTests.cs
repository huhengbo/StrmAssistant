using System;
using System.Threading;
using System.Threading.Tasks;
using StrmAssistant.Common;
using Xunit;

namespace StrmAssistant.CompatibilityTests
{
    public class MediaMountCompatibilityTests
    {
        [Fact]
        public async Task MountAsync_UsesLegacyMemoryContract()
        {
            var manager = new LegacyMediaMountManager();

            using var mount = await MediaMountCompatibility
                .MountAsync(manager, "legacy.strm", CancellationToken.None);

            Assert.Equal("legacy.strm", manager.MediaPath.ToString());
            Assert.True(manager.Container.IsEmpty);
            Assert.Equal("/mounted/legacy.mp4", MediaMountCompatibility.GetMountedPath(mount));
        }

        [Fact]
        public async Task MountAsync_UsesCurrentStringContract()
        {
            var manager = new CurrentMediaMountManager();

            using var mount = await MediaMountCompatibility
                .MountAsync(manager, "current.strm", CancellationToken.None);

            Assert.Equal("current.strm", manager.MediaPath);
            Assert.Null(manager.Container);
            Assert.Equal(CancellationToken.None, manager.CancellationToken);
            Assert.Equal("/mounted/current.mp4", MediaMountCompatibility.GetMountedPath(mount));
        }

        [Fact]
        public async Task MountAsync_FailsClearlyForUnknownContract()
        {
            var exception = await Assert.ThrowsAsync<MissingMethodException>(() =>
                MediaMountCompatibility.MountAsync(new UnsupportedMediaMountManager(), "unknown.strm",
                    CancellationToken.None));

            Assert.Contains("Mount(string|ReadOnlyMemory<char>", exception.Message);
        }

        private sealed class LegacyMediaMountManager
        {
            public ReadOnlyMemory<char> MediaPath { get; private set; }
            public ReadOnlyMemory<char> Container { get; private set; }

            public Task<LegacyMediaMount> Mount(ReadOnlyMemory<char> mediaPath, ReadOnlyMemory<char> container,
                CancellationToken cancellationToken)
            {
                MediaPath = mediaPath;
                Container = container;
                return Task.FromResult(new LegacyMediaMount());
            }
        }

        private sealed class CurrentMediaMountManager
        {
            public string MediaPath { get; private set; }
            public string Container { get; private set; }
            public CancellationToken CancellationToken { get; private set; }

            public Task<CurrentMediaMount> Mount(string mediaPath, string container,
                CancellationToken cancellationToken)
            {
                MediaPath = mediaPath;
                Container = container;
                CancellationToken = cancellationToken;
                return Task.FromResult(new CurrentMediaMount());
            }
        }

        private sealed class UnsupportedMediaMountManager
        {
            public Task<object> Mount(Uri mediaPath, string container, CancellationToken cancellationToken)
            {
                return Task.FromResult<object>(null);
            }
        }

        private sealed class LegacyMediaMount : IDisposable
        {
            public string MountedPath => "/mounted/legacy.mp4";
            public void Dispose() { }
        }

        private sealed class CurrentMediaMount : IDisposable
        {
            public MountedPathInfo MountedPathInfo { get; } = new MountedPathInfo();
            public void Dispose() { }
        }

        private sealed class MountedPathInfo
        {
            public string FullName => "/mounted/current.mp4";
        }
    }
}
