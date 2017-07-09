using IDPJobManager.Core.Domain;
using System;
using System.Linq;

namespace IDPJobManager.Core.Commands.Job
{
    public class AddJobCommandInvoker : ICommandInvoker<AddJobCommand, CommandResult>
    {

        public CommandResult Execute(AddJobCommand command)
        {
            using (var dataContext = new IDPJobManagerDataContext())
            {
                if (dataContext.T_Job.Any(j => j.JobName == command.JobName && j.JobGroup == command.JobGroup))
                    return new CommandResult("The job name and group already exist");

                JobInfo job = new JobInfo();
                job.ID = Guid.NewGuid();
                job.JobName = command.JobName;
                job.AssemblyName = command.AssemblyName;
                job.ClassName = command.ClassName;
                job.CronExpression = command.CronExpression;
                if (!string.IsNullOrWhiteSpace(command.JobGroup))
                    job.JobGroup = command.JobGroup;
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
        public string JobGroup { get; set; }
        public string AssemblyName { get; set; }
        public string ClassName { get; set; }
        public string CronExpression { get; set; }
        public string StartTimeString { get; set; }
        public string EndTimeString { get; set; }
    }
}
