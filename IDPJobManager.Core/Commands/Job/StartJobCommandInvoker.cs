using IDPJobManager.Core.Domain;
using Quartz;
using System.ComponentModel.Composition;
using System.Linq;
using IDPJobManager.Core.Extensions;

namespace IDPJobManager.Core.Commands.Job
{
    [Export(typeof(ICommandInvoker<StartJobCommand, CommandResult>))]
    public class StartJobCommandInvoker : ICommandInvoker<StartJobCommand, CommandResult>
    {
        private readonly IDPJobManagerDataContext dataContext;
        private readonly IScheduler scheduler;

        [ImportingConstructor]
        public StartJobCommandInvoker(IDPJobManagerDataContext dataContext,IScheduler scheduler)
        {
            this.dataContext = dataContext;
            this.scheduler = scheduler;
        }

        public CommandResult Execute(StartJobCommand command)
        {
            var jobSets = dataContext.Set<JobInfo>();
            var job = jobSets.FirstOrDefault(j => j.ID == command.ID);
            if (job != null)
            {
                job.Status = 1;
                if (scheduler.ScheduleJob(job))
                {
                    dataContext.SaveChanges();
                }
            }
            return CommandResult.SuccessResult;
        }
    }

    public class StartJobCommand : JobCommand
    {

    }
}
