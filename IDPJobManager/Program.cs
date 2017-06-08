using System;
using Quartz.Impl;
using IDPJobManager.Web;
using Quartz;
using System.Configuration;
using Topshelf;
using IDPJobManager.Core.Extensions;

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
                    scheduler.StartRunning().ScheduleJobsAsync().Wait();
                    Console.WriteLine($"Web host started on {IDPJobManagerStarter.Configure.BaseUri}.");
                    Console.Read();
                }
  
            });
            

            //var jobTrigger = TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(10).WithRepeatCount(int.MaxValue)).Build();
            //var jobKey = JobKey.Create("job1", "jobgroup1");
            //IJobDetail jobDetail = scheduler.GetJobDetail(jobKey);
            //if (jobDetail == null)
            //{
            //    jobDetail = JobBuilder.Create<HelloJob>().WithIdentity(jobKey).Build();
            //    scheduler.ScheduleJob(jobDetail, jobTrigger);
            //}
            //else
            //{
            //    scheduler.ResumeJob(jobKey);
            //}
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
            return schedulerFactory.GetScheduler();
        }
    }
}
