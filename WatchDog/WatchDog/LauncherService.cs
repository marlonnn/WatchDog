using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchDog
{
    public class LauncherService : System.ServiceProcess.ServiceBase
    {
        /// <summary>
        /// Collection of Auto application launcher objects
        /// </summary>
        private Hashtable AppX;

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public LauncherService()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // 
            // LauncherService
            // 
            this.ServiceName = "AppLauncherService";

        }

        static void Main()
        {
            LauncherService service = new LauncherService();
            service.MainOnStart();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
        }

        private void MainOnStart()
        {
            try
            {
                /// Start up Applications defined in WatchDog.ini file
                string AppPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                IniFile WatchDogIni = new IniFile(AppPath + @"\WatchDog.ini");

                int i = int.Parse(WatchDogIni.IniReadValue("Settings", "Period"));

                AppX = new Hashtable();

                string theAppPath = "";
                int iApp = 1;// Start from app number 1
                do
                {
                    theAppPath = WatchDogIni.IniReadValue("Apps", "App" + iApp.ToString());
                    if ((theAppPath != "") && (theAppPath != null))
                        AppX.Add(iApp, new LaunchApp(i, theAppPath));
                    else
                        break;
                    iApp++;
                } while (true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                MainOnStart();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected override void OnStop()
        {
            try
            {
                for (int i = 1; i <= AppX.Count; i++)
                {
                    ((LaunchApp)AppX[i]).Dispose();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
