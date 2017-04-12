namespace IDPJobManager.Web
{
    using System;
    using System.Reflection;
    using Nancy.Bootstrapper;
    using Nancy.Json;
    using Nancy.ViewEngines;
    using Nancy;
    using Nancy.Authentication.Basic;
    using Nancy.TinyIoc;
    using Nancy.Hosting.Self;
    using Nancy.Session;

    internal class IDPJobManagerBootstrapper : DefaultNancyBootstrapper
    {
        static IDPJobManagerBootstrapper()
        {
            JsonSettings.MaxJsonLength = int.MaxValue;
            StaticConfiguration.EnableRequestTracing = true;
            StaticConfiguration.Caching.EnableRuntimeViewDiscovery = true;
            StaticConfiguration.Caching.EnableRuntimeViewUpdates = true;
            StaticConfiguration.DisableErrorTraces = false;
        }

        internal static NancyHost Start(Uri hostUrl)
        {
            var defaultNancyBootstrapper = new IDPJobManagerBootstrapper();
            var nancyHost = new NancyHost(hostUrl, defaultNancyBootstrapper);
            nancyHost.Start();

            return nancyHost;
        }

        
  
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            CookieBasedSessions.Enable(pipelines);

            pipelines.EnableBasicAuthentication(new BasicAuthenticationConfiguration(
                container.Resolve<IUserValidator>(),
                "IDPJobManager"));

            ResourceViewLocationProvider.RootNamespaces.Add(Assembly.GetExecutingAssembly(), "IDPJobManager.Web.Views");

            ResourceViewLocationProvider.Ignore.Add(typeof(Nancy.ViewEngines.Razor.RazorViewEngine).Assembly); 
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

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

        void OnConfigurationBuilder(NancyInternalConfiguration x)
        {
            x.ViewLocationProvider = typeof(ResourceViewLocationProvider);
        }
    }
}
