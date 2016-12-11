using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WatchDogLib;

namespace WatchDog
{
    public partial class WatchDogForm : Form
    {
        private ApplicationWatcher applicationWatcher;
        public WatchDogForm()
        {
            InitializeComponent();
            applicationWatcher = new ApplicationWatcher("MonitoredApplication", "WatchDog", 5000);
        }

        private void WatchDogForm_Resize(object sender, EventArgs e)
        {
            notifyIcon1.BalloonTipTitle = "Minimize to Tray App";
            notifyIcon1.BalloonTipText = "You have successfully minimized your form.";

            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(500);
                this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon1.Visible = false;
            }
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void WatchDogForm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            applicationWatcher.Stop();
        }

    }
}
