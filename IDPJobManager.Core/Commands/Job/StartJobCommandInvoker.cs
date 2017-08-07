using IDPJobManager.Core.Domain;
using Quartz;
using System.Linq;
using IDPJobManager.Core.Extensions;

namespace IDPJobManager.Core.Commands.Job
{
    public class StartJobCommandInvoker : ICommandInvoker<StartJobCommand, CommandResult>
    {
        private readonly IScheduler scheduler;

        public StartJobCommandInvoker(IScheduler scheduler)
        {
            this.scheduler = scheduler;
        }

        public CommandResult Execute(StartJobCommand command)
        {
            using (var dataContext = new IDPJobManagerDataContext())
            {
                var jobSets = dataContext.Set<JobInfo>();
                var job = jobSets.FirstOrDefault(j => j.ID == command.ID && j.Status == 0);
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
    }

    public class StartJobCommand : JobCommand
    {

    }
}
