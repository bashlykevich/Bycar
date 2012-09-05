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

        public SelectReportSalesByCodeDate()
        {
            InitializeComponent();
            edtReportDateFrom.SelectedDate = DateTime.Now;
            edtReportDateTo.SelectedDate = DateTime.Now;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (SpareID == 0)
                MessageBox.Show("Проверьте правильность введённых данных!");
            else
            {
                DateFrom = edtReportDateFrom.SelectedDate;
                DateTo = edtReportDateTo.SelectedDate;
                Close();
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            SimpleSpareSelectView v = new SimpleSpareSelectView();
            bool? res = v.ShowDialog();
            if(v.SV != null)
            {
                SpareID = v.SV.id;
                edtSpare.Text = v.SV.name + " (" + v.SV.codeShatem + ")";
            }
        }
    }
}
