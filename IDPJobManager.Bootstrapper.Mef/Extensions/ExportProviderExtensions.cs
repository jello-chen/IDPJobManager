using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace IDPJobManager.Bootstrapper.Mef.Extensions
{

    /// <summary>
    /// Various extension methods avaible for use against the Cogito specific <see cref="ExportProvider"/>
    /// implementation.
    /// </summary>
    public static class ExportProviderExtensions
    {

        /// <summary>
        /// Invokes TryGetExports, returning the output as a collection.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="definition"></param>
        /// <param name="atomicComposition"></param>
        /// <returns></returns>
        public static IEnumerable<Export> TryGetExports(this ExportProvider provider, ImportDefinition definition, AtomicComposition atomicComposition)
        {
            Debug.Assert(provider != null);
            Debug.Assert(definition != null);

            IEnumerable<Export> exports;
            provider.TryGetExports(definition, atomicComposition, out exports);
            return exports;
        }

    }

}
