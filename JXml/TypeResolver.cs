using System;
using System.Collections.Generic;

namespace JXml
{
    public static class TypeResolver
    {
        private static readonly Dictionary<string, Type> cache = new Dictionary<string, Type>();

        public static Type Resolve(string className)
        {
            if (cache.TryGetValue(className, out Type type))
                return type;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var ass in assemblies)
            {
                var t = ass.GetType(className, false, true);
                if(t != null)
                {
                    type = t;
                    break;
                }
            }

            cache.Add(className, type);
            return type;
        }

        public static void ClearCache()
        {
            cache.Clear();
        }
    }
}
