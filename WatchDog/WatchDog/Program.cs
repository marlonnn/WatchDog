using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WatchDog
{
    /// <summary>
    /// The Watchdog Application
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new WatchDogForm());
            }
            catch (Exception ex)
            {
            }
        }
    }
}
