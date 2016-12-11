using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WatchDogLib
{
    public class ApplicationWatcher
    {
        private const string HEARTBEAT_FILE_NAME = "heartbeat";
        private string monitoredAppName = "";
        private string watchdogAppName = "WatchDog";
        private int monitorInterval = 3000;

        private Thread watchDogThread;

        public ApplicationWatcher(string monitoredApplicationName, string watchdogAppliationName, int monitorInterval)
        {
            this.monitoredAppName = monitoredApplicationName;
            this.watchdogAppName = watchdogAppliationName;
            this.monitorInterval = monitorInterval;

            try
            {
                Process[] process = Process.GetProcessesByName(watchdogAppName);
                if (process.Length > 1)
                {
                    Process.GetCurrentProcess().Kill();
                }

                watchDogThread = new Thread(new ThreadStart(StartAppMonitoring));
                watchDogThread.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ApplicationWatcher Exception1: " + ex.StackTrace);
            }
        }

        private void StartAppMonitoring()
        {
            while (true)
            {
                string monitoredAppExePath = monitoredAppName + ".exe";
                if (!File.Exists("watchdoglock"))
                {
                    if (!MonitoredAppExists())
                    {
                        if (File.Exists(monitoredAppExePath))
                        {
                            Process.Start(monitoredAppExePath);
                        }
                        else
                        {
                            Debug.WriteLine("ApplicationWatcher StartAppMonitoring Monitored Application exe not found at: " + monitoredAppExePath);
                        }
                    }
                    else
                    {
                        if (File.Exists(HEARTBEAT_FILE_NAME))
                        {
                            try
                            {
                                File.Delete(HEARTBEAT_FILE_NAME);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("ApplicationWatcher StartAppMonitoring Exception1: " + ex.StackTrace);
                            }
                        }
                        else
                        {
                            /// If the heartbeat file is not created, this could mean that the Monitored Application could be frozen
                            while (MonitoredAppExists())
                            {
                                try
                                {
                                    Process[] pname = Process.GetProcessesByName(monitoredAppName);
                                    foreach (Process process in pname)
                                    {
                                        process.Kill();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine("ApplicationWatcher StartAppMonitoring Exception2: " + ex.StackTrace);
                                }
                            }
                            Process.Start(monitoredAppExePath);
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("ApplicationWatcher StartAppMonitoring watchdoglock found.");
                }
                Thread.Sleep(monitorInterval);
            }
        }

        private bool MonitoredAppExists()
        {
            try
            {
                Process[] ProcessList = Process.GetProcessesByName(monitoredAppName);
                if (ProcessList.Length == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ApplicationWatcher MonitoredAppExists Exception: " + ex.StackTrace);
                return true;
            }
        }

        public void Stop()
        {
            if (watchDogThread != null && watchDogThread.IsAlive)
            {
                watchDogThread.Abort();
                watchDogThread = null;
            }
        }
    }
}
