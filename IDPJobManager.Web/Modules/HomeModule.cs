namespace IDPJobManager.Web.Modules
{
    using Core;
    using IDPJobManager.Core.ViewProjections.Performance;
    using IDPJobManager.Core.ViewProjections.Job;
    using System;
    using IDPJobManager.Core.Extensions;
    using System.Linq;
    using System.Collections.Generic;
    using Nancy;

    public class HomeModule : BaseModule
    {

        private readonly IViewProjectionFactory viewProjectionFactory;
        private readonly ICommandInvokerFactory commandInvokerFactory;

        public HomeModule(IViewProjectionFactory viewProjectionFactory,
            ICommandInvokerFactory commandInvokerFactory)
            : base()
        {
            this.viewProjectionFactory = viewProjectionFactory;
            this.commandInvokerFactory = commandInvokerFactory;

            Get["/"] = _ =>
            {
                ViewBag.NewJobCount = GetNewJobCount();
                ViewBag.NewPerformanceCount = GetNewPerformanceCount();
                return View["Index"];
            };

            Get["/Home/GetJobPerformanceTrend"] = _ => GetJobPerformanceTrend();
        }

        private int GetNewJobCount()
        {
            var vm = viewProjectionFactory.Get<AllJobsBindingModel, AllJobsViewModel>(new AllJobsBindingModel { StartDate = DateTime.Now.GetStartDateTimeOfDay()});
            return vm.TotalCount;
        }

        private int GetNewPerformanceCount()
        {
            var vm = viewProjectionFactory.Get<AllPerformancesBindingModel, AllPerformancesViewModel>(new AllPerformancesBindingModel { StartDate = DateTime.Now.GetStartDateTimeOfDay() });
            return vm.TotalCount;
        }

        private dynamic GetJobPerformanceTrend()
        {
            var data = new List<Dictionary<string, dynamic>>();
            var performanceTrendList = JobOperator.GetPerformanceTrend();
            var dates = performanceTrendList.GroupBy(p => p.Date).OrderBy(p => p.Key).Select(p => p.Key).ToList();
            var jobs = performanceTrendList.GroupBy(p => p.Job).OrderBy(p => p.Key).Select(p => p.Key).ToList();
            foreach (var date in dates)
            {
                var dict = new Dictionary<string, dynamic>();
                dict.Add("period", date);
                foreach (var job in jobs)
                {
                    var jm = performanceTrendList.FirstOrDefault(p => p.Date == date && p.Job == job);
                    dict.Add(job, jm == null ? 0 : jm.ElapsedTime);
                }
                data.Add(dict);
            }
            return Response.AsJson(new { data = data, ykeys = jobs });
        }
    }
}
