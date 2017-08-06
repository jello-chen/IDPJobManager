using IDPJobManager.Core.Extensions;

namespace IDPJobManager.Core
{
    public abstract class DependableJob : PerformanceJob
    {
        public override bool BeforeExecute(JobExecutionContext context)
        {
            base.BeforeExecute(context);
            //Gets the dependent job list and schedule them.
            var dependentJobs = JobOperator.GetDependentJobInfoList(context.JobName, context.JobGroup);
            JobExecutor.Execute(dependentJobs);
            return true;
        }
    }
}
