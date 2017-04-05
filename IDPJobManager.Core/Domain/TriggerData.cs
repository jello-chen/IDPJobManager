namespace IDPJobManager.Core.Domain
{
    using System;
    using IDPJobManager.Core.Domain.TriggerTypes;

    public class TriggerData : Activity
    {
        public TriggerData(string name, ActivityStatus status) : base(name, status)
        {
        }

        public string Group { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? NextFireDate { get; set; }

        public DateTime? PreviousFireDate { get; set; }

        public TriggerType TriggerType { get; set; }

        public new bool CanPause
        {
            get { return Status == ActivityStatus.Active; }
        }

        public new bool CanStart
        {
            get { return Status == ActivityStatus.Paused; }
        }
    }
}