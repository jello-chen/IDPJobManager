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
using System.Diagnostics;

namespace IDPJobManager.Bootstrapper.Mef
{
    /// <summary>
    /// Serves as a bootstrapper for Nancy when using the Managed Extensibility Framework.
    /// </summary>
    /// <typeparam name="TContainer"></typeparam>
    /// <typeparam name="TInternalContainer"></typeparam>
    [InheritedExport(typeof(INancyBootstrapper))]
    [InheritedExport(typeof(INancyModuleCatalog))]
    public abstract class MefNancyBootstrapper<TContainer, TInternalContainer> :
        NancyBootstrapperWithRequestContainerBase<TInternalContainer>,
        IDisposable
        where TContainer : CompositionContainer
        where TInternalContainer : CompositionContainer
    {

        TContainer container;
        readonly bool containerOwner;
        TInternalContainer internalContainer;

        /// <summary>
        /// Initializes a new instance. Creates a container that scans for all assemblies in the current <see
        /// cref="AppDomain"/>.
        /// </summary>
        public MefNancyBootstrapper() : this(createDefaultContainer: true)
        {

        }

        /// <summary>
        /// Initializes a new instance. Optionally specifies whether a default container should be created. By default
        /// a container that scans for all assemblies in the current <see cref="AppDomain"/> is used.
        /// </summary>
        public MefNancyBootstrapper(bool createDefaultContainer = true) : base()
        {
            if (createDefaultContainer)
            {
                this.container = CreateDefaultContainer();
                this.containerOwner = true;
            }
        }

        public MefNancyBootstrapper(TContainer container) : base()
        {
            Debug.Assert(container != null);
            this.container = container;
        }

        /// <summary>
        /// Creates the default container when none other is specified.
        /// </summary>
        /// <returns></returns>
        protected abstract TContainer CreateDefaultContainer();

        /// <summary>
        /// Creates a Nancy-specific <see cref="CompositionContainer"/> containing the <see
        /// cref="NancyExportProvider"/>. This container serves merely as a holder for the <see
        /// cref="NancyExportProvider"/> and should itself home no exports.
        /// </summary>
        /// <returns></returns>
        protected sealed override TInternalContainer GetApplicationContainer()
        {
            return GetInternalContainer(container);
        }

        /// <summary>
        /// Gets the internal container instance.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        protected abstract TInternalContainer GetInternalContainer(TContainer container);

        /// <summary>
        /// Provides a place to configure the newly created <see cref="CompositionContainer"/>.
        /// </summary>
        /// <param name="container"></param>
        protected sealed override void ConfigureApplicationContainer(TInternalContainer container)
        {
            ConfigureInternalContainer(container);
        }

        /// <summary>
        /// Provides a place to configure the newly created internal container.
        /// </summary>
        /// <param name="container"></param>
        protected virtual void ConfigureInternalContainer(TInternalContainer container)
        {

        }

        /// <summary>
        /// Bind the bootstrapper's implemented types into the container. This is necessary so a user can pass in a
        /// populated container but not have to take the responsibility of registering things like <see
        /// cref="INancyModuleCatalog"/> manually.
        /// </summary>
        /// <param name="applicationContainer">Application container to register into</param>
        protected override sealed void RegisterBootstrapperTypes(TInternalContainer applicationContainer)
        {
            applicationContainer.ComposeParts(this);
        }

        /// <summary>
        /// Bind the default implementations of internally used types into the container as singletons.
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="typeRegistrations">Type registrations to register</param>
        protected override void RegisterTypes(TInternalContainer container, IEnumerable<TypeRegistration> typeRegistrations)
        {
            Debug.Assert(container != null);
            Debug.Assert(typeRegistrations != null);

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
        protected override sealed void RegisterCollectionTypes(TInternalContainer container, IEnumerable<CollectionTypeRegistration> collectionTypeRegistrations)
        {
            Debug.Assert(container != null);
            Debug.Assert(collectionTypeRegistrations != null);

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
        protected override void RegisterInstances(TInternalContainer container, IEnumerable<InstanceRegistration> instanceRegistrations)
        {
            Debug.Assert(container != null);
            Debug.Assert(instanceRegistrations != null);

            // register the types
            RegisterTypes(container, instanceRegistrations
                .Select(i => new TypeRegistration(i.RegistrationType, i.Implementation.GetType())));

            // and export the instances
            foreach (var r in instanceRegistrations)
                container.ComposeExportedValue(r.RegistrationType, r.Implementation);
        }

        /// <summary>
        /// Returns <c>true</c> if the type given by <paramref name="implementationType"/> is already available as an export
        /// of <paramref cref="contractType"/> in the container.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="contractType"></param>
        /// <param name="implementationType"></param>
        /// <returns></returns>
        bool IsTypeRegistered(TInternalContainer container, Type contractType, Type implementationType)
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

        /// <summary>
        /// Adds the specified <see cref="ComposablePartCatalog"/> to the specified container. The default
        /// implementation expects the container to have a <see cref="NancyExportProvider"/> in it's Providers
        /// collection. To override this method, ensure that the catalog you add is added to a <see
        /// cref="NancyExportProvider"/>. This ensures behavior expected by Nancy, such as default instance resolution,
        /// is provided properly.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="catalog"></param>
        protected virtual void AddCatalog(TInternalContainer container, ComposablePartCatalog catalog)
        {
            Debug.Assert(container != null);
            Debug.Assert(catalog != null);

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
        /// Registers per-request modules in the per-request container.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="moduleRegistrationTypes"></param>
        protected override void RegisterRequestContainerModules(TInternalContainer container, IEnumerable<ModuleRegistration> moduleRegistrationTypes)
        {
            Debug.Assert(container != null);
            Debug.Assert(moduleRegistrationTypes != null);

            RegisterModules(container, moduleRegistrationTypes);
        }

        /// <summary>
        /// Gets the diagnostics for initialization.
        /// </summary>
        protected override IDiagnostics GetDiagnostics()
        {
            Debug.Assert(ApplicationContainer != null);

            return ApplicationContainer.GetExportedValue<IDiagnostics>();
        }

        /// <summary>
        /// Gets all registered application startup tasks.
        /// </summary>
        protected override IEnumerable<IApplicationStartup> GetApplicationStartupTasks()
        {
            Debug.Assert(ApplicationContainer != null);

            return ApplicationContainer.GetExportedValues<IApplicationStartup>();
        }

        protected override IEnumerable<IRequestStartup> RegisterAndGetRequestStartupTasks(TInternalContainer container, Type[] requestStartupTypes)
        {
            container.ComposeExportedValues<IRequestStartup>(requestStartupTypes);
            return container.GetExportedValues<IRequestStartup>();
        }

        /// <summary>
        /// Gets all registration tasks.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<IRegistrations> GetRegistrationTasks()
        {
            return ApplicationContainer.GetExportedValues<IRegistrations>();
        }

        /// <summary>
        /// Gets the engine implementation from the container.
        /// </summary>
        protected override sealed INancyEngine GetEngineInternal()
        {
            Debug.Assert(ApplicationContainer != null);

            return ApplicationContainer.GetExportedValueOrDefault<INancyEngine>();
        }

        /// <summary>
        /// Retrieve all module instances from the container.
        /// </summary>
        protected override sealed IEnumerable<INancyModule> GetAllModules(TInternalContainer container)
        {
            Debug.Assert(container != null);

            return container.GetExportedValues<INancyModule>();
        }

        /// <summary>
        /// Retreive a specific module instance from the container.
        /// </summary>
        /// <param name="container">Container to use</param>
        /// <param name="moduleType">Type of the module</param>
        protected override INancyModule GetModule(TInternalContainer container, Type moduleType)
        {
            Debug.Assert(container != null);
            Debug.Assert(moduleType != null);

            var nancyModule = container.GetExports<INancyModule>()
                .Select(i => i.Value)
                .FirstOrDefault(i => i.GetType() == moduleType);

            return nancyModule;
        }

        public new void Dispose()
        {
            base.Dispose();

            // dispose of user provided, or default container
            if (container != null)
            {
                if (containerOwner) container.Dispose();
                container = null;
            }

            // dispose of Nancy specific container
            if (internalContainer != null)
            {
                internalContainer.Dispose();
                internalContainer = null;
            }
        }
    }

    /// <summary>
    /// Serves as a bootstrapper for Nancy when using the Managed Extensibility Framework. This generic version
    /// operates with the specified container type.
    /// </summary>
    /// <typeparam name="TContainer"></typeparam>
    public abstract class MefNancyBootstrapper<TContainer>
        : MefNancyBootstrapper<TContainer, CompositionContainer>
        where TContainer : CompositionContainer
    {
        public MefNancyBootstrapper() : base()
        {

        }

        public MefNancyBootstrapper(bool createDefaultContainer = true)
            : base(createDefaultContainer)
        {

        }

        public MefNancyBootstrapper(TContainer container)
            : base(container)
        {

        }

        protected override CompositionContainer GetInternalContainer(TContainer container)
        {
            var providers = new List<ExportProvider>
            {
                new NancyExportProvider(),
            };

            if (container != null)
                providers.Add(container);

            return new CompositionContainer(
                CompositionOptions.DisableSilentRejection |
                CompositionOptions.ExportCompositionService |
                CompositionOptions.IsThreadSafe,
                providers.ToArray());

        }

        protected override CompositionContainer CreateRequestContainer(NancyContext context)
        {
            return new CompositionContainer(
                CompositionOptions.DisableSilentRejection |
                CompositionOptions.ExportCompositionService |
                CompositionOptions.IsThreadSafe,
                new NancyExportProvider(),
                ApplicationContainer);
        }
    }

    /// <summary>
    /// Serves as a bootstrapper for Nancy when using the Managed Extensibility Framework. This non-generic version
    /// operates directly with MEF <see cref="CompositionContainer"/> types.
    /// </summary>
    public class MefNancyBootstrapper :
        MefNancyBootstrapper<CompositionContainer>
    {
        public MefNancyBootstrapper() : base()
        {
            
        }

        public MefNancyBootstrapper(bool createDefaultContainer = true)
            : base(createDefaultContainer)
        {

        }

        public MefNancyBootstrapper(CompositionContainer container)
            : base(container)
        {

        }

        protected override CompositionContainer CreateDefaultContainer()
        {
            return new CompositionContainer();
        }
    }
}
