namespace IDPJobManager.Web.Modules
{
    using Core;
    using Core.Commands.Job;
    using Core.ViewProjections.Job;
    using Nancy;
    using Nancy.Metadata.Modules;
    using Nancy.ModelBinding;
    using Nancy.Swagger;
    using Swagger.ObjectModel;

    public class JobModule : BaseModule
    {
        private readonly IViewProjectionFactory viewProjectionFactory;
        private readonly ICommandInvokerFactory commandInvokerFactory;

        public JobModule(IViewProjectionFactory viewProjectionFactory,
            ICommandInvokerFactory commandInvokerFactory) : base("/Job")
        {
            this.viewProjectionFactory = viewProjectionFactory;
            this.commandInvokerFactory = commandInvokerFactory;

            Get["/"] = _ => View["List"];
            Get["/GetJobList"] = _ => GetJobList();
            Get["/Get"] = _ => GetJob();
            Post["/Add"] = _ => AddJob(this.Bind<AddJobCommand>());
            Post["/Edit"] = _ => EditJob(this.Bind<EditJobCommand>());
            Post["/Delete"] = _ => DeleteJob(this.Bind<DeleteJobCommand>());
            Post["/Start"] = _ => StartJob(this.Bind<StartJobCommand>());
            Post["/Stop"] = _ => StopJob(this.Bind<StopJobCommand>());
        }

        private dynamic GetJob()
        {
            var model = this.Bind<JobBindingModel>();
            var vm = viewProjectionFactory.Get<JobBindingModel, JobViewModel>(model);
            return Response.AsJson(new { success = vm.Success, model = vm.Model });
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

        private dynamic AddJob(AddJobCommand command)
        {
            var commandResult = commandInvokerFactory.Handle<AddJobCommand, CommandResult>(command);
            return Response.AsJson(new { success = commandResult.Success, message = commandResult.GetErrors() });
        }

        private dynamic EditJob(EditJobCommand command)
        {
            var commandResult = commandInvokerFactory.Handle<EditJobCommand, CommandResult>(command);
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

    //public class JobMetadataModule: MetadataModule<PathItem>
    //{
    //    public JobMetadataModule(ISwaggerModelCatalog modelCatalog)
    //    {
    //        modelCatalog.AddModel<StartJobCommand>();
    //        Describe["StartJob"] = description => description.AsSwagger(
    //            with => with.Operation(
    //                op => op.OperationId("StartJob")
    //                        .Tag("Jobs")
    //                        .Summary("Starts a job")
    //                        .Description("This starts a specified job and returns the operation status")
    //                        .Response(r => r.Schema<dynamic>().Description("OK"))));
    //    }
    //}
}
