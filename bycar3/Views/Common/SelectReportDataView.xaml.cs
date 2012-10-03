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

namespace bycar3.Views.Common
{
    /// <summary>
    /// Interaction logic for SelectReportDataView.xaml
    /// </summary>
    public partial class SelectReportDataView : Window
    {
        public DateTime? ReportDate = null;
        public DateTime? ReportDateTo = null;
        public int WarehouseID;

        public SelectReportDataView()
        {
            InitializeComponent();
            edtReportDate.SelectedDate = DateTime.Now;
            edtReportDateTo.SelectedDate = DateTime.Now;

            LoadWarehouses();
        }
        void LoadWarehouses()
        {
            DataAccess da = new DataAccess();
            List<warehouse> l = da.GetWarehouses();
            warehouse w = new warehouse();
            w.name = "Все склады";
            l.Add(w);
            edtWarehouse.DataContext = l;
            edtWarehouse.SelectedItem = w;
        }   
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (edtReportDate.SelectedDate.HasValue && edtReportDateTo.SelectedDate.HasValue)
            {
                ReportDate = edtReportDate.SelectedDate;
                ReportDateTo = edtReportDateTo.SelectedDate;
                WarehouseID = (edtWarehouse.SelectedItem as warehouse).id;
                Close();
            }
            else
            {
                MessageBox.Show("Укажите корректную дату!");
            }
        }
    }
}
