using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace StrmAssistant.Common
{
    internal static class MediaMountCompatibility
    {
        public static async Task<IDisposable> MountAsync(object mediaMountManager, string mediaPath,
            CancellationToken cancellationToken)
        {
            if (mediaMountManager is null) throw new ArgumentNullException(nameof(mediaMountManager));
            if (string.IsNullOrEmpty(mediaPath)) throw new ArgumentException("Media path is required.", nameof(mediaPath));

            var mountMethod = mediaMountManager.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(IsSupportedMountMethod);

            if (mountMethod is null)
            {
                throw new MissingMethodException(mediaMountManager.GetType().FullName,
                    "Mount(string|ReadOnlyMemory<char>, string|ReadOnlyMemory<char>, CancellationToken)");
            }

            var parameters = mountMethod.GetParameters();
            var result = mountMethod.Invoke(mediaMountManager, new[]
            {
                GetMountArgument(parameters[0].ParameterType, mediaPath),
                GetMountArgument(parameters[1].ParameterType, null),
                (object)cancellationToken
            });

            if (!(result is Task task))
            {
                throw new InvalidOperationException($"{mountMethod.Name} did not return a Task.");
            }

            await task.ConfigureAwait(false);

            var mediaMount = task.GetType().GetProperty("Result", BindingFlags.Instance | BindingFlags.Public)
                ?.GetValue(task);
            if (mediaMount is null) return null;

            if (mediaMount is IDisposable disposable) return disposable;

            throw new InvalidOperationException($"{mountMethod.Name} returned a mount that is not disposable.");
        }

        public static string GetMountedPath(object mediaMount)
        {
            if (mediaMount is null) return null;

            var mediaMountType = mediaMount.GetType();
            if (mediaMountType.GetProperty("MountedPath", BindingFlags.Instance | BindingFlags.Public)
                    ?.GetValue(mediaMount) is string mountedPath)
            {
                return mountedPath;
            }

            var mountedPathInfo = mediaMountType
                .GetProperty("MountedPathInfo", BindingFlags.Instance | BindingFlags.Public)
                ?.GetValue(mediaMount);
            if (mountedPathInfo?.GetType().GetProperty("FullName", BindingFlags.Instance | BindingFlags.Public)
                    ?.GetValue(mountedPathInfo) is string fullName)
            {
                return fullName;
            }

            throw new MissingMemberException(mediaMountType.FullName, "MountedPath or MountedPathInfo.FullName");
        }

        private static bool IsSupportedMountMethod(MethodInfo method)
        {
            if (!string.Equals(method.Name, "Mount", StringComparison.Ordinal)) return false;

            var parameters = method.GetParameters();
            return parameters.Length == 3 &&
                   IsSupportedPathType(parameters[0].ParameterType) &&
                   IsSupportedPathType(parameters[1].ParameterType) &&
                   parameters[2].ParameterType == typeof(CancellationToken) &&
                   typeof(Task).IsAssignableFrom(method.ReturnType);
        }

        private static bool IsSupportedPathType(Type parameterType)
        {
            return parameterType == typeof(string) || parameterType == typeof(ReadOnlyMemory<char>);
        }

        private static object GetMountArgument(Type parameterType, string value)
        {
            if (parameterType == typeof(string)) return value;
            if (parameterType == typeof(ReadOnlyMemory<char>))
                return value?.AsMemory() ?? ReadOnlyMemory<char>.Empty;

            throw new NotSupportedException($"Unsupported media mount path type: {parameterType.FullName}");
        }
    }
}
