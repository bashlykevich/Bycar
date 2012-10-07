using System.Windows;
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