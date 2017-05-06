using IDPJobManager.Core.Domain;
using System;
using System.ComponentModel.Composition;
using System.Linq;

namespace IDPJobManager.Core.Commands.Job
{
    [Export(typeof(ICommandInvoker<DeleteJobCommand, CommandResult>))]
    public class DeleteJobCommandInvoker : ICommandInvoker<DeleteJobCommand, CommandResult>
    {
        public CommandResult Execute(DeleteJobCommand command)
        {
            using (var dataContext = new IDPJobManagerDataContext())
            {
                var jobSets = dataContext.Set<JobInfo>();
                var job = jobSets.FirstOrDefault(j => j.ID == command.ID);
                if(job != null)
                {
                    job.IsDelete = 1;
                    dataContext.SaveChanges();
                }
                return CommandResult.SuccessResult;
            }
        }
    }

    public class DeleteJobCommand
    {
        public Guid ID { get; set; }
    }
}
