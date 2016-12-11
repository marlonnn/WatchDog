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
    public class WatchdogWatcher
    {
        private const string HEARTBEAT_FILE_NAME = "heartbeat";
        private string watchdogAppName = "WatchDog";
        private string watchdogExePath = "WatchDog.exe";
        private int watchDogMonitorInterval = 3000;
        //private Thread watchDogManagerThread;
        private Thread heatbeatThread;

        public WatchdogWatcher(string watchdogApplicationName, string watchdogExecutablePath, int watchdogMonitoringInterval)
        {
            this.watchdogAppName = watchdogApplicationName;
            this.watchdogExePath = watchdogExecutablePath;
            this.watchDogMonitorInterval = watchdogMonitoringInterval;

            try
            {
                //watchDogManagerThread = new Thread(new ThreadStart(StartWatchDogMonitoring));
                //watchDogManagerThread.Start();

                heatbeatThread = new Thread(new ThreadStart(StartHeartbeatThread));
                heatbeatThread.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception WatchdogMonitor2: " + ex.StackTrace);
            }
        }

        private void StartWatchDogMonitoring()
        {
            while (true)
            {
                try
                {
                    if (!File.Exists("watchdoglock"))
                    {
                        Process[] processList = Process.GetProcessesByName(watchdogAppName);
                        if (processList.Length == 0)
                        {
                            Process.Start(watchdogExePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception WatchdogMonitor3: " + ex.StackTrace);
                }
                Thread.Sleep(watchDogMonitorInterval);
            }
        }

        private void StartHeartbeatThread()
        {
            while (true)
            {
                try
                {
                    File.Create(HEARTBEAT_FILE_NAME).Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception WatchdogMonitor4: " + ex.StackTrace);
                }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Utility method to terminate the watchdog process
        /// </summary>
        public void KillWatchDog()
        {
            try
            {
                Stop();
                Process[] processList = Process.GetProcessesByName(watchdogAppName);
                if (processList.Length > 0)
                {
                    foreach (Process process in processList)
                    {
                        process.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception WatchdogMonitor5: " + ex.StackTrace);
            }
        }

        public void Stop()
        {
            //if (watchDogManagerThread != null && watchDogManagerThread.IsAlive)
            //{
            //    watchDogManagerThread.Abort();
            //    watchDogManagerThread = null;
            //}
            if (heatbeatThread != null && heatbeatThread.IsAlive)
            {
                heatbeatThread.Abort();
                heatbeatThread = null;
            }
            if (File.Exists("watchdoglock"))
            {
                File.Delete("watchdoglock");
            }
            if (File.Exists("heartbeat"))
            {
                File.Delete("heartbeat");
            }
        }
    }
}
