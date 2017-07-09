using IDPJobManager.Core;
using IDPJobManager.Core.Domain;
using IDPJobManager.Core.ViewProjections.Performance;
using Nancy;
using Nancy.ModelBinding;

namespace IDPJobManager.Web.Modules
{
    public class PerformanceModule : BaseModule
    {
        private readonly IViewProjectionFactory viewProjectionFactory;
        private readonly ICommandInvokerFactory commandInvokerFactory;

        public PerformanceModule(
            IViewProjectionFactory viewProjectionFactory,
            ICommandInvokerFactory commandInvokerFactory) : base("/Performance")
        {
            this.viewProjectionFactory = viewProjectionFactory;
            this.commandInvokerFactory = commandInvokerFactory;

            Get["/"] = _ => View["List"];
            Get["/GetPerformanceList"] = _ => GetPerformanceList();
        }

        private dynamic GetPerformanceList()
        {
            var model = this.Bind<AllPerformancesBindingModel>();
            var vm = viewProjectionFactory.Get<AllPerformancesBindingModel, AllPerformancesViewModel>(model);
            return Response.AsJson(vm);
        }
    }
}
