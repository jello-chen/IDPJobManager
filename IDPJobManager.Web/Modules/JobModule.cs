namespace IDPJobManager.Web.Modules
{
    using Core;
    using Core.Commands.Job;
    using Core.ViewProjections.Job;
    using Nancy;
    using Nancy.ModelBinding;
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
            Post["/Delete"] = _ => DeleteJob(this.Bind<DeleteJobCommand>());
            Post["/Start"] = _ => StartJob(this.Bind<StartJobCommand>());
            Post["/Stop"] = _ => StopJob(this.Bind<StopJobCommand>());
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

        private dynamic StartJob(StartJobCommand command)
        {
            var commandResult = commandInvokerFactory.Handle<StartJobCommand, CommandResult>(command);
            return Response.AsJson(new { success = commandResult.Success, message = commandResult.GetErrors() });
        }

        private dynamic StopJob(StopJobCommand command)
        {
            var commandResult = commandInvokerFactory.Handle<StopJobCommand, CommandResult>(command);
            return Response.AsJson(new { success = commandResult.Success, message = commandResult.GetErrors() });
        }
    }
}
