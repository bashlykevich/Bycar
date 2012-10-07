using System;
using System.Windows;
using System.Windows.Input;
using bycar;

namespace bycar3.Views
{
    /// <summary>
    /// Interaction logic for ContactsView.xaml
    /// </summary>
    public partial class ContactsView : Window
    {
        public ContactsView()
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

            //this.DataContext = da.GetContacts();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            CreateItem();
            ReloadList();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            EditSelectedItem();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteItem();
        }

        private void DeleteItem()
        {
            /*
            int id = 0;
            contact b = null;
            if (dgList.SelectedItem != null)
            {
                object sel = dgList.SelectedItem;
                b = (contact)sel;
                id = b.id;
            }
            if (id > 0)
            {
                MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выделенную запись?", "Удаление!", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    DataAccess da = new DataAccess();
                    da.ContactDelete(id);
                    ReloadList();
                }
            }*/
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
            ContactsEditView v = new ContactsEditView();
            v._id = -1;
            v.ShowDialog();
        }

        private void EditSelectedItem()
        {
            /*
            int id = 0;
            contact b = null;
            if (dgList.SelectedItem != null)
            {
                object sel = dgList.SelectedItem;
                b = (contact)sel;
                id = b.id;
            }
            if (id > 0)
            {
                ContactsEditView v = new ContactsEditView();
                v._id = b.id;
                v.edtName.Text = b.name;
                v.edtNumber.Text = b.passport_number;
                v.edtSeries.Text = b.passport_series;
                v.edtPassportBy.Text = b.passport_by;
                v.edtDate.SelectedDate = b.passort_date.GetValueOrDefault(DateTime.Now);
                v.ShowDialog();
                ReloadList();
            }*/
        }
    }
}