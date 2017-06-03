using IDPJobManager.Core.Domain;
using System;
using System.ComponentModel.Composition;
using System.Linq;

namespace IDPJobManager.Core.ViewProjections.Job
{
    [Export(typeof(IViewProjection<JobBindingModel, JobViewModel>))]
    public class JobViewProjection : IViewProjection<JobBindingModel, JobViewModel>
    {

        private readonly IDPJobManagerDataContext dataContext;

        [ImportingConstructor]
        public JobViewProjection(IDPJobManagerDataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public JobViewModel Project(JobBindingModel input)
        {
            var model = dataContext.Set<JobInfo>().FirstOrDefault(t => t.ID == input.ID);
            return new JobViewModel { Success = true, Model = model };
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
