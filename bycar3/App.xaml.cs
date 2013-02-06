using System;
using System.Globalization;
using System.Threading;
using System.Windows;
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
            SplashThreadStart();
            MainWindow mw = new MainWindow();
            mw.Show();
            mw.Activate();
            SplashThreadStop();
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