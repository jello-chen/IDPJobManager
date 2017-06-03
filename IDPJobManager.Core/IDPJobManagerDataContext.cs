using EasyORM;
using IDPJobManager.Core.Config;
using System;
using System.ComponentModel.Composition;

namespace IDPJobManager.Core
{
    [Export]
    public class IDPJobManagerDataContext : DataContext, IDisposable
    {
        public IDPJobManagerDataContext() 
            : base(GlobalConfiguration.ConnectionString, GlobalConfiguration.ProviderName)
        {

        }

        public void Dispose()
        {

        }
    }
}
