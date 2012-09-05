using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Threading;
using splashDemo;
using bycar3.Core;
using System.Globalization;
using bycar3.Views.Administration;
using bycar3.Views.Common;

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
        Thread splash = null;
        void SplashThreadStart()
        {
            splash = new Thread(SplashWindowShow);
            splash.IsBackground = true;
            splash.SetApartmentState(ApartmentState.STA);
            splash.Start();
        }
        void SplashThreadStop()
        {
            if (splash != null)
            {
                splash.Abort();
                splash = null;
            }
        }
        void SplashWindowShow()
        {
            new SplashWindow().ShowDialog();
        }
    }

}
