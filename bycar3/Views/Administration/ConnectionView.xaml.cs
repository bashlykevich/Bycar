using System.Windows;

namespace bycar3.Views.Administration
{
    /// <summary>
    /// Interaction logic for ConnectionView.xaml
    /// </summary>
    public partial class ConnectionView : Window
    {
        public bool connect = false;

        public ConnectionView()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            connect = true;
            Close();
        }
    }
}