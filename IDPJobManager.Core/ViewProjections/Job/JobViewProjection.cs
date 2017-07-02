using IDPJobManager.Core.Domain;
using System;
using System.Linq;

namespace IDPJobManager.Core.ViewProjections.Job
{
    public class JobViewProjection : IViewProjection<JobBindingModel, JobViewModel>
    {

        public JobViewModel Project(JobBindingModel input)
        {
            using (var dataContext = new IDPJobManagerDataContext())
            {
                var model = dataContext.Set<JobInfo>().FirstOrDefault(t => t.ID == input.ID);
                return new JobViewModel { Success = true, Model = model }; 
            }
        }
    }

    public class JobBindingModel
    {
        public Guid ID { get; set; }
    }
    public class JobViewModel
    {
        public bool Success { get; set; }
        public JobInfo Model { get; set; }
    }
}
