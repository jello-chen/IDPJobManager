using IDPJobManager.Core.Domain;
using IDPJobManager.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace IDPJobManager.Core
{
    public class JobExecutor
    {
        public static void Execute(IEnumerable<JobInfo> jobInfos)
        {
            if (jobInfos.IsNullOrEmpty()) return;
            Task.WaitAll(jobInfos.Select(j => Task.Run(() => Execute(j))).ToArray());
        }

        public static void Execute(JobInfo jobInfo)
        {
            var jobType = Type.GetType($"{jobInfo.ClassName},{jobInfo.AssemblyName}", true, true);
            if (jobType.Is<BaseJob>())
            {
                var job = (BaseJob)Activator.CreateInstance(jobType);
                Task.Run(() => job.Execute(null)).Wait();
            }
        }
    }
}
