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

namespace bycar3.Views
{
    /// <summary>
    /// Interaction logic for ClientsView.xaml
    /// </summary>
    public partial class ClientsView : Window
    {
        public ClientsView()
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
            List<client> clients = da.GetClients();
            foreach (client c in clients)
            {
                c.car_markReference.Load();
                c.car_mark.car_producerReference.Load();
            }
            this.DataContext = clients;                        
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
        void DeleteItem()
        {
            int id = 0;
            client i = null;
            if (dgList.SelectedItem != null)
            {
                object sel = dgList.SelectedItem;
                i = (client)sel;
                id = i.id;
            }
            if (id > 0)
            {
                MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выделенную запись?", "Удаление!", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    DataAccess da = new DataAccess();
                    da.ClientDelete(id);
                    ReloadList();
                }
            }
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteItem();   

        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                EditSelectedItem();
                ReloadList();
            }
            catch (Exception)
            {
                ReloadList();
            };
        }
        private void CreateItem()
        {
            ClientsEditView v = new ClientsEditView();
            v._id = -1;
            v.ShowDialog();
        }
        private void EditSelectedItem()
        {
            int id = 0;
            client b = null;
            if (dgList.SelectedItem != null)
            {
                object sel = dgList.SelectedItem;
                b = (client)sel;
                id = b.id;
            }
            if (id > 0)
            {
                ClientsEditView v = new ClientsEditView();
                v._id = b.id;

                v.cbCar.SelectedItem = b.car_mark.name;
                v.cbProds.SelectedItem = b.car_mark.car_producer.name;
                v.edtName.Text = b.name;
                v.edtDescr.Text = b.description;
                v.ShowDialog();
                ReloadList();
            }
        }
    }
}
