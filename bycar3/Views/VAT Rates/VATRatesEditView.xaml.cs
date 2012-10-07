﻿using System.Windows;
using bycar;

namespace bycar3.Views
{
    /// <summary>
    /// Interaction logic for VATRatesEditView.xaml
    /// </summary>
    public partial class VATRatesEditView : Window
    {
        public int _id = 0;

        public VATRatesEditView()
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
            da.VATRateEdit(getItemFromFields());
        }

        private void CreateItem()
        {
            DataAccess da = new DataAccess();
            da.VATRateCreate(getItemFromFields());
        }

        private vat_rate getItemFromFields()
        {
            vat_rate item = new vat_rate();
            item.id = this._id;
            item.name = edtName.Text;
            item.rate = 0;
            decimal tmp = 0;
            decimal.TryParse(edtRate.Text, out tmp);
            item.rate = tmp;
            return item;
        }
    }
}