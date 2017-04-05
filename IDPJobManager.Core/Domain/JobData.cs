namespace IDPJobManager.Core.Domain
{
    using System.Collections.Generic;
    using System.Linq;

    public class JobData : ActivityNode<TriggerData>
    {
        public JobData(string name, string group, IList<TriggerData> triggers): base(name)
        {
            Triggers = triggers;
            Group = group;
        }

        public IList<TriggerData> Triggers { get; private set; }

        public string Group { get; private set; }

        public string UniqueName
        {
            get
            {
                return string.Format("{0}_{1}", Group, Name);
            }
        }

        public bool HasTriggers
        {
            get
            {
                return Triggers != null && Triggers.Count > 0;
            }
        }

        public new bool CanPause
        {
            get { return Triggers.Any(t => t.CanPause); }
        }

        public new bool CanStart
        {
            get { return Triggers.Any(t => t.CanStart); }
        }

        protected override IList<TriggerData> ChildrenActivities
        {
            get { return Triggers; }
        }
    }
}