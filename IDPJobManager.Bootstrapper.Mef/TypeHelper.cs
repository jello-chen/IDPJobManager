using IDPJobManager.Bootstrapper.Mef.Extensions;
using Nancy;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDPJobManager.Bootstrapper.Mef
{
    internal static class TypeHelper
    {

        /// <summary>
        /// Returns <c>true</c> if the type references Nancy in some way.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool ReferencesNancy(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            // does type's assembly's references contain Nancy assembly?
            return type.Assembly.GetReferencedAssemblies()
                .Prepend(type.Assembly.GetName())
                .Any(r => r.Name == typeof(INancyEngine).Assembly.GetName().Name);
        }

    }
}
