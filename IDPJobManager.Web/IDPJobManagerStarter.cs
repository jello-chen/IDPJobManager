namespace IDPJobManager.Web
{
    using System;
    using Nancy.Hosting.Self;
    using Quartz;

    public class IDPJobManagerStarter
    {
        private Uri uri;

        internal static IScheduler Scheduler { get; private set; }

        private IDPJobManagerStarter()
        {
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

        public IDPJobManagerStarter HostedOn(Uri uri)
        {
            this.uri = uri;
            return this;
        }
        public IDPJobManagerStarter HostedOn(string uri)
        {
            this.uri = new Uri(uri);
            return this;
        }

        public IDPJobManagerStarter HostedOnDefault()
        {
            uri = new Uri(Configuration.ConfigProvider.GetInstance(Scheduler).Uri);
            return this;
        }

        public NancyHost Start()
        {
            if (uri == null)
                throw new InvalidOperationException("Uri to host on is not specified");
            if (Scheduler == null)
                throw new InvalidOperationException("Scheduler is not specified");

            return IDPJobManagerBootstrapper.Start(uri);
        }

    }
}