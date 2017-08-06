using System;
using System.Linq;
using IDPJobManager.Web;
using Topshelf;
using IDPJobManager.Core.Extensions;
using IDPJobManager.Core;
using System.IO;
using IDPJobManager.Core.Config;

namespace IDPJobManager
{
    class Program
    {
        static int Main(string[] args)
        {
            return (int)HostFactory.Run(x =>
            {
                GlobalConfiguration.ConfigureDBConnectionString();

                x.UseLog4Net("log4net.config");

                x.UseAssemblyInfoForServiceInfo();

                x.Service<WebServer>(s =>
                {
                    s.ConstructUsing(name => new WebServer(new JobWatcher("Jobs", cl =>
                    {
                        var assemblyNames = cl.Select(c => Path.GetFileNameWithoutExtension(c)).ToList();
                        foreach (var assemblyName in assemblyNames)
                        {
                            var jobInfos = JobOperator.GetJobInfoList(assemblyName);
                            JobPoolManager.Instance.RemoveAll(jobInfos.Select(t => t.ID).ToList());
                            jobInfos.ForEach(jobInfo => JobPoolManager.Scheduler.ScheduleJob(jobInfo));
                        }
                    })));
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.RunAsLocalSystem();

                //var scheduler = JobPoolManager.Scheduler;

                //using (IDPJobManagerStarter.Configure
                //        .UsingScheduler(scheduler)
                //        .UsingJobWatcher(new JobWatcher("Jobs", cl =>
                //        {
                //            var assemblyNames = cl.Select(c => Path.GetFileNameWithoutExtension(c)).ToList();
                //            foreach (var assemblyName in assemblyNames)
                //            {
                //                var jobInfos = JobOperator.GetJobInfoList(assemblyName);
                //                JobPoolManager.Instance.RemoveAll(jobInfos.Select(t => t.ID).ToList());
                //                jobInfos.ForEach(jobInfo => scheduler.ScheduleJob(jobInfo));
                //            }
                //        }))
                //        .HostedOnDefault()
                //        .Start())
                //{
                //    AppDomain.MonitoringIsEnabled = true;
                //    Console.WriteLine($"Web host started on {IDPJobManagerStarter.Configure.BaseUri}.");
                //    scheduler.StartRunning().ScheduleJobsAsync().Wait();
                //    Console.Read();
                //}
            });
        }
    }
}
