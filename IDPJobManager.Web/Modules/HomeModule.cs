namespace IDPJobManager.Web.Modules
{
    using Core;
    using IDPJobManager.Core.ViewProjections.Performance;
    using IDPJobManager.Core.ViewProjections.Job;
    using System;
    using IDPJobManager.Core.Extensions;

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
    }
}
