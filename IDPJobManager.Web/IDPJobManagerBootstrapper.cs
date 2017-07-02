namespace IDPJobManager.Web
{
    using System;
    using System.Reflection;
    using Nancy.Bootstrapper;
    using Nancy.Json;
    using Nancy.ViewEngines;
    using Nancy;
    using Nancy.Authentication.Basic;
    using Nancy.Hosting.Self;
    using Nancy.Session;
    using Nancy.Conventions;
    using Swagger.ObjectModel;
    using Nancy.Swagger.Services;
    using Nancy.TinyIoc;
    using IDPJobManager.Core;
    using IDPJobManager.Web.Features;
    using System.Linq;
    using Quartz;

    internal class IDPJobManagerBootstrapper : DefaultNancyBootstrapper
    {

        private readonly IScheduler _scheduler;

        static IDPJobManagerBootstrapper()
        {
            JsonSettings.MaxJsonLength = int.MaxValue;
            JsonSettings.RetainCasing = true;
            StaticConfiguration.EnableRequestTracing = true;
            StaticConfiguration.Caching.EnableRuntimeViewDiscovery = true;
            StaticConfiguration.Caching.EnableRuntimeViewUpdates = true;
            StaticConfiguration.DisableErrorTraces = false;
        }

        public IDPJobManagerBootstrapper(IScheduler scheduler)
        {
            if (scheduler == null)
                throw new ArgumentNullException("scheduler");
            _scheduler = scheduler;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register(_scheduler);
            RegisterIViewProjections(container);
            RegisterICommandInvoker(container);
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            SwaggerMetadataProvider.SetInfo("IDP Job APIs", "v0.1", "Our job service", new Contact()
            {
                EmailAddress = "exampleEmail@example.com"
            });

            pipelines.AfterRequest.AddItemToEndOfPipeline(x => x.Response.Headers.Add("Access-Control-Allow-Origin", "*"));

            CookieBasedSessions.Enable(pipelines);

            pipelines.EnableBasicAuthentication(new BasicAuthenticationConfiguration(
                container.Resolve<IUserValidator>(),
                "IDPJobManager"));

            ResourceViewLocationProvider.RootNamespaces.Add(Assembly.GetExecutingAssembly(), "IDPJobManager.Web.Views");

            ResourceViewLocationProvider.Ignore.Add(typeof(Nancy.ViewEngines.Razor.RazorViewEngine).Assembly);
        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);
            nancyConventions.StaticContentsConventions.AddDirectory("swagger-ui", "Content/swagger-ui/dist");
        }

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get
            {
                var res = base.InternalConfiguration;
                res.ViewLocationProvider = typeof(ResourceViewLocationProvider);
                return res;
            }
        }

        public static void RegisterICommandInvoker(TinyIoCContainer container)
        {
            var commandInvokerTypes = Assembly.GetAssembly(typeof(ICommandInvoker<,>))
                                              .DefinedTypes
                                              .Select(t => new
                                              {
                                                  Type = t.AsType(),
                                                  Interface = t.ImplementedInterfaces.FirstOrDefault(
                                                      i =>
                                                      i.IsGenericType() &&
                                                      i.GetGenericTypeDefinition() == typeof(ICommandInvoker<,>))
                                              })
                                              .Where(t => t.Interface != null)
                                              .ToArray();

            foreach (var commandInvokerType in commandInvokerTypes)
            {
                container.Register(commandInvokerType.Interface, commandInvokerType.Type);
            }

            container.Register(typeof(ICommandInvokerFactory), (cContainer, overloads) => new CommandInvokerFactory(cContainer));
        }

        public static void RegisterIViewProjections(TinyIoCContainer container)
        {
            var viewProjectionTypes = Assembly.GetAssembly(typeof(IViewProjection<,>))
                                              .DefinedTypes
                                              .Select(t => new
                                              {
                                                  Type = t.AsType(),
                                                  Interface = t.ImplementedInterfaces.FirstOrDefault(
                                                                       i =>
                                                                       i.IsGenericType() &&
                                                                       i.GetGenericTypeDefinition() == typeof(IViewProjection<,>))
                                              })
                                              .Where(t => t.Interface != null)
                                              .ToArray();

            foreach (var viewProjectionType in viewProjectionTypes)
            {
                container.Register(viewProjectionType.Interface, viewProjectionType.Type);
            }

            container.Register(typeof(IViewProjectionFactory), (cContainer, overloads) => new ViewProjectionFactory(cContainer));
        }
    }
}
