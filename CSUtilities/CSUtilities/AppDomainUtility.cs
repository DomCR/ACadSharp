using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSUtilities
{
    internal static class AppDomainUtility
    {
        public static Type[] GetTypesOfInterface<T>()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => p.GetInterface(typeof(T).FullName) != null)
                .ToArray();
        }
    }
}
