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
    /// Interaction logic for ContactsEditView.xaml
    /// </summary>
    public partial class ContactsEditView : Window
    {
        public int _id = 0;
        public ContactsEditView()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
            //da.ContactEdit(getItemFromFields());
        }
        private void CreateItem()
        {
            DataAccess da = new DataAccess();            
            //da.ContactCreate(getItemFromFields());
        }
        /*
        contact getItemFromFields()
        {            
            contact item = new contact();
            item.id = _id;
            item.name = edtName.Text;
            item.passort_date = edtDate.SelectedDate;
            item.passport_by = edtPassportBy.Text;
            item.passport_number = edtNumber.Text;
            item.passport_series = edtSeries.Text;
            return item;
        }*/
    }
}
