using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StrmAssistant.Common
{
    internal static class ReflectionMethodResolver
    {
        public static MethodInfo FindExactMethod(Type targetType, IEnumerable<string> names, Type returnType,
            params Type[][] acceptedSignatures)
        {
            if (targetType is null) throw new ArgumentNullException(nameof(targetType));
            if (names is null) throw new ArgumentNullException(nameof(names));
            if (returnType is null) throw new ArgumentNullException(nameof(returnType));

            var methodNames = new HashSet<string>(names, StringComparer.Ordinal);
            foreach (var signature in acceptedSignatures)
            {
                var method = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(candidate =>
                    {
                        if (!methodNames.Contains(candidate.Name) || candidate.ReturnType != returnType) return false;
                        var parameters = candidate.GetParameters();
                        return parameters.Length == signature.Length &&
                               parameters.Select(p => p.ParameterType).SequenceEqual(signature);
                    });

                if (method != null) return method;
            }

            return null;
        }
    }
}
