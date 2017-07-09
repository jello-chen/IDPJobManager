using Quartz;

namespace IDPJobManager.Core
{
    public interface IAspectableJob : IJob
    {
        bool BeforeExecute(IJobExecutionContext context);
        void AfterExecute(IJobExecutionContext context);
    }
}
