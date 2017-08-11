using Microsoft.AspNet.SignalR;

namespace IDPJobManager.Web
{
    public class JobHub : Hub
    {
        public void Reload()
        {
            Clients.All.Reload();
        }
    }
}
