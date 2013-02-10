using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;
using System.Configuration;

namespace splashDemo
{
    /// <summary>
    /// Interaction logic for splash.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        private Thread loadingThread;
        private Storyboard Showboard;
        private Storyboard Hideboard;

        private delegate void ShowDelegate(string txt);

        private delegate void HideDelegate();

        private ShowDelegate showDelegate;
        private HideDelegate hideDelegate;

        public SplashWindow()
        {
            InitializeComponent();
            
            
            string DriveApplicationVersion = bycar3.Properties.Settings.Default.DriveApplicationVersion;
            string DriveDatabaseVersion = bycar3.Properties.Settings.Default.DriveDatabaseVersion;
            edtVersion.Text = "версия " + DriveApplicationVersion + "-" + DriveDatabaseVersion;
            showDelegate = new ShowDelegate(this.showText);
            hideDelegate = new HideDelegate(this.hideText);
            Showboard = this.Resources["showStoryBoard"] as Storyboard;
            Hideboard = this.Resources["HideStoryBoard"] as Storyboard;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadingThread = new Thread(load);
            loadingThread.IsBackground = true;
            loadingThread.Start();
        }

        private void load()
        {
            while (true)
            {
                Thread.Sleep(2000);
                this.Dispatcher.Invoke(showDelegate, "Идёт загрузка данных...");
                Thread.Sleep(2000);
                this.Dispatcher.Invoke(hideDelegate);

                Thread.Sleep(2000);
                this.Dispatcher.Invoke(showDelegate, "Пожалуйста, подождите...");
                Thread.Sleep(2000);

                //load data
                this.Dispatcher.Invoke(hideDelegate);
            }
        }

        private void showText(string txt)
        {
            txtLoading.Text = txt;
            BeginStoryboard(Showboard);
        }

        private void hideText()
        {
            BeginStoryboard(Hideboard);
        }
    }
}