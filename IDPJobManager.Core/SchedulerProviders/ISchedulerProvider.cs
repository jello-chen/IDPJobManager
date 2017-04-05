namespace IDPJobManager.Core.SchedulerProviders
{
    using Quartz;

    public interface ISchedulerProvider
    {
        string Uri { get; set; }

        /// <summary>
        /// Gets scheduler instance. Should return same instance on every call.
        /// </summary>
        IScheduler Scheduler { get; }
    }
}