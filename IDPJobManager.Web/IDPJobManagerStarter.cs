namespace IDPJobManager.Web
{
    using System;
    using Nancy.Hosting.Self;
    using Quartz;
    using IDPJobManager.Core;
    using Owin;

    public class IDPJobManagerStarter
    {
        internal static IScheduler Scheduler { get; private set; }
        private NancyHost nancyHost;
        private Uri baseUri;
        private JobWatcher jobWatcher;
        public bool IsStart { get; private set; }

        public void Configuration(IAppBuilder app)
        {

        }


        public Uri BaseUri { get { return baseUri; } }

        public IDPJobManagerStarter UsingScheduler(IScheduler scheduler)
        {
            Scheduler = scheduler;
            return this;
        }

        public IDPJobManagerStarter UsingJobWatcher(JobWatcher jobWatcher)
        {
            this.jobWatcher = jobWatcher;
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
            //baseUri = new Uri(Configuration.ConfigProvider.GetInstance(Scheduler).Uri);
            return this;
        }

        public void Start()
        {
            if (baseUri == null)
                throw new InvalidOperationException("Uri to host on is not specified");
            if (Scheduler == null)
                throw new InvalidOperationException("Scheduler is not specified");
            if (jobWatcher == null)
                throw new InvalidOperationException("Job watcher is not specified");

            if(!IsStart)
            {
                //Start the job watcher
                jobWatcher?.Start();

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
                IsStart = true;
            }
        }

        public void Stop()
        {
            if(IsStart)
            {
                jobWatcher.Stop();
                nancyHost.Stop();
            }
        }
    }
}