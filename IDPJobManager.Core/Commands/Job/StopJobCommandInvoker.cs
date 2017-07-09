using IDPJobManager.Core.Domain;
using Quartz;
using System.Linq;
using IDPJobManager.Core.Extensions;

namespace IDPJobManager.Core.Commands.Job
{
    public class StopJobCommandInvoker : ICommandInvoker<StopJobCommand, CommandResult>
    {

        private readonly IScheduler scheduler;

        public StopJobCommandInvoker(IScheduler scheduler)
        {
            this.scheduler = scheduler;
        }

        public CommandResult Execute(StopJobCommand command)
        {
            using (var dataContext = new IDPJobManagerDataContext())
            {
                var jobSets = dataContext.Set<JobInfo>();
                var job = jobSets.FirstOrDefault(j => j.ID == command.ID);
                if (job != null)
                {
                    if (scheduler.PauseJob(job))
                    {
                        job.Status = 0;
                        dataContext.SaveChanges();
                    }
                }
                return CommandResult.SuccessResult;
            }
        }
    }

    public class StopJobCommand : JobCommand
    {

    }
}
