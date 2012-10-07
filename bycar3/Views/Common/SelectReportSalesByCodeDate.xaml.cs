using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using bycar;

namespace bycar3.Views.Common
{
    /// <summary>
    /// Interaction logic for SelectReportSalesByCodeDate.xaml
    /// </summary>
    public partial class SelectReportSalesByCodeDate : Window
    {
        public DateTime? DateFrom = null;
        public DateTime? DateTo = null;
        public int SpareID = 0;
        public int WarehouseID = 0;

        public SelectReportSalesByCodeDate()
        {
            InitializeComponent();
            edtReportDateFrom.SelectedDate = DateTime.Now;
            edtReportDateTo.SelectedDate = DateTime.Now;
            LoadWarehouses();
        }

        private void LoadWarehouses()
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
            if (SpareID == 0)
                MessageBox.Show("Проверьте правильность введённых данных!");
            else
            {
                DateFrom = edtReportDateFrom.SelectedDate;
                DateTo = edtReportDateTo.SelectedDate;
                WarehouseID = (edtWarehouse.SelectedItem as warehouse).id;
                Close();
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            SimpleSpareSelectView v = new SimpleSpareSelectView();
            bool? res = v.ShowDialog();
            if (v.SV != null)
            {
                SpareID = v.SV.id;
                edtSpare.Text = v.SV.name + " (" + v.SV.codeShatem + ")";
            }
        }
    }
}