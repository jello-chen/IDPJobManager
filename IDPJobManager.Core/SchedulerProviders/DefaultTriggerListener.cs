using IDPJobManager.Core.Extensions;
using IDPJobManager.Core.Utils;
using Quartz;
using System;
using System.Threading.Tasks;

namespace IDPJobManager.Core.SchedulerProviders
{
    public class DefaultTriggerListener : ITriggerListener
    {
        public string Name { get { return "Default trigger listener"; } }

        public void TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode)
        {
            Logger.Instance.InfoFormat($"{context.JobDetail.Key.Name} fired,{context.NextFireTimeUtc.Value.DateTime}");
            JobOperator.UpdateNextFireTimeAsync(Guid.Parse(context.JobDetail.Key.Name), TimeZoneInfo.ConvertTimeFromUtc(context.NextFireTimeUtc.Value.DateTime, TimeZoneInfo.Local)).Wait();
        }

        public void TriggerFired(ITrigger trigger, IJobExecutionContext context)
        {
            Logger.Instance.InfoFormat($"{context.JobDetail.Key.Name} fired.");
        }

        public void TriggerMisfired(ITrigger trigger)
        {
            Logger.Instance.InfoFormat($"{trigger.JobKey.Name} misfired.");
        }

        public bool VetoJobExecution(ITrigger trigger, IJobExecutionContext context)
        {
            Logger.Instance.InfoFormat($"{context.JobDetail.Key.Name} executing,{context.FireTimeUtc.Value.DateTime}");
            JobOperator.UpdateRecentRunTimeAsync(Guid.Parse(context.JobDetail.Key.Name), TimeZoneInfo.ConvertTimeFromUtc(context.FireTimeUtc.Value.DateTime, TimeZoneInfo.Local)).Wait();
            return false;
        }
    }
}
