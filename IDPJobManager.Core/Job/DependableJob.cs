using IDPJobManager.Core.Extensions;
using Quartz;

namespace IDPJobManager.Core
{
    public abstract class DependableJob : PerformanceJob
    {
        public override bool BeforeExecute(IJobExecutionContext context)
        {
            base.BeforeExecute(context);
            //Gets the dependent job list and schedule them.
            var jobKey = context.JobDetail.Key;
            var dependentJobs = JobOperator.GetDependentJobInfoList(jobKey.Name, jobKey.Group);
            JobExecutor.Execute(dependentJobs);
            return true;
        }
    }
}
