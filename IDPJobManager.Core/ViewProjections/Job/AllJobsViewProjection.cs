using IDPJobManager.Core.Domain;
using IDPJobManager.Core.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using IDPJobManager.Core.Extensions;

namespace IDPJobManager.Core.ViewProjections.Job
{
    public class AllJobsViewProjection : IViewProjection<AllJobsBindingModel, AllJobsViewModel>
    {

        public AllJobsViewModel Project(AllJobsBindingModel input)
        {

            using (var dataContext = new IDPJobManagerDataContext())
            {
                var query = from j in dataContext.Set<JobInfo>()
                            where j.IsDelete == 0
                            select j;

                if (!string.IsNullOrEmpty(input.JobName))
                    query = query.Where(j => j.JobName.Contains(input.JobName));

                if (input.StartDate != DateTime.MinValue)
                    query = query.Where(j => j.CreatedTime >= input.StartDate);

                if (input.EndDate != DateTime.MinValue)
                    query = query.Where(j => j.CreatedTime <= input.EndDate);

                var totalCount = query.Count();

                if (!string.IsNullOrEmpty(input.SortKey) && !string.IsNullOrEmpty(input.SortType))
                {
                    query = query.OrderBy(input.SortKey, input.SortType);
                }
                else
                {
                    query = query.OrderByDescending(j => j.CreatedTime).ThenByDescending(j => j.ModifyTime);
                }

                var jobs = query
                            .Skip((input.PageCurrent - 1) * input.PageSize)
                            .Take(input.PageSize)
                            .ToList();

                return new AllJobsViewModel
                {
                    Items = jobs,
                    TotalCount = totalCount
                }; 
            }
        }
    }

    public class AllJobsViewModel
    {
        public IEnumerable<JobInfo> Items { get; set; }
        public int TotalCount { get; set; }
    }

    public class AllJobsBindingModel : IPager, ISortor
    {
        public string JobName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        #region IPager implements
        private int pageCurrent;
        public int PageCurrent
        {
            get { return pageCurrent; }
            set { pageCurrent = value; }
        }

        private int pageSize;

        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = value; }
        }
        #endregion

        #region ISortor implements
        private string sortKey;

        public string SortKey
        {
            get { return sortKey; }
            set { sortKey = value; }
        }

        private string sortType;

        public string SortType
        {
            get { return sortType; }
            set { sortType = value; }
        } 
        #endregion
    }
}
