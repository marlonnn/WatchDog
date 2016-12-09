using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WatchDog
{
    #region ProcessStatusEventArgs: Fires when Queue Engine starts or stops
    public class ProcessStatusEventArgs : EventArgs
    {
        public Process QueueEngineProcess;
        public bool IsRunning;
        public ProcessStatusEventArgs(Process queueEngineProcess, bool isRunning)
        {
            QueueEngineProcess = queueEngineProcess;
            IsRunning = isRunning;
        }
    }
    #endregion

    public class AppWrapper : ITrainingDog
    {
        /// <summary>
        /// Is being use to destroy the hidden window in Spinning Engine
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        [DllImport("user32")]
        public static extern int SendMessage(int hwnd, int msg, int p1, int p2);

        protected int PID = -1; /// Process ID associated to this object

        protected Process theProcess = null;
        protected bool b_IsAlive = false;
        public bool IsAlive
        {
            get
            {
                return b_IsAlive;
            }
        }

        #region override FindProcess
        /// <summary>
        /// Overrided find the spinning engine process
        /// </summary>
        /// <returns>The first process found with SpinningEngine.exe name </returns>
        public Process FindProcess()
        {
            if (PID >= 0)
            {
                try
                {
                    Process TheProcess = GetProcessByID();
                    if (TheProcess == null)
                    {
                        OnProcessStatusChangedEvent(this, new ProcessStatusEventArgs(null, false));
                        return null;
                    }
                    else
                    {
                        OnProcessStatusChangedEvent(this, new ProcessStatusEventArgs(TheProcess, true));
                        return TheProcess;
                    }
                }
                catch (Exception ex)
                {
                    Console.Write("ISpin", "FindProcess: " + ex.Message,
                        System.Diagnostics.EventLogEntryType.Error, 0);
                    return null;
                }
            }
            else
            {
                OnProcessStatusChangedEvent(this, new ProcessStatusEventArgs(null, false));
                return null;
            }
        }
        #endregion

        #region override StartUp
        public Process StartUp(string theApp2Watch)
        {
            /// Send the process a unique name
            Process p = StartUpProcess(theApp2Watch, "");

            // April 12, 05
            OnProcessStatusChangedEvent(this, new ProcessStatusEventArgs(theProcess, true));

            return p;
        }
        #endregion

        #region override ShutDown
        public void ShutDown()
        {
            b_IsAlive = false;
            if (theProcess != null)
            {
                if (GetProcessByID() != null)
                    theProcess.Kill();
                OnProcessStatusChangedEvent(this, new ProcessStatusEventArgs(null, false));
            }
            else
                OnProcessStatusChangedEvent(this, new ProcessStatusEventArgs(theProcess, false));
        }
        #endregion

        #region GetProcessByID
        /// <summary>
        /// Return the Process this object is manageing
        /// </summary>
        /// <returns>The process of this object PID</returns>
        protected Process GetProcessByID()
        {
            try
            {
                if (PID < 0)
                    return null;
                else
                    return Process.GetProcessById(PID);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Set the PID for an object, It has been used in CCMUtil
        /// </summary>
        /// <param name="thPID"></param>
        public int ProcessID
        {
            get
            {
                return PID;
            }
            set
            {
                if (value < 0)
                    throw new Exception("Process ID cannot be negative.");
                PID = value;
            }
        }
        #endregion

        public object SuspectedSmell()
        {
            try
            {
                Process TheProcess = GetProcessByID();
                if (TheProcess == null)
                    return (bool)false;
                else
                    return null;
            }
            catch
            {
                return (bool)true;
            }
        }

        //Process Status Event definitions
        public delegate void ProcessStatusEventHandler(object source, ProcessStatusEventArgs rea);
        public event ProcessStatusEventHandler ProcessStatusChangedEvent;
        protected void OnProcessStatusChangedEvent(object source, ProcessStatusEventArgs rea)
        {
            if (!rea.IsRunning)
            {
                b_IsAlive = false;
                if (theProcess != null)
                {
                    try
                    {
                        try
                        {
                            theProcess.Kill();
                        }
                        catch
                        {
                        }
                    }
                    finally
                    {
                        theProcess = null;
                    }
                }
            }
            if (ProcessStatusChangedEvent != null)
            {
                this.ProcessStatusChangedEvent(source, rea);
            }
        }

        protected Process StartUpProcess(string exe, string cmd)
        {
            try
            {
                if (!b_IsAlive)
                {
                    theProcess = Process.Start(exe, cmd);
                    PID = theProcess.Id;
                    b_IsAlive = true;
                }
                OnProcessStatusChangedEvent(this, new ProcessStatusEventArgs(theProcess, true));
                return theProcess;
            }
            catch (Exception ex)
            {
                OnProcessStatusChangedEvent(this, new ProcessStatusEventArgs(theProcess, false));
                Console.Write("CallbackMNethod.InitRemoting", "Cannot start process: " + exe + "\r\n\r\n" + ex.Message,
                    System.Diagnostics.EventLogEntryType.Error, 0);
                //MessageBox.Show ( "Cannot start process: "+exe+"\r\n\r\n"+ex.Message);
                return null;
            }
        }

        public void Initialize(string theApp2Watch)
        {
            theProcess = FindProcess();
            if (theProcess == null)
            {
                theProcess = StartUp(theApp2Watch);
                if (theProcess == null)
                    throw new Exception();
            }
        }

        public AppWrapper()
        {
            try
            {
                string AppPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                //SAMShareIniFile = new Utils.IniFile(AppPath+@"\SAMShare.ini");
            }
            catch (Exception ex)
            {
                Console.Write("IPCParent", "Constructoe: " + ex.Message,
                    System.Diagnostics.EventLogEntryType.Error, 0);
            }
        }

        public void Dispose()
        {
            if (theProcess != null)
            {
                ShutDown();
            }
        }
    }
}
