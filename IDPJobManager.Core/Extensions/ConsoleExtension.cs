using IDPJobManager.Core.Utils;
using System;
using Topshelf.HostConfigurators;

namespace IDPJobManager.Core.Extensions
{
    public static class ConsoleExtension
    {
        public static void OnCloseConsole(this HostConfigurator configurator, Func<int, bool> closeHandler)
        {
            Ensure.Requires<ArgumentNullException>(configurator != null);
            ConsoleUtil.OnClose += new ConsoleCtrlDelegate(closeHandler);      
            ConsoleUtil.RegisterCloseConsoleHandle();           
        }
    }
}
