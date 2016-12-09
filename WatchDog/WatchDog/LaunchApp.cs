using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchDog
{
    public class LaunchApp : IDisposable
    {
        private string theApp2RunPath = "";
        private int iCheckPeriod = 1000; // Default 1 second

        private AppWrapper appStarterWrapper;
        private WatchDog pitBull = null;

        private LaunchApp() { }
        public LaunchApp(int CheckPeriod, string the2Run)
        {
            iCheckPeriod = CheckPeriod;
            theApp2RunPath = the2Run;
            this.Create();
            appStarterWrapper.StartUp(theApp2RunPath);
        }

        private void Create()
        {
            appStarterWrapper = new AppWrapper();
            pitBull = new WatchDog(appStarterWrapper);
            pitBull.BarkEvent += new WatchDog.BarkEventHandler(Dog_BarkEvent);
            appStarterWrapper.ProcessStatusChangedEvent += new AppWrapper.ProcessStatusEventHandler(appStarterWrapper_ProcessStatusChangedEvent);
        }

        private void Dog_BarkEvent(object source, WatchDogEventArgs rea)
        {
            // start the the Engine again
            AppWrapper beWatched = (AppWrapper)((WatchDog)source).ObjectToBeWatched;
            beWatched.ShutDown();
            beWatched.StartUp(theApp2RunPath);
            //CallbackMethods_StatusChangedEvent(Utils.MailSlot.TWebgateStatus.wgWebgateNotRunning.ToString());
        }

        public void appStarterWrapper_ProcessStatusChangedEvent(object source, ProcessStatusEventArgs rea)
        {
            if ((pitBull != null))
            {
                if (rea.IsRunning)
                {
                    /// Pitbull is a little stupid! so give him 4 seconds to learn the suspected smell!
                    pitBull.TrainTheSmell(100);
                    pitBull.Wakeup(iCheckPeriod);

                }
                else
                {
                    pitBull.Sleep();
                }
            }
        }

        private void Destroy()
        {
            try
            {
                if (appStarterWrapper != null)
                    appStarterWrapper.Dispose();
                if (pitBull != null)
                    pitBull.Dispose();
            }
            catch
            {
                // Ignore any failiur
            }
        }

        public void Dispose()
        {
            this.Destroy();
        }
    }
}
