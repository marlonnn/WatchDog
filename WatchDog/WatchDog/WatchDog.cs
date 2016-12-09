using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WatchDog
{
    /// <summary>
    /// training dog interface
    /// </summary>
    public interface ITrainingDog
    {
        object SuspectedSmell();
    }

    #region WatchDogEventArgs: Event will fire in intervals
    public class WatchDogEventArgs : EventArgs
    {
        public object msg;
        public WatchDogEventArgs(object o_msg)
        {
            msg = o_msg;
        }
    }
    #endregion

    public class WatchDog : IDisposable
    {
        /// <summary>
        /// Chase is happening when the WatchDog object start checking the process status
        /// </summary>
        private System.Threading.Timer Chase;

        private bool b_IsAwake;
        public bool IsAwake
        {
            get
            {
                return b_IsAwake;
            }
        }
        public ITrainingDog ObjectToBeWatched = null;

        private WatchDog() { }

        /// <summary>
        ///  event will be fired when SuspectedSmell() of ITrainingDog returns true
        /// </summary>
        #region Watch dog barks
        public delegate void BarkEventHandler(object source, WatchDogEventArgs rea);
        public event BarkEventHandler BarkEvent;
        private void OnBarkEvent(object source, WatchDogEventArgs rea)
        {
            if (BarkEvent != null)
            {
                this.BarkEvent(source, rea);
            }
        }
        #endregion

        /// <summary>
        ///  event will be fired each time the watch dog object 
        ///  is going to check the subject object and it is okay
        /// </summary>
        #region Watch dog Idle event
        public event BarkEventHandler IdleEvent;
        private void OnIdleEvent(object source, WatchDogEventArgs rea)
        {
            if (IdleEvent != null)
            {
                this.IdleEvent(source, rea);
            }
        }
        #endregion

        /// <summary>
        /// Train the dog to smell the suspected smell until she could differentiate it by barking!
        /// TrainingPeriod is 1/10 of a second, For stupid dogs (slow CPUs!) use larger amount
        /// </summary>
        /// <param name="TrainingPeriod">
        ///		The amount of time the dog needs to be trained.
        ///		10 means one second and 20 means two seconds and so on
        /// </param>
        public void TrainTheSmell(int TrainingPeriod)
        {
            while ((ObjectToBeWatched.SuspectedSmell() != null) && (TrainingPeriod > 0))
            {
                TrainingPeriod--;
                Thread.Sleep(100);
            }
        }

        public WatchDog(ITrainingDog objectToBeWatched)
        {
            ObjectToBeWatched = objectToBeWatched;
            /// Create the delegate that invokes methods for the timer.
            TimerCallback StartChase = new TimerCallback(WakeUpAndChase);
            /// Create a timer that waits one second, then invokes every xxx seconds.
            Chase = new System.Threading.Timer(StartChase, this, Timeout.Infinite, 0);

        }

        public void Sleep()
        {
            Chase.Change(Timeout.Infinite, 0);
            b_IsAwake = false;
        }

        public void Wakeup(int Period)
        {
            Chase.Change(1, Period);
            b_IsAwake = true;
        }

        public void Wakeup(int WakeupTime, int Period)
        {
            Chase.Change(WakeupTime, Period);
            b_IsAwake = true;
        }

        public void Dispose()
        {
            Chase.Dispose();
            Chase = null;
        }

        private void WakeUpAndChase(Object state)
        {
            WatchDog theDog = (WatchDog)state;
            lock (theDog)
            {
                /// If the smell is suspected!!!
                if (ObjectToBeWatched != null)
                {
                    object o_msg = ObjectToBeWatched.SuspectedSmell();
                    if (o_msg != null)
                        OnBarkEvent(this, new WatchDogEventArgs(o_msg));
                    else
                        OnIdleEvent(this, null);
                }
            }
        }

    }
}
