using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using bycar;
using bycar3.Core;

namespace bycar3.Views.Common
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            LoadUsers();
        }

        private List<admin_unit> items;

        private void LoadUsers()
        {
            DataAccess db = new DataAccess();
            items = db.GetAdminUnits();
            edtUser.DataContext = items;
            edtUser.SelectedIndex = 0;
        }

        public bool Res = false;

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            TryLogin();
        }

        private void TryLogin()
        {
            int UserID = (int)edtUser.SelectedValue;
            admin_unit user = items.FirstOrDefault(x => x.id == UserID);
            if (edtPassword.Password == user.password)
            {
                Res = true;
                Marvin.Instance.CurrentUser = user;
                if (edtRememberUser.IsChecked == true)
                {
                    DataAccess db = new DataAccess();
                    settings_profile settings = db.getProfileCurrent();
                    settings.DefaultUserID = user.id;
                    db.ProfileEdit(settings);
                }
                this.Close();
            }
            else
            {
                MessageBox.Show("Неправильная комбинация логина и пароля!");
                edtPassword.Password = "";
                edtPassword.Focus();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            edtPassword.Focus();
        }

        private void edtPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !String.IsNullOrWhiteSpace(edtPassword.Password))
                TryLogin();
        }
    }
}