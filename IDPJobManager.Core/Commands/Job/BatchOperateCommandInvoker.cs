using IDPJobManager.Core.Domain;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using IDPJobManager.Core.Extensions;

namespace IDPJobManager.Core.Commands.Job
{
    public class BatchOperateCommandInvoker : ICommandInvoker<BatchOperateJobCommand, CommandResult>
    {
        private readonly IScheduler scheduler;

        public BatchOperateCommandInvoker(IScheduler scheduler)
        {
            this.scheduler = scheduler;
        }

        public CommandResult Execute(BatchOperateJobCommand command)
        {
            using (var dataContext = new IDPJobManagerDataContext())
            {
                var jobIds = command.IDS.Select(t => Guid.Parse(t)).ToList();
                var jobSets = dataContext.Set<JobInfo>();
                var jobs = jobSets.Where(j => jobIds.Contains(j.ID)).ToList();
                if (!jobs.IsNullOrEmpty())
                {
                    switch (command.Type.Value)
                    {
                        case BatchOperateType.Start:
                            foreach (var job in jobs)
                            {
                                if (job.Status == 0)
                                {
                                    job.Status = 1;
                                    scheduler.ScheduleJob(job);
                                }
                            }
                            break;
                        case BatchOperateType.Stop:
                            foreach (var job in jobs)
                            {
                                if (job.Status == 1)
                                {
                                    scheduler.PauseJob(job);
                                    job.Status = 0;
                                }
                            }
                            break;
                        case BatchOperateType.Delete:
                            foreach (var job in jobs)
                            {
                                if(job.IsDelete == 0)
                                {
                                    scheduler.DeleteJob(new JobKey(job.JobName, job.JobGroup));
                                    job.IsDelete = 1;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    dataContext.SaveChanges();
                }
                return CommandResult.SuccessResult;
            }
        }
    }

    public class BatchOperateJobCommand
    {
        public List<string> IDS { get; set; }
        public BatchOperateType? Type { get; set; }
    }

    public enum BatchOperateType
    {
        Start,
        Stop,
        Delete
    }
}
