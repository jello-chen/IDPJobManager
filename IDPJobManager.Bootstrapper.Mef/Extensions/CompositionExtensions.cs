using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.Registration;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace IDPJobManager.Bootstrapper.Mef.Extensions
{

    static class CompositionExtensions
    {

        /// <summary>
        ///  Creates a part from the specified object under the specified contract name and composes it in the specified
        ///  composition container.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="contractName"></param>
        /// <param name="exportedValue"></param>
        public static void ComposeExportedValue(this CompositionContainer container, string contractName, object exportedValue)
        {
            Debug.Assert(container != null);
            Debug.Assert(contractName != null);
            Debug.Assert(exportedValue != null);

            var b = new CompositionBatch();
            var m = new Dictionary<string, object>();
            b.AddExport(new Export(contractName, m, () => exportedValue));
            container.Compose(b);
        }

        public static void ComposeExportedValue(this CompositionContainer container, Type contractType, object exportedValue)
        {
            Debug.Assert(container != null);
            Debug.Assert(contractType != null);
            Debug.Assert(exportedValue != null);

            var contractName = AttributedModelServices.GetTypeIdentity(contractType);
            var b = new CompositionBatch();
            var m = new Dictionary<string, object>();
            m[CompositionConstants.ExportTypeIdentityMetadataName] = contractName;
            b.AddExport(new Export(contractName, m, () => exportedValue));
            container.Compose(b);
        }

        public static void ComposeExportedValues(this CompositionContainer container, Type contractType, Type[] implementationTypes)
        {
            var registrationBuilder = new RegistrationBuilder();
            foreach (var implementationType in implementationTypes)
            {
                registrationBuilder.ForType(implementationType).Export(c => c.AsContractType(contractType));
            }
            container.ComposeParts(registrationBuilder);
        }

        public static void ComposeExportedValues<T>(this CompositionContainer container, Type[] implementationTypes)
        {
            var registrationBuilder = new RegistrationBuilder();
            foreach (var implementationType in implementationTypes)
            {
                registrationBuilder.ForType(implementationType).Export(c => c.AsContractType<T>());
            }
            container.ComposeParts(registrationBuilder);
        }

        public static T GetExportedValue<T>(this CompositionContainer container, Type contractType)
        {
            Debug.Assert(container != null);
            return container.GetExportedValue<T>(AttributedModelServices.GetContractName(contractType));
        }

        public static T GetExportedValueOrDefault<T>(this CompositionContainer container, Type contractType)
        {
            Debug.Assert(container != null);
            return container.GetExportedValueOrDefault<T>(AttributedModelServices.GetContractName(contractType));
        }

    }

}
