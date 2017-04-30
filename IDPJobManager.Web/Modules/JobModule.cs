namespace IDPJobManager.Web.Modules
{
    using Core.Config;
    using Core.Utils;
    using Models;
    using Nancy;
    using System.Threading.Tasks;

    public class JobModule : NancyModule
    {
        //private readonly DbHelper dbHelper = new DbHelper(GlobalConfig.ConnectionString, GlobalConfig.ProviderName);

        public JobModule() : base("/Job")
        {
            Get["/", runAsync: true] = async (p, ct) =>
              {
                  //var list = await dbHelper.ExecuteListAsync<JobModel>("SELECT * FROM T_Job");
                  //return View["Job/Index", list];
                  await Task.Delay(1);
                  return "Job";
              };
        }
    }
}
