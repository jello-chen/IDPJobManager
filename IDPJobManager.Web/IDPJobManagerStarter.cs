namespace IDPJobManager.Web
{
    using System;
    using Nancy.Hosting.Self;
    using Quartz;
    using IDPJobManager.Core;

    public class IDPJobManagerStarter
    {
        public static IDPJobManagerStarter Configure = new IDPJobManagerStarter();
        internal static IScheduler Scheduler { get; private set; }
        private NancyHost nancyHost;
        private Uri baseUri;
        private JobWatcher watcher;


        public Uri BaseUri { get { return baseUri; } }

        public IDPJobManagerStarter UsingScheduler(IScheduler scheduler)
        {
            Scheduler = scheduler;
            return this;
        }

        public IDPJobManagerStarter UsingJobWatcher(JobWatcher watcher)
        {
            this.watcher = watcher;
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
            if (watcher == null)
                throw new InvalidOperationException("Job watcher is not specified");

            //Start the job watcher
            watcher?.Start();

            //Start the nancy host
            nancyHost = new NancyHost(
                new IDPJobManagerBootstrapper(Scheduler),
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