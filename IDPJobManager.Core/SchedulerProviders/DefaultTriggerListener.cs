using IDPJobManager.Core.Extensions;
using IDPJobManager.Core.Utils;
using Quartz;
using System;

namespace IDPJobManager.Core.SchedulerProviders
{
    public class DefaultTriggerListener : ITriggerListener
    {
        public string Name { get { return "Default trigger listener"; } }

        public void TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode)
        {
            var jobKey = context.JobDetail.Key;
            //Logger.Instance.InfoFormat($"{jobKey.Name}--{jobKey.Group} completed,{context.NextFireTimeUtc.Value.DateTime}");
            JobOperator.UpdateNextFireTimeAsync(jobKey.Name, jobKey.Group, TimeZoneInfo.ConvertTimeFromUtc(context.NextFireTimeUtc.Value.DateTime, TimeZoneInfo.Local)).Wait();
        }

        public void TriggerFired(ITrigger trigger, IJobExecutionContext context)
        {
            //var jobKey = context.JobDetail.Key;
            //Logger.Instance.InfoFormat($"{jobKey.Name}--{jobKey.Group} fired.");
        }

        public void TriggerMisfired(ITrigger trigger)
        {
            //var jobKey = trigger.JobKey;
            //Logger.Instance.InfoFormat($"{jobKey.Name}--{jobKey.Group} misfired.");
        }

        public bool VetoJobExecution(ITrigger trigger, IJobExecutionContext context)
        {
            var jobKey = context.JobDetail.Key;
            //Logger.Instance.InfoFormat($"{jobKey.Name}--{jobKey.Group} executing,{context.FireTimeUtc.Value.DateTime}");
            var jobStatus = JobOperator.GetJobStatus(jobKey.Name, jobKey.Group);
            if (jobStatus == 0) return true;//When returns true, the job would't be executed.
            JobOperator.UpdateRecentRunTimeAsync(jobKey.Name, jobKey.Group, TimeZoneInfo.ConvertTimeFromUtc(context.FireTimeUtc.Value.DateTime, TimeZoneInfo.Local)).Wait();
            return false;
        }
    }
}
