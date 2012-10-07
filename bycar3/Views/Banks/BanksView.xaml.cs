using System;
using System.Windows;
using System.Windows.Input;
using bycar;

namespace bycar3.Views
{
    /// <summary>
    /// Interaction logic for BanksView.xaml
    /// </summary>
    public partial class BanksView : Window
    {
        public BanksView()
        {
            InitializeComponent();
            ReloadList();
        }

        private void ReloadList()
        {
            DataAccess da = new DataAccess();
            this.DataContext = da.GetAllBanks();
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                EditSelectedBank();
                ReloadList();
            }
            catch (Exception)
            {
                ReloadList();
            };
        }

        private void CreateBank()
        {
            BanksEditView v = new BanksEditView();
            v._ID = -1;
            v.ShowDialog();
        }

        private void EditSelectedBank()
        {
            int id = 0;
            bank b = null;
            if (dgList.SelectedItem != null)
            {
                object sel = dgList.SelectedItem;
                b = (bank)sel;
                id = b.id;
            }
            if (id > 0)
            {
                BanksEditView v = new BanksEditView();
                v._ID = b.id;
                v.edtName.Text = b.name;
                v.edtPhone.Text = b.phone;
                v.edtMFO.Text = b.mfo;
                v.edtFax.Text = b.fax;
                v.edtAddress.Text = b.address;
                v.ShowDialog();
                ReloadList();
            }
        }

        private void DeleteItem()
        {
            int id = 0;
            bank b = null;
            if (dgList.SelectedItem != null)
            {
                object sel = dgList.SelectedItem;
                b = (bank)sel;
                id = b.id;
            }
            if (id > 0)
            {
                MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выделенную запись?", "Удаление", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    DataAccess da = new DataAccess();
                    da.BankDelete(id);
                    ReloadList();
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteItem();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            EditSelectedBank();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            CreateBank();
            ReloadList();
        }
    }
}