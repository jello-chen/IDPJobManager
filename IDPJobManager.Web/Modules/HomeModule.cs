namespace IDPJobManager.Web.Modules
{
    using Core;
    using Core.SchedulerProviders;
    using Configuration;
    using System.Linq;
    using Core.Domain;
    using Nancy;

    public class HomeModule : BaseModule
    {
        private static readonly ISchedulerDataProvider SchedulerDataProvider;

        static HomeModule()
        {
            ISchedulerProvider schedulerProvider = ConfigProvider.GetInstance(IDPJobManagerStarter.Scheduler).SchedulerProvider;

            SchedulerDataProvider = new DefaultSchedulerDataProvider(schedulerProvider);
        }

        public HomeModule()
            : base()
        {
            Get["/"] = _ =>
            {
                ViewBag["SelfVersion"] = GetType().Assembly.GetName().Version.ToString();
                ViewBag["QuartzVersion"] = typeof(Quartz.Impl.StdScheduler).Assembly.GetName().Version.ToString();
                return View["Index", SchedulerDataProvider.Data];
            };

            Get["/jobdetails/{job}/{group}"] = _ =>
            {
                var job = (string)_.job;
                var group = (string)_.group;

                var detailsData = SchedulerDataProvider.GetJobDetailsData(job, group);

                var jobDataMap = detailsData
                    .JobDataMap
                    .Select(pair => new Property(pair.Key.ToString(), pair.Value))
                    .ToArray();

                var jobProperties = detailsData
                    .JobProperties
                    .Select(pair => new Property(pair.Key, pair.Value))
                    .ToArray();

                return View["Content_Job_Details.haml", new { datamaps = jobDataMap, properties = jobProperties }];
            };
        }
    }
}
