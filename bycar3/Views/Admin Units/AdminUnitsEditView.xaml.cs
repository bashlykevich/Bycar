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
    /// Interaction logic for AdminUnitsEditView.xaml
    /// </summary>
    public partial class AdminUnitsEditView : Window
    {
        public int _id = 0;
        public AdminUnitsEditView()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (this._id > 0)
                EditItem();
            else
                CreateItem();

            this.Close();
        }

        private void EditItem()
        {
            DataAccess da = new DataAccess();
            account_type item = new account_type();
            item.id = _id;
            item.name = edtName.Text;
            item.description = edtDescr.Text;
            da.AccountTypeEdit(item);
        }
        private void CreateItem()
        {
            DataAccess da = new DataAccess();
            account_type item = new account_type();
            item.name = edtName.Text;
            item.description = edtDescr.Text;
            da.AccountTypeCreate(item);
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
