using IDPJobManager.Core.Domain;
using IDPJobManager.Core.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using IDPJobManager.Core.Extensions;

namespace IDPJobManager.Core.ViewProjections.Performance
{
    public class AllPerformancesViewProjection : IViewProjection<AllPerformancesBindingModel, AllPerformancesViewModel>
    {
        public AllPerformancesViewModel Project(AllPerformancesBindingModel input)
        {
            using (var dataContext = new IDPJobManagerDataContext())
            {
                var query = from j in dataContext.Set<JobPerformance>()
                            select j;

                if (!string.IsNullOrEmpty(input.JobName))
                    query = query.Where(j => j.JobName.Contains(input.JobName));

                if (!string.IsNullOrWhiteSpace(input.JobGroup))
                    query = query.Where(j => j.JobGroup.Contains(input.JobGroup));

                if (input.StartDate != DateTime.MinValue)
                    query = query.Where(j => j.CreateTime >= input.StartDate);

                if (input.EndDate != DateTime.MinValue)
                {
                    var endDate = input.EndDate.GetEndDateTimeOfDay();
                    query = query.Where(j => j.CreateTime <= endDate);
                }

                var totalCount = query.Count();

                if (!string.IsNullOrEmpty(input.SortKey) && !string.IsNullOrEmpty(input.SortType))
                {
                    query = query.OrderBy(input.SortKey, input.SortType);
                }
                else
                {
                    query = query.OrderByDescending(j => j.StartTime).ThenByDescending(j => j.EndTime);
                }

                var jobPerformances = query
                            .Skip((input.PageCurrent - 1) * input.PageSize)
                            .Take(input.PageSize)
                            .ToList();

                return new AllPerformancesViewModel
                {
                    Items = jobPerformances,
                    TotalCount = totalCount
                };
            }
        }
    }

    public class AllPerformancesBindingModel : IPager, ISortor
    {
        public string JobName { get; set; }
        public string JobGroup { get; set; }
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

    public class AllPerformancesViewModel
    {
        public IEnumerable<JobPerformance> Items { get; set; }
        public int TotalCount { get; set; }
    }
}
