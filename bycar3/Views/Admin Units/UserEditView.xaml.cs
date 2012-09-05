using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using bycar;

namespace bycar3.Views.Admin_Units
{
    /// <summary>
    /// Interaction logic for UserEditView.xaml
    /// </summary>
    public partial class UserEditView : Window
    {
        admin_unit user = null;
        public UserEditView()
        {
            InitializeComponent();
        }
        public UserEditView(int UserID)
        {
            InitializeComponent();
            LoadItem(UserID);
        }
        void LoadItem(int UserID)
        {
            DataAccess db = new DataAccess();
            user = db.AdminUnitGet(UserID);

            edtName.Text = user.name;
            edtPassword.Password = user.password;
            edtPasswordConfirm.Password = user.password;
            edtIsAdmin.IsChecked = user.is_admin > 0;
        }
        admin_unit SaveItem()
        {
            if (user == null)
                user = new admin_unit();
            user.name = edtName.Text;
            user.password = edtPassword.Password;
            user.is_admin = edtIsAdmin.IsChecked.HasValue?( edtIsAdmin.IsChecked.Value ? 1 : 0):0;
            return user;
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            // check pass confirm
            if (string.IsNullOrWhiteSpace(edtPassword.Password))
            {
                MessageBox.Show("Пароль не может быть пустым!");
                return;
            }
            if (edtPassword.Password != edtPasswordConfirm.Password)
            {
                MessageBox.Show("Пароли не совпадают!");
                return;
            }
            DataAccess db = new DataAccess();
            if (user == null)
            {
                // create
                // check username                
                if (db.UserNameExist(edtName.Text))
                {
                    MessageBox.Show("Пользователь с таким именем уже существует!");
                    return;
                }
                db.AdminUnitCreate(SaveItem());
            }
            else
            {
                // edit
                db.AdminUnitEdit(SaveItem());
            }
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            edtName.Focus();
        }
    }
}
