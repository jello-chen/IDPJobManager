namespace IDPJobManager.Core
{
    using IDPJobManager.Core.Domain;
    using Quartz;

    /// <summary>
    /// Translates Quartz.NET entyties to IDPJobManager objects graph.
    /// </summary>
    public interface ISchedulerDataProvider
    {
        SchedulerData Data { get; }

        JobDetailsData GetJobDetailsData(string name, string group);

        TriggerData GetTriggerData(TriggerKey key);
    }
}