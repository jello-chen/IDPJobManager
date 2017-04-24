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
    using IDPJobManager.Bootstrapper.Mef;
    using System.ComponentModel.Composition.Hosting;

    internal class IDPJobManagerBootstrapper : MefNancyBootstrapper
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

        protected override void ApplicationStartup(CompositionContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            CookieBasedSessions.Enable(pipelines);

            pipelines.EnableBasicAuthentication(new BasicAuthenticationConfiguration(
                container.GetExportedValue<IUserValidator>(),
                "IDPJobManager"));

            ResourceViewLocationProvider.RootNamespaces.Add(Assembly.GetExecutingAssembly(), "IDPJobManager.Web.Views");

            ResourceViewLocationProvider.Ignore.Add(typeof(Nancy.ViewEngines.Razor.RazorViewEngine).Assembly);
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
    }
}
