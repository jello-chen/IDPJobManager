using IDPJobManager.Core.Domain;
using System;

namespace IDPJobManager.Core.Commands.Job
{
    public class AddJobCommandInvoker : ICommandInvoker<AddJobCommand, CommandResult>
    {

        public CommandResult Execute(AddJobCommand command)
        {
            using (var dataContext = new IDPJobManagerDataContext())
            {
                JobInfo job = new JobInfo();
                job.ID = Guid.NewGuid();
                job.JobName = command.JobName;
                job.AssemblyName = command.AssemblyName;
                job.ClassName = command.ClassName;
                job.CronExpression = command.CronExpression;

                DateTime startTime = DateTime.MinValue;
                DateTime endTime = DateTime.MinValue;
                if (DateTime.TryParse(command.StartTimeString, out startTime))
                    job.StartTime = startTime;
                if (DateTime.TryParse(command.EndTimeString, out endTime))
                    job.EndTime = endTime;

                job.CreatedTime = DateTime.Now;
                job.Status = 0;
                job.IsDelete = 0;

                dataContext.T_Job.Add(job);
                dataContext.SaveChanges();

                return CommandResult.SuccessResult; 
            }
        }
    }

    public class AddJobCommand : JobCommand
    {
        public string JobName { get; set; }
        public string AssemblyName { get; set; }
        public string ClassName { get; set; }
        public string CronExpression { get; set; }
        public string StartTimeString { get; set; }
        public string EndTimeString { get; set; }
    }
}
