using IDPJobManager.Core.Domain;
using Quartz;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDPJobManager.Core.Extensions;

namespace IDPJobManager.Core.Commands.Job
{
    [Export(typeof(ICommandInvoker<StopJobCommand, CommandResult>))]
    public class StopJobCommandInvoker : ICommandInvoker<StopJobCommand, CommandResult>
    {

        private readonly IDPJobManagerDataContext dataContext;
        private readonly IScheduler scheduler;

        [ImportingConstructor]
        public StopJobCommandInvoker(IDPJobManagerDataContext dataContext, IScheduler scheduler)
        {
            this.dataContext = dataContext;
            this.scheduler = scheduler;
        }

        public CommandResult Execute(StopJobCommand command)
        {
            var jobSets = dataContext.Set<JobInfo>();
            var job = jobSets.FirstOrDefault(j => j.ID == command.ID);
            if (job != null)
            {
                if (scheduler.PauseJob(job.ID.ToString()))
                {
                    job.Status = 0;
                    dataContext.SaveChanges();
                }
            }
            return CommandResult.SuccessResult;
        }
    }

    public class StopJobCommand : JobCommand
    {

    }
}
