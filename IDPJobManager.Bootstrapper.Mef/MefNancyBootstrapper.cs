using Nancy.Bootstrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Reflection;
using IDPJobManager.Bootstrapper.Mef.Composition.Hosting;
using IDPJobManager.Bootstrapper.Mef.Extensions;
using Nancy.Diagnostics;

namespace IDPJobManager.Bootstrapper.Mef
{
    /// <summary>
    /// Serves as a bootstrapper for Nancy when using the Managed Extensibility Framework.
    /// </summary>
    [InheritedExport(typeof(INancyBootstrapper))]
    [InheritedExport(typeof(INancyModuleCatalog))]
    public abstract class MefNancyBootstrapper : NancyBootstrapperWithRequestContainerBase<CompositionContainer>
    {

        private CompositionContainer _container;

        protected MefNancyBootstrapper(CompositionContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        protected override CompositionContainer GetApplicationContainer()
        {
            return _container;
        }

        protected override void RegisterBootstrapperTypes(CompositionContainer applicationContainer)
        {
            applicationContainer.ComposeParts(this);
        }

        protected override void RegisterTypes(CompositionContainer container, IEnumerable<TypeRegistration> typeRegistrations)
        {
            var types = typeRegistrations
                .Where(i => i.ImplementationType.IsClass)
                .Where(i => !IsTypeRegistered(container, i.RegistrationType, i.ImplementationType))
                .SelectMany(i => new[] { i.ImplementationType, i.RegistrationType })
                .Select(i => i.UnderlyingSystemType)
                .Distinct()
                .OrderBy(i => i.FullName);
            if (types.Any())
                AddCatalog(container, new NancyTypeCatalog(types));
        }

        /// <summary>
        /// Bind the various collections into the container as singletons to later be resolved by IEnumerable{Type}
        /// constructor dependencies.
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="collectionTypeRegistrations">Collection type registrations to register</param>
        protected override void RegisterCollectionTypes(CompositionContainer container, IEnumerable<CollectionTypeRegistration> collectionTypeRegistrations)
        {
            // transform for RegisterTypes implementation
            RegisterTypes(container, collectionTypeRegistrations
                .SelectMany(i => i.ImplementationTypes
                    .Select(j => new TypeRegistration(i.RegistrationType, j))));
        }
        /// <summary>
        /// Bind the given instances into the container.
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="instanceRegistrations">Instance registration types</param>
        protected override void RegisterInstances(CompositionContainer container, IEnumerable<InstanceRegistration> instanceRegistrations)
        {
            // register the types
            RegisterTypes(container, instanceRegistrations
                .Select(i => new TypeRegistration(i.RegistrationType, i.Implementation.GetType())));

            // and export the instances
            foreach (var r in instanceRegistrations)
                container.ComposeExportedValue(r.RegistrationType, r.Implementation);
        }

        /// <summary>
        /// Registers per-request modules in the per-request container.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="moduleRegistrationTypes"></param>
        protected override void RegisterRequestContainerModules(CompositionContainer container, IEnumerable<ModuleRegistration> moduleRegistrationTypes)
        {
            RegisterModules(container, moduleRegistrationTypes);
        }

        /// <summary>
        /// Gets the diagnostics for initialization.
        /// </summary>
        protected override IDiagnostics GetDiagnostics()
        {
            return ApplicationContainer.GetExportedValue<IDiagnostics>();
        }


        /// <summary>
        /// Gets all registered application startup tasks.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<IApplicationStartup> GetApplicationStartupTasks()
        {
            return ApplicationContainer.GetExportedValues<IApplicationStartup>();
        }

        protected override IEnumerable<IRequestStartup> RegisterAndGetRequestStartupTasks(CompositionContainer container, Type[] requestStartupTypes)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<IRegistrations> GetRegistrationTasks()
        {
            throw new NotImplementedException();
        }

        protected override INancyEngine GetEngineInternal()
        {
            return ApplicationContainer.GetExportedValueOrDefault<INancyEngine>();
        }

        protected override IEnumerable<INancyModule> GetAllModules(CompositionContainer container)
        {
            return container.GetExportedValues<INancyModule>();
        }

        protected override INancyModule GetModule(CompositionContainer container, Type moduleType)
        {
            return container.GetExports<INancyModule>().Select(t => t.Value).FirstOrDefault(t => t.GetType() == moduleType);
        }

        protected virtual void AddCatalog(CompositionContainer container, ComposablePartCatalog catalog)
        {
            var provider = container.Providers.OfType<NancyExportProvider>().FirstOrDefault();
            if (provider == null)
                throw new NullReferenceException("Container does not contain a NancyExportProvider. Override RegisterTypes to provide custom implementation.");

            // add new catalog
            provider.Providers.Add(new CatalogExportProvider(catalog)
            {
                SourceProvider = container,
            });
        }

        /// <summary>
        /// Returns <c>true</c> if the type given by <paramref name="implementationType"/> is already available as an export
        /// of <paramref cref="contractType"/> in the container.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="contractType"></param>
        /// <param name="implementationType"></param>
        /// <returns></returns>
        private bool IsTypeRegistered(CompositionContainer container, Type contractType, Type implementationType)
        {
            return container.GetExports(
                new ContractBasedImportDefinition(
                    AttributedModelServices.GetContractName(contractType),
                    AttributedModelServices.GetTypeIdentity(contractType),
                    null,
                    ImportCardinality.ZeroOrMore,
                    false,
                    false,
                    CreationPolicy.Any))
            .Select(i =>
                ReflectionModelServices.GetExportingMember(i.Definition))
            .Where(i =>
                // export must be associated with a Type, implemented by the specified type
                i.MemberType == MemberTypes.TypeInfo &&
                i.GetAccessors().Any(j => ((TypeInfo)j).UnderlyingSystemType == implementationType.UnderlyingSystemType))
            .Any();
        }
    }
}
