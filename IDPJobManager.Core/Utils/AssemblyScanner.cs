using System;
using System.Collections.Generic;
using System.Reflection;

namespace IDPJobManager.Core.Utils
{
    public class AssemblyScanner
    {
        private static readonly Dictionary<string, Assembly> assemblyCache = new Dictionary<string, Assembly>();
        private readonly string assemblyDirectory;

        public AssemblyScanner(string assemblyDirectory)
        {
            this.assemblyDirectory = assemblyDirectory;
        }

        public Type GetType(string assemblyName, string className)
        {
            var assemblyPath = $"{assemblyDirectory}\\{assemblyName}.dll";
            var assembly = default(Assembly);
            if (assemblyCache.ContainsKey(assemblyPath))
                assembly = assemblyCache[assemblyPath];
            else
                assembly = Assembly.LoadFile(assemblyPath);
            return assembly.GetType(className, true, true);
        }
    }
}
