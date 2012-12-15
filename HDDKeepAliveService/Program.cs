using System.ServiceProcess;

namespace HDDKeepAliveService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun = new ServiceBase[] 
                { 
                    new HDDKeepAliveService() 
                };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
