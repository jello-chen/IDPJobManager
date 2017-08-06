using System;
using System.Configuration;

namespace IDPJobManager.Core.Config
{
    public static class GlobalConfiguration
    {
        public static string Name { get; set; }
        public static string ConnectionString { get; set; }

        public static string ProviderName { get; set; }

        public static void ConfigureDBConnectionString()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["IDP-JobManager"];
            if (connectionString == null)
                throw new InvalidOperationException("Not configure `IDP-JobManager` connection string.");
            Name = "IDP-JobManager";
            ConnectionString = connectionString.ConnectionString;
            ProviderName = connectionString.ProviderName;
        }
    }
}
