using IDPJobManager.Core.Domain;
using IDPJobManager.Core.Extensions;
using IDPJobManager.Core.Utils;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IDPJobManager.Core
{
    public class JobExecutor
    {
        public static void Execute(IEnumerable<JobInfo> jobInfos)
        {
            foreach (var jobInfo in jobInfos)
            {
                Execute(jobInfo);
            }
        }

        public static void Execute(JobInfo jobInfo)
        {
            var assemblyScanner = new AssemblyScanner(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Jobs"));
            var jobType = assemblyScanner.GetType(jobInfo.AssemblyName, jobInfo.ClassName);
            if (jobType.Is<IJob>())
            {
                var job = (IJob)Activator.CreateInstance(jobType);
                Task.Run(() => job.Execute(null)).Wait();
            }
        }
    }
}
