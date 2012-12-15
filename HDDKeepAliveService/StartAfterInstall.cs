using System.ServiceProcess;

namespace HDDKeepAliveService
{
    partial class ProjectInstaller
    {
        protected override void OnCommitted(System.Collections.IDictionary savedState)
        {
            new ServiceController(serviceInstaller1.ServiceName).Start();
        }
    }
}

