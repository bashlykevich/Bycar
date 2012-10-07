using System;
using System.Windows;
using System.Windows.Input;
using bycar;

namespace bycar3.Views
{
    /// <summary>
    /// Interaction logic for AdminUnitsView.xaml
    /// </summary>
    public partial class AdminUnitsView : Window
    {
        public AdminUnitsView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadList();
        }

        private void ReloadList()
        {
            DataAccess da = new DataAccess();
            this.DataContext = da.GetAdminUnits();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            EditSelectedItem();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            CreateItem();
            ReloadList();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                EditSelectedItem();
            }
            catch (Exception)
            {
                ReloadList();
            };
        }

        private void CreateItem()
        {
            AdminUnitsEditView v = new AdminUnitsEditView();
            v._id = -1;
            v.ShowDialog();
        }

        private void EditSelectedItem()
        {
            int id = 0;
            admin_unit b = null;
            if (dgList.SelectedItem != null)
            {
                object sel = dgList.SelectedItem;
                b = (admin_unit)sel;
                id = b.id;
            }
            if (id > 0)
            {
                AdminUnitsEditView v = new AdminUnitsEditView();
                v._id = b.id;
                v.edtName.Text = b.name;

                //v.edtDescr.Text = b.description;
                v.ShowDialog();
                ReloadList();
            }
        }
    }
}