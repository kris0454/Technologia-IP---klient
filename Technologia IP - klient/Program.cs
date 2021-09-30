using System;
using System.Windows.Forms;

namespace Technologia_IP___klient
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form2());
            }
            catch (Exception e)
            { MessageBox.Show(e.ToString()); }
        }
    }
}