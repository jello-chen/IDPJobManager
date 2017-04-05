namespace IDPJobManager.Core.Domain
{
    using System.Collections.Generic;
    using System.Linq;

    public class JobGroupData : ActivityNode<JobData>
    {
        public JobGroupData(string name, IList<JobData> jobs)
            : base(name)
        {
            Jobs = jobs;
        }

        public IList<JobData> Jobs { get; private set; }

        public bool HasTriggers
        {
            get { return Jobs.Any(j => j.HasTriggers); }
        }

        public new bool CanPause
        {
            get { return Jobs.Any(t => t.CanPause); }
        }

        public new bool CanStart
        {
            get { return Jobs.Any(t => t.CanStart); }
        }

        protected override IList<JobData> ChildrenActivities
        {
            get { return Jobs; }
        }
    }
}