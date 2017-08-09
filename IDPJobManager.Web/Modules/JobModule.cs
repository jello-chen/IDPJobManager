namespace IDPJobManager.Web.Modules
{
    using Core;
    using Core.Commands.Job;
    using Core.ViewProjections.Job;
    using IDPJobManager.Web.Utils;
    using Nancy;
    using Nancy.ModelBinding;
    using System.Linq;

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
            Get["/GetJobDependency"] = _ => GetJobDependency();
            Post["/Add"] = _ => AddJob(this.Bind<AddJobCommand>());
            Post["/Edit"] = _ => EditJob(this.Bind<EditJobCommand>());
            Post["/Delete"] = _ => DeleteJob(this.Bind<DeleteJobCommand>());
            Post["/Start"] = _ => StartJob(this.Bind<StartJobCommand>());
            Post["/Stop"] = _ => StopJob(this.Bind<StopJobCommand>());
            Post["/SaveJobDependency"] = _ => SaveJobDependency(this.Bind<EditJobDependencyCommand>());
            Post["/BatchOperate"] = _ => BatchOperate(this.Bind<BatchOperateJobCommand>());
            Post["/Upload"] = _ => Upload();
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

        private dynamic GetJobDependency()
        {
            var model = this.Bind<JobDependencyBindingModel>();
            var vm = viewProjectionFactory.Get<JobDependencyBindingModel, JobDependencyViewModel>(model);
            return Response.AsJson(vm);
        }

        private dynamic SaveJobDependency(EditJobDependencyCommand command)
        {
            var commandResult = commandInvokerFactory.Handle<EditJobDependencyCommand, CommandResult>(command);
            return Response.AsJson(new { success = commandResult.Success, message = commandResult.GetErrors() });
        }

        private dynamic BatchOperate(BatchOperateJobCommand command)
        {
            var commandResult = commandInvokerFactory.Handle<BatchOperateJobCommand, CommandResult>(command);
            return Response.AsJson(new { success = commandResult.Success, message = commandResult.GetErrors() });
        }

        private dynamic Upload()
        {
            var files = Request.Files.ToList();
            if (files.Count != 1)
                return Response.AsJson(new { success = false, message = "Not upload any file." });

            using (var stream = files[0].Value)
            {
                if (!ZipUtil.IsRar(stream))
                    return Response.AsJson(new { success = false, message = "This is not rar file." });
                if (ZipUtil.UnRar(stream, "Jobs"))
                    return Response.AsJson(new { success = true, message = string.Empty });
                else
                    return Response.AsJson(new { success = false, message = "Upload failed." });
            }

        }
    }
}
