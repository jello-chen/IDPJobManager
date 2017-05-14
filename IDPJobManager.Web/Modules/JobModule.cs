namespace IDPJobManager.Web.Modules
{
    using IDPJobManager.Core;
    using IDPJobManager.Core.Commands.Job;
    using IDPJobManager.Core.ViewProjections.Job;
    using Nancy;
    using Nancy.ModelBinding;
    using Nancy.Responses.Negotiation;
    using System.ComponentModel.Composition;

    [Export(typeof(INancyModule))]
    public class JobModule : BaseModule
    {
        private readonly IViewProjectionFactory viewProjectionFactory;
        private readonly ICommandInvokerFactory commandInvokerFactory;

        [ImportingConstructor]
        public JobModule(IViewProjectionFactory viewProjectionFactory,
            ICommandInvokerFactory commandInvokerFactory) : base("/Job")
        {
            this.viewProjectionFactory = viewProjectionFactory;
            this.commandInvokerFactory = commandInvokerFactory;

            Get["/"] = _ => View["List"];
            Get["/GetJobList"] = _ => GetJobList();
            Get["/Delete/{id:guid}"] = _ => DeleteJob(this.Bind<DeleteJobCommand>());
        }


        private dynamic GetJobList()
        {
            var model = this.Bind<AllJobsBindingModel>();
            var vm = viewProjectionFactory.Get<AllJobsBindingModel, AllJobsViewModel>(model);
            return Response.AsJson(vm);
        }

        private dynamic DeleteJob(DeleteJobCommand command)
        {
            var commandResult = commandInvokerFactory.Handle<DeleteJobCommand, CommandResult>(command);
            return Response.AsJson(new { success = commandResult.Success, message = commandResult.GetErrors() });
        }
    }
}
