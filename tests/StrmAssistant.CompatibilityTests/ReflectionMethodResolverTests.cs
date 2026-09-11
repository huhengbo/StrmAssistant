using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StrmAssistant.Common;
using Xunit;

namespace StrmAssistant.CompatibilityTests
{
    public class ReflectionMethodResolverTests
    {
        [Fact]
        public void FindsPreferredExactSignatureInsteadOfSameLengthLookalike()
        {
            var method = ReflectionMethodResolver.FindExactMethod(
                typeof(MediaSourceManagerFixture),
                new[] { "GetStaticMediaSources" },
                typeof(List<string>),
                new[] { typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(string[]), typeof(int), typeof(object), typeof(object), typeof(CancellationToken) },
                new[] { typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(string[]), typeof(int), typeof(object), typeof(object) });

            Assert.NotNull(method);
            Assert.Equal(10, method.GetParameters().Length);
            Assert.Equal(typeof(CancellationToken), method.GetParameters()[9].ParameterType);
        }

        [Fact]
        public void RejectsWrongReturnTypeEvenWhenParametersMatch()
        {
            var method = ReflectionMethodResolver.FindExactMethod(
                typeof(WrongReturnFixture),
                new[] { "Resolve" },
                typeof(Task<bool>),
                new[] { typeof(string), typeof(CancellationToken) });

            Assert.Null(method);
        }

        [Fact]
        public void SupportsRenamedMethodsWithTheSameContract()
        {
            var method = ReflectionMethodResolver.FindExactMethod(
                typeof(RenamedFixture),
                new[] { "GetExternalSubtitleStreams", "GetExternalTracks" },
                typeof(List<string>),
                new[] { typeof(string), typeof(int), typeof(bool) });

            Assert.NotNull(method);
            Assert.Equal("GetExternalTracks", method.Name);
        }

        private sealed class MediaSourceManagerFixture
        {
            public List<string> GetStaticMediaSources(string item, bool a, bool b, bool c, int wrong,
                string[] folders, int options, object profile, object user, CancellationToken token) => null;

            public List<string> GetStaticMediaSources(string item, bool a, bool b, bool c, bool d,
                string[] folders, int options, object profile, object user, CancellationToken token) => null;

            public List<string> GetStaticMediaSources(string item, bool a, bool b, bool c,
                string[] folders, int options, object profile, object user) => null;
        }

        private sealed class WrongReturnFixture
        {
            public Task Resolve(string value, CancellationToken token) => Task.CompletedTask;
        }

        private sealed class RenamedFixture
        {
            public List<string> GetExternalTracks(string item, int index, bool clearCache) => null;
        }
    }
}
