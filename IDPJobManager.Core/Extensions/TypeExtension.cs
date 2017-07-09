using IDPJobManager.Core.Utils;
using System;

namespace IDPJobManager.Core.Extensions
{
    public static class TypeExtension
    {
        public static bool Is<T>(this Type type)
        {
            Ensure.Requires<ArgumentNullException>(type != null, "`type` should be required.");
            return typeof(T).IsAssignableFrom(type);
        }

        public static bool Is(this Type type, Type baseType)
        {
            Ensure.Requires<ArgumentNullException>(type != null, "`type` should be required.");
            Ensure.Requires<ArgumentNullException>(baseType != null, "`baseType` should be required.");
            return baseType.IsAssignableFrom(type);
        }
    }
}
