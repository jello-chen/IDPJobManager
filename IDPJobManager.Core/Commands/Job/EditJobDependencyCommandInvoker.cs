using IDPJobManager.Core.Domain;
using IDPJobManager.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDPJobManager.Core.Commands.Job
{
    public class EditJobDependencyCommandInvoker : ICommandInvoker<EditJobDependencyCommand, CommandResult>
    {
        public CommandResult Execute(EditJobDependencyCommand command)
        {
            try
            {
                using (var dataContext = new IDPJobManagerDataContext())
                {
                    var list = dataContext.T_JobDependency.Where(t => t.JobID == command.ID).ToList();
                    if (list.Count > 0)
                        dataContext.T_JobDependency.RemoveRange(list);
                    if (command.DependentJobIDs != null && command.DependentJobIDs.Count > 0)
                        dataContext.T_JobDependency.AddRange(
                            command.DependentJobIDs.Select(t => new JobDependency
                            {
                                ID = Guid.NewGuid(),
                                JobID = command.ID,
                                DependentJobID = t
                            }));
                    dataContext.SaveChanges();
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

    public class EditJobDependencyCommand : JobCommand
    {
        public List<Guid> DependentJobIDs { get; set; }
    }
}
