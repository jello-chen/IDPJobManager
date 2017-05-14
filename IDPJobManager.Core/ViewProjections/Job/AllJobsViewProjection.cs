using IDPJobManager.Core.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace IDPJobManager.Core.ViewProjections.Job
{
    [Export(typeof(IViewProjection<AllJobsBindingModel, AllJobsViewModel>))]
    public class AllJobsViewProjection : IViewProjection<AllJobsBindingModel, AllJobsViewModel>
    {
        private readonly IDPJobManagerDataContext dataContext;

        [ImportingConstructor]
        public AllJobsViewProjection(IDPJobManagerDataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public AllJobsViewModel Project(AllJobsBindingModel input)
        {
            var skip = (input.Page - 1) * input.Take;

            var jobs = (from j in dataContext.Set<JobInfo>()
                        where j.IsDelete == 0
                        orderby j.CreatedTime descending, j.ModifyTime descending
                        select j)
                        .Skip(skip)
                        .Take(input.Take + 1)
                        .ToList();

            var pagedJobs = jobs.Take(input.Take);
            var hasNextPage = jobs.Count > input.Take;

            return new AllJobsViewModel
            {
                Jobs = pagedJobs,
                Page = input.Page,
                HasNextPage = hasNextPage
            };
        }
    }

    public class AllJobsViewModel
    {
        public IEnumerable<JobInfo> Jobs { get; set; }
        public int Page { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPrevPage => Page > 1;
    }

    public class AllJobsBindingModel
    {
        public string JobName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Page { get; set; } = 1;
        public int Take { get; set; } = 10;
    }
}
