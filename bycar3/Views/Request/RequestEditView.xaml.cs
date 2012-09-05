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
using bycar3.Reporting;
using bycar;

namespace bycar3.Views.Request
{
    /// <summary>
    /// Interaction logic for RequestEditView.xaml
    /// </summary>
    public partial class RequestEditView : Window
    {
        public List<SpareView> items = null;
        public void RefreshSpares()
        {
            dgSpares.DataContext = null;
            decimal sum1 = 0;
            for (int i = 0; i < items.Count; i++)
            {
                if (!items[i].QRest.HasValue)
                    items[i].QRest = 0;
                items[i].demand = ((int)items[i].q_demand.Value - (int)items[i].QRest.Value);
                sum1 += items[i].demand.Value;
            }
            dgSpares.DataContext = items;
            dgSpares.UpdateLayout();
        }
        void LoadSpares()
        {
            decimal sum1 = 0;
            DataAccess da = new DataAccess();
            items = da.GetSparesDemand();
            for (int i = 0; i < items.Count; i++)
            {
                if (!items[i].QRest.HasValue)
                    items[i].QRest = 0;
                items[i].demand = ((int)items[i].q_demand.Value - (int)items[i].QRest.Value);
                sum1 += items[i].demand.Value;
            }
            dgSpares.DataContext = items;
        }

        public RequestEditView()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        public void UpdateGrid()
        {
            dgSpares.DataContext = items;
            dgSpares.UpdateLayout();
        }
        private void btnSpareAdd_Click(object sender, RoutedEventArgs e)
        {
            SpareInRequestSelectView v = new SpareInRequestSelectView(this);
            v.ShowDialog();            
        }

        void SpareDelete()
        {
            if ((dgSpares.SelectedItem as SpareView) != null)
            {
                items.Remove(dgSpares.SelectedItem as SpareView);
                RefreshSpares();
            }
        }
        private void btnSpareDelete_Click(object sender, RoutedEventArgs e)
        {
            SpareDelete();
        }

        private void dgSpares_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            HandleMainDataGridCellEditEnding(sender, e);
            SpareView sv = (SpareView)e.Row.DataContext;
            if (sv.id > 0)
                if (sv.q_demand.HasValue)
                {
                    sv.demand = (int)sv.q_demand - (int)sv.QRest;
                    if (sv.demand < 0)
                        sv.demand = 0;
                }
        }
        private bool isManualEditCommit;
        private void HandleMainDataGridCellEditEnding(
         object sender, DataGridCellEditEndingEventArgs e)
        {
            if (!isManualEditCommit)
            {
                isManualEditCommit = true;
                DataGrid grid = (DataGrid)sender;
                grid.CommitEdit(DataGridEditingUnit.Row, true);
                isManualEditCommit = false;
            }
        }
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            Reporter.GenerateRequestReport(items);
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSpares();
        }
    }
}
