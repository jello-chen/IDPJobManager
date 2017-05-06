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
    public class JobModule : NancyModule
    {
        private readonly IViewProjectionFactory viewProjectionFactory;
        private readonly ICommandInvokerFactory commandInvokerFactory;

        [ImportingConstructor]
        public JobModule(IViewProjectionFactory viewProjectionFactory, 
            ICommandInvokerFactory commandInvokerFactory) : base("/Job")
        {
            this.viewProjectionFactory = viewProjectionFactory;
            this.commandInvokerFactory = commandInvokerFactory;

            Get["/{page?1}"] = _ => ShowJobs(_.page);
            Get["/Delete/{id:guid}"] = _ => DeleteJob(this.Bind<DeleteJobCommand>());
        }


        private Negotiator ShowJobs(int page)
        {
            var model = viewProjectionFactory.Get<AllJobsBindingModel, AllJobsViewModel>(new AllJobsBindingModel
            {
                Page = page,
                Take = 20
            });
            return View["List", model];
        }

        private dynamic DeleteJob(DeleteJobCommand command)
        {
            commandInvokerFactory.Handle<DeleteJobCommand, CommandResult>(command);
            string returnURL = Request.Headers.Referrer;
            return Response.AsRedirect(returnURL);
        }
    }
}
