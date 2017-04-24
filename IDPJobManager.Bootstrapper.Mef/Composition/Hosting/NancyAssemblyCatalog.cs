using IDPJobManager.Bootstrapper.Mef.Composition.Registration;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace IDPJobManager.Bootstrapper.Mef.Composition.Hosting
{

    /// <summary>
    /// Assembly catalog that provides export of Nancy implementations that are not decorated with standard
    /// MEF attributes. The <see cref="NancyReflectionContext"/> is used to virtualize MEF attributes.
    /// </summary>
    public class NancyAssemblyCatalog : AssemblyCatalog
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="assembly"></param>
        public NancyAssemblyCatalog(Assembly assembly)
            : base(assembly, new NancyReflectionContext())
        {
            Debug.Assert(assembly != null);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="codeBase"></param>
        public NancyAssemblyCatalog(string codeBase)
            : base(codeBase, new NancyReflectionContext())
        {
            Debug.Assert(codeBase != null);
        }

        public override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
        {
            //return CatalogUtils.GetExports(base.GetExports, definition);
            return base.GetExports(definition);
        }

    }

}
