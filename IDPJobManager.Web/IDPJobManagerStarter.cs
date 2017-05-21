namespace IDPJobManager.Web
{
    using System;
    using Nancy.Hosting.Self;
    using Quartz;
    using System.ComponentModel.Composition.Hosting;
    using System.ComponentModel.Composition;

    public class IDPJobManagerStarter
    {
        private Uri baseUri;

        internal static IScheduler Scheduler { get; private set; }
        private readonly AggregateCatalog catalog;
        private readonly CompositionContainer container;
        private NancyHost nancyHost;

        public Uri BaseUri { get { return baseUri; } }

        private IDPJobManagerStarter()
        {
            var catalog = new AggregateCatalog(new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory, "IDPJobManager*.dll"));
            container = new CompositionContainer(catalog, CompositionOptions.DisableSilentRejection | CompositionOptions.IsThreadSafe | CompositionOptions.ExportCompositionService);
            container.ComposeExportedValue(container);
        }

        public static IDPJobManagerStarter Configure
        {
            get
            {
                return new IDPJobManagerStarter();
            }
        }

        public IDPJobManagerStarter UsingScheduler(IScheduler scheduler)
        {
            Scheduler = scheduler;
            return this;
        }

        public IDPJobManagerStarter HostedOn(Uri baseUri)
        {
            this.baseUri = baseUri;
            return this;
        }
        public IDPJobManagerStarter HostedOn(string baseUri)
        {
            this.baseUri = new Uri(baseUri);
            return this;
        }

        public IDPJobManagerStarter HostedOnDefault()
        {
            baseUri = new Uri(Configuration.ConfigProvider.GetInstance(Scheduler).Uri);
            return this;
        }

        public NancyHost Start()
        {
            if (baseUri == null)
                throw new InvalidOperationException("Uri to host on is not specified");
            if (Scheduler == null)
                throw new InvalidOperationException("Scheduler is not specified");
            nancyHost = new NancyHost(
                new IDPJobManagerBootstrapper(container),
                new HostConfiguration
                {
                    UrlReservations = new UrlReservations
                    {
                        CreateAutomatically = true,
                    }
                },
                baseUri);
            nancyHost.Start();
            return nancyHost;
        }

    }
}