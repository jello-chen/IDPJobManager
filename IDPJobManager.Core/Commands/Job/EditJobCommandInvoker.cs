using IDPJobManager.Core.Domain;
using IDPJobManager.Core.Utils;
using System;
using System.Linq;

namespace IDPJobManager.Core.Commands.Job
{
    public class EditJobCommandInvoker : ICommandInvoker<EditJobCommand, CommandResult>
    {

        public CommandResult Execute(EditJobCommand command)
        {
            try
            {
                using (var dataContext = new IDPJobManagerDataContext())
                {
                    JobInfo job = dataContext.T_Job.FirstOrDefault(j => j.ID == command.ID);
                    if (job != null)
                    {
                        job.JobName = command.JobName;
                        job.AssemblyName = command.AssemblyName;
                        job.ClassName = command.ClassName;
                        job.CronExpression = command.CronExpression;

                        DateTime startTime = DateTime.MinValue;
                        DateTime endTime = DateTime.MinValue;
                        if (DateTime.TryParse(command.StartTimeString, out startTime))
                            job.StartTime = startTime;
                        else
                            job.StartTime = null;
                        if (DateTime.TryParse(command.EndTimeString, out endTime))
                            job.EndTime = endTime;
                        else
                            job.EndTime = null;

                        job.ModifyTime = DateTime.Now;

                        dataContext.SaveChanges();
                    }
                    return CommandResult.SuccessResult; 
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex.Message);
                return new CommandResult(ex.Message);
            }
        }
    }

    public class EditJobCommand : JobCommand
    {
        public string JobName { get; set; }
        public string AssemblyName { get; set; }
        public string ClassName { get; set; }
        public string CronExpression { get; set; }
        public string StartTimeString { get; set; }
        public string EndTimeString { get; set; }
    }
}