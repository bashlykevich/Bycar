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
using bycar3.Views.Spare_Income;
using bycar3.Views.Spare_Outgo;

namespace bycar3.Views.Common
{
    /// <summary>
    /// Interaction logic for SelectSpareMovementTypeView.xaml
    /// </summary>
    public partial class SelectSpareMovementTypeView : Window
    {
        public SelectSpareMovementTypeView()
        {
            InitializeComponent();
        }

        private void btnIncome_Click(object sender, RoutedEventArgs e)
        {
            SpareIncomeEditView v1 = new SpareIncomeEditView();
            this.Close();
            v1.ShowDialog();
            
        }

        private void btnOutgo_Click(object sender, RoutedEventArgs e)
        {
            SpareOutgoEditView v2 = new SpareOutgoEditView();
            this.Close();
            v2.ShowDialog();            
        }
    }
}
