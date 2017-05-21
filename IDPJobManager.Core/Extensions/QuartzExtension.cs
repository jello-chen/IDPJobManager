using IDPJobManager.Core.Domain;
using IDPJobManager.Core.Utils;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDPJobManager.Core.Extensions
{
    public static class QuartzExtension
    {

        public static bool ScheduleJob(this IScheduler scheduler, JobInfo jobInfo)
        {
            var assemblyScanner = new AssemblyScanner(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Jobs"));
            var jobType = assemblyScanner.GetType(jobInfo.AssemblyName, jobInfo.ClassName);
            var jobIdentity = jobInfo.ID.ToString();
            var jobDetail = new JobDetailImpl(jobIdentity, jobType);
            var jobTrigger = new CronTriggerImpl();
            jobTrigger.CronExpressionString = jobInfo.CronExpression;
            jobTrigger.Name = jobIdentity;
            jobTrigger.Description = jobInfo.JobName;
            jobTrigger.StartTimeUtc = jobInfo.StartTime;
            jobTrigger.EndTimeUtc = jobInfo.EndTime;
            scheduler.ScheduleJob(jobDetail, jobTrigger);
            if (jobInfo.Status == 0) scheduler.PauseJob(new JobKey(jobIdentity));
            return true;
        }

        public static bool DeleteJob(this IScheduler scheduler, string jobKey)
        {
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));
            var _jobKey = new JobKey(jobKey);
            if (scheduler.CheckExists(_jobKey))
            {
                return scheduler.DeleteJob(_jobKey);
            }
            return true;
        }

    }
}
