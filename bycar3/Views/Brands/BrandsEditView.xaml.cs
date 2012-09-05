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
    /// Interaction logic for BrandsEditView.xaml
    /// </summary>
    public partial class BrandsEditView : Window
    {
        public int _id = 0;
        public BrandsEditView()
        {
            InitializeComponent();
        }

        public void LoadItem(int id)
        {
            _id = id;
            DataAccess da = new DataAccess();
            brand b = da.GetBrand(id);
            edtName.Text = b.name;
            edtDescr.Text = b.description;
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
            brand item = new brand();
            item.id = _id;
            item.name = edtName.Text;
            item.description = edtDescr.Text;
            da.BrandEdit(item);
        }
        private void CreateItem()
        {
            DataAccess da = new DataAccess();
            brand item = new brand();
            item.name = edtName.Text;
            item.description = edtDescr.Text;
            da.BrandCreate(item);
        }
    }
}
