using System;
using Quartz.Impl;
using IDPJobManager.Web;
using Quartz;
using System.Configuration;
using Topshelf;
using IDPJobManager.Core.Extensions;
using IDPJobManager.Core.SchedulerProviders;
using Quartz.Impl.Matchers;

namespace IDPJobManager
{
    class Program
    {
        static int Main(string[] args)
        {
            return (int)HostFactory.Run(x =>
            {
                ConfigureDBConnectionString();

                x.UseLog4Net("log4net.config");

                x.UseAssemblyInfoForServiceInfo();

                x.EnablePauseAndContinue();

                var scheduler = CreateScheduler();

                using (IDPJobManagerStarter.Configure.UsingScheduler(scheduler)
                        .HostedOnDefault()
                        .Start())
                {
                    Console.WriteLine($"Web host started on {IDPJobManagerStarter.Configure.BaseUri}.");
                    scheduler.StartRunning().ScheduleJobsAsync().Wait();
                    Console.Read();
                }
            });
        }

        static void ConfigureDBConnectionString()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["IDP-JobManager"];
            if (connectionString == null)
                throw new InvalidOperationException("Not configure `IDP-JobManager` connection string.");
            Core.Config.GlobalConfiguration.Name = "IDP-JobManager";
            Core.Config.GlobalConfiguration.ConnectionString = connectionString.ConnectionString;
            Core.Config.GlobalConfiguration.ProviderName = connectionString.ProviderName;
        }

        static IScheduler CreateScheduler()
        {
            var schedulerFactory = new StdSchedulerFactory();
            var scheduler = schedulerFactory.GetScheduler();
            scheduler.ListenerManager.AddTriggerListener(new DefaultTriggerListener(), GroupMatcher<TriggerKey>.AnyGroup());
            return scheduler;
        }
    }
}
