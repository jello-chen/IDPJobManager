﻿using IDPJobManager.Core.Domain;
using Quartz;
using System.ComponentModel.Composition;
using System.Linq;

namespace IDPJobManager.Core.Commands.Job
{
    [Export(typeof(ICommandInvoker<DeleteJobCommand, CommandResult>))]
    public class DeleteJobCommandInvoker : ICommandInvoker<DeleteJobCommand, CommandResult>
    {
        private readonly IDPJobManagerDataContext dataContext;
        private readonly IScheduler scheduler;

        [ImportingConstructor]
        public DeleteJobCommandInvoker(IDPJobManagerDataContext dataContext,IScheduler scheduler)
        {
            this.dataContext = dataContext;
            this.scheduler = scheduler;
        }

        public CommandResult Execute(DeleteJobCommand command)
        {
            var jobSets = dataContext.Set<JobInfo>();
            var job = jobSets.FirstOrDefault(j => j.ID == command.ID);
            if (job != null)
            {
                if (scheduler.DeleteJob(new JobKey(command.ID.ToString())))
                {
                    job.IsDelete = 1;
                    dataContext.SaveChanges();
                }
            }
            return CommandResult.SuccessResult;
        }
    }

    public class DeleteJobCommand : JobCommand
    {

    }
}
