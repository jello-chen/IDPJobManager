namespace IDPJobManager.Core
{
    public interface IAspectableJob
    {
        bool BeforeExecute(JobExecutionContext context);
        void AfterExecute(JobExecutionContext context);
    }
}
