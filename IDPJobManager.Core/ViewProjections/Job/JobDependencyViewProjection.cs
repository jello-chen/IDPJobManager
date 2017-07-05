using IDPJobManager.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDPJobManager.Core.ViewProjections.Job
{
    public class JobDependencyViewProjection : IViewProjection<JobDependencyBindingModel, JobDependencyViewModel>
    {
        public JobDependencyViewModel Project(JobDependencyBindingModel input)
        {
            var vm = new JobDependencyViewModel();
            using (var dataContext = new IDPJobManagerDataContext())
            {
                vm.DependentJobList = (from j1 in dataContext.T_JobDependency
                                       join j2 in dataContext.T_Job
                                       on j1.DependentJobID equals j2.ID into g
                                       from g1 in g.DefaultIfEmpty()
                                       select new { ID = g1.ID, JobName = g1.JobName }).ToList();

                vm.DependableJobList = dataContext.Database.SqlQuery<JobInfo>(
                             $@"SELECT * FROM [dbo].[T_Job] 
                                WHERE IsDelete = 0 
                                  AND ID <> '{input.ID}' 
                                  AND ID NOT IN (
                                         SELECT DependentJobID FROM [dbo].[T_JobDependency] 
                                          WHERE JobID = '{input.ID}')").Select(j => new { ID = j.ID, JobName = j.JobName }).ToList();
                return vm;
            }
        }
    }

    public class JobDependencyBindingModel
    {
        public Guid ID { get; set; }
    }

    public class JobDependencyViewModel
    {
        public dynamic DependentJobList { get; set; }
        public dynamic DependableJobList { get; set; }
    }
}
