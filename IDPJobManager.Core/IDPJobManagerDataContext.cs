using EasyORM;
using IDPJobManager.Core.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDPJobManager.Core
{
    [Export]
    public class IDPJobManagerDataContext : DataContext, IDisposable
    {
        public IDPJobManagerDataContext() : base(GlobalConfig.ConnectionString, GlobalConfig.ProviderName)
        {

        }

        public void Dispose()
        {

        }
    }
}
