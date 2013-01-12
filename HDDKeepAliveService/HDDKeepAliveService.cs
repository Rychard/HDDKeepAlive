using System;
using System.ServiceProcess;
using System.Text;
using System.Threading;


namespace HDDKeepAliveService
{
    partial class HDDKeepAliveService : ServiceBase
    {
        private Thread t;
        private readonly AutoResetEvent stopEvent = new AutoResetEvent(false);

        public HDDKeepAliveService()
        {
            InitializeComponent();
            if (!System.Diagnostics.EventLog.SourceExists("HDDKeepAlive"))
            {
                System.Diagnostics.EventLog.CreateEventSource("HDDKeepAlive", "HDDKeepAliveLog");
            }
            EventLogWriter.Source = "HDDKeepAlive";
            EventLogWriter.Log = "HDDKeepAliveLog";
        }

        protected override void OnStart(string[] args)
        {
            t = new Thread(KeepAlive);
            t.Start();
            EventLogWriter.WriteEntry("Service Started.");
        }

        protected override void OnStop()
        {
            stopEvent.Set();
            t.Join();
            EventLogWriter.WriteEntry("Service Stopped.");
        }

        private void KeepAlive()
        {
            ConfigHelper.ValidateConfiguration();
            TimeSpan WaitInterval = GetInterval();

            while (true)
            {
                // Primitive Timer
                // This code puts the thread to sleep, 
                if (stopEvent.WaitOne(WaitInterval)) { return; }
                DateTime Start = DateTime.Now;
                KeepAliveDrive[] Drives = KeepAliveDrive.GetDrives();

                var DrivesAvailable = new StringBuilder();
                var DrivesEnabled = new StringBuilder();
                int numDrivesEnabled = 0;
                foreach (KeepAliveDrive Drive in Drives)
                {
                    String DisplayName = "[" + Drive.DrivePath + "] ";
                    DrivesAvailable.Append(DisplayName);
                    if (Drive.IsKeepAliveEnabled)
                    {
                        numDrivesEnabled++;
                        DrivesEnabled.Append(DisplayName);
                    }
                }
                DrivesAvailable.AppendLine("");
                DrivesEnabled.AppendLine("");

                var PingLog = new StringBuilder("");
                foreach (var Drive in Drives)
                {
                    Boolean isEnabled = ConfigHelper.IsKeepaliveEnabled(Drive.DriveType);
                    if (isEnabled)
                    {
                        PingLog.Append("Pinging Drive " + Drive.DriveLetter.ToString() + "... ");
                        try
                        {
                            Drive.PingDrive();
                            PingLog.AppendLine("Done.");
                        }
                        catch (UnauthorizedAccessException)
                        {
                            PingLog.AppendLine("Failed.");
                        }
                    }
                }

                var LogEntry = new StringBuilder("");
                LogEntry.AppendLine("Available Drives: " + DrivesAvailable);
                LogEntry.AppendLine("Drives To Ping: " + DrivesEnabled);
                LogEntry.AppendLine(PingLog.ToString());

                DateTime End = DateTime.Now;
                TimeSpan Length = End - Start;
                LogEntry.AppendLine("Drives Pinged: " + numDrivesEnabled.ToString());
                LogEntry.AppendLine("Time Taken: " + Length.TotalSeconds.ToString() + " seconds");
                EventLogWriter.WriteEntry(LogEntry.ToString());
            }
        }

        /// <summary>
        /// Gets the configured interval between KeepAlive events from the configuration file.
        /// </summary>
        private static TimeSpan GetInterval()
        {
            int? Interval = ConfigHelper.GetConfigurationValueInteger("KeepAliveInterval");
            TimeSpan WaitInterval = Interval.HasValue ? new TimeSpan(0, 0, Interval.Value) : new TimeSpan(0, 0, 1);
            return WaitInterval;
        }
    }
}
