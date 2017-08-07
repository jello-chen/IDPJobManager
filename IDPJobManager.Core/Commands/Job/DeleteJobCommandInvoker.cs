using IDPJobManager.Core.Domain;
using Quartz;
using System.Linq;

namespace IDPJobManager.Core.Commands.Job
{
    public class DeleteJobCommandInvoker : ICommandInvoker<DeleteJobCommand, CommandResult>
    {
        private readonly IScheduler scheduler;

        public DeleteJobCommandInvoker(IScheduler scheduler)
        {
            this.scheduler = scheduler;
        }

        public CommandResult Execute(DeleteJobCommand command)
        {
            using (var dataContext = new IDPJobManagerDataContext())
            {
                var jobSets = dataContext.Set<JobInfo>();
                var job = jobSets.FirstOrDefault(j => j.ID == command.ID && j.IsDelete == 0);
                if (job != null)
                {
                    scheduler.DeleteJob(new JobKey(job.JobName, job.JobGroup));
                    job.IsDelete = 1;
                    dataContext.SaveChanges();
                }
                return CommandResult.SuccessResult; 
            }
        }
    }

    public class DeleteJobCommand : JobCommand
    {

    }
}
