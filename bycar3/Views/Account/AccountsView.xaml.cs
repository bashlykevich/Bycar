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
    /// Interaction logic for AccountsView.xaml
    /// </summary>
    public partial class AccountsView : Window
    {
        public AccountsView()
        {
            InitializeComponent();
            ReloadList();
        }
        private void ReloadList()
        {
            DataAccess da = new DataAccess();
            List<AccountView> items = da.GetAllAccountViews();            
            this.DataContext = items;
        }
        private void CreateItem()
        {
            AccountsEditView v = new AccountsEditView();
            v._id = -1;
            v.ShowDialog();
        }
        private void EditSelectedItem()
        {
            int id = 0;
            AccountView b = null;
            if (dgList.SelectedItem != null)
            {
                object sel = dgList.SelectedItem;
                b = (AccountView)sel;
                id = b.id;
            }
            if (id > 0)
            {
                AccountsEditView v = new AccountsEditView();
                v._id = b.id;
                v.edtName.Text = b.name;
                v.edtDescr.Text = b.description;
                v.edtAddress.Text = b.address;                
                v.edtDiscount.Text = b.discount.ToString();
                v.edtOKPO.Text = b.okpo.ToString();
                //v.edtScore.Text = b.bank_score;
                v.edtShippingBase.Text = b.shipping_base;
                v.edtShippingDest.Text = b.shipping_point;
                //v.edtType.SelectedItem = b.account_type.name;
                //v.edtBank.SelectedItem = b.bank.name;
                v.edtUNN.Text = b.unn;                
                v.ShowDialog();
                ReloadList();
            }
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
        void DeleteItem()
        {            
            if (dgList.SelectedItems.Count > 0)
            {
                MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выделенные записи?", "Удаление", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    DataAccess da = new DataAccess();
                    foreach (AccountView item in dgList.SelectedItems)
                    {
                        da.AccountDelete(item.id);
                    }
                    ReloadList();
                }
            }                      
        }
    }
}
