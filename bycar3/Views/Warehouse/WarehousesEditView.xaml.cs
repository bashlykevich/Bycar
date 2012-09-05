﻿using System;
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
    /// Interaction logic for WarehousesEditView.xaml
    /// </summary>
    public partial class WarehousesEditView : Window
    {
        public int _id = 0;
        public WarehousesEditView()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void LoadItem(int id)
        {
            _id = id;
            DataAccess da = new DataAccess();
            warehouse b = da.GetWarehouse(id);
                edtName.Text = b.name;
                edtDescr.Text = b.description;
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
            da.WarehouseEdit(getItemFromFields());
        }
        private void CreateItem()
        {
            DataAccess da = new DataAccess();            
            da.WarehouseCreate(getItemFromFields());
        }
        warehouse getItemFromFields()
        {
            warehouse item = new warehouse();
            item.id = this._id;
            item.name = edtName.Text;
            item.description = edtDescr.Text;
            return item;
        }
    }
}
