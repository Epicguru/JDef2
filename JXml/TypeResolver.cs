using System;
using System.Collections.Generic;
using System.Reflection;

namespace JXml
{
    public static class TypeResolver
    {
        private static readonly Dictionary<string, Type> cache = new Dictionary<string, Type>();
        private static Assembly[] assemblies;

        public static Type Resolve(string className)
        {
            if (cache.TryGetValue(className, out Type type))
                return type;

            if (assemblies == null)
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }

            foreach (var ass in assemblies)
            {
                var t = ass.GetType(className, false, true);
                if(t != null)
                {
                    type = t;
                    break;
                }
            }

            if (type != null)
            {
                cache.Add(className, type);
                return type;
            }
            return null;
        }

        public static void ClearCache()
        {
            cache.Clear();
        }
    }
}
