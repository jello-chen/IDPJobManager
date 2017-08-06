namespace IDPJobManager.Web.Configuration
{
    using System;
    using Core.SchedulerProviders;

    public sealed class ConfigProvider
    {
        static ConfigProvider instance = null;
        static readonly object padlock = new object();

        ISchedulerProvider schedulerProvider;

        ProviderElement provider;

        ConfigProvider(Quartz.IScheduler scheduler)
        {
            provider = IDPJobManagerConfiguration.Config.Provider;

            schedulerProvider = new StdSchedulerProvider(scheduler)
            {
                Uri = provider.Uri
            };
        }

        public ISchedulerProvider SchedulerProvider
        {
            get { return schedulerProvider; }
        }

        public string Uri
        {
            get { return provider.Uri; }
        }

        public dynamic AuthenticationInfo
        {
            get
            {
                return new { Validate = provider.Authentication.Validate, User = provider.Authentication.User, Password = provider.Authentication.Password };
            }
        }

        public static ConfigProvider GetInstance(Quartz.IScheduler scheduler)
        {
            lock (padlock)
            {
                return instance ?? (instance = new ConfigProvider(scheduler));
            }
        }
    }
}