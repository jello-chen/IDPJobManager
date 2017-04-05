namespace IDPJobManager.Core.SchedulerProviders
{
    using Quartz;

    public class StdSchedulerProvider : ISchedulerProvider
    {
        public StdSchedulerProvider(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        private IScheduler _scheduler;

        public string Uri { get; set; }

        public IScheduler Scheduler
        {
            get
            {
                return _scheduler;
            }
        }
    }
}