using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using bycar3.Core;
using splashDemo;

namespace bycar3
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnStartUp(Object sender, StartupEventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            try
            {
                SplashThreadStart();
                MainWindow mw = new MainWindow();
                mw.Show();
                mw.Activate();
                SplashThreadStop();
            }
            catch (Exception edf2)
            {
                SplashThreadStop();
                Marvin.Instance.Log(edf2.Message);
                MessageBox.Show("EXCEPTION: " + edf2.Message + "\nINNER: " + edf2.InnerException.Message);
            }                     
        }

        private Thread splash = null;

        private void SplashThreadStart()
        {
            splash = new Thread(SplashWindowShow);
            splash.IsBackground = true;
            splash.SetApartmentState(ApartmentState.STA);
            splash.Start();
        }

        private void SplashThreadStop()
        {
            if (splash != null)
            {
                splash.Abort();
                splash = null;
            }
        }

        private void SplashWindowShow()
        {
            new SplashWindow().ShowDialog();
        }
    }
}