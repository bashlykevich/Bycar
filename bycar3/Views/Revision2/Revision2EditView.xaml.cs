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

namespace bycar3.Views.Revision2
{
    /// <summary>
    /// Interaction logic for Revision2EditView.xaml
    /// </summary>
    public partial class Revision2EditView : Window
    {
        DataAccess da = new DataAccess();
        revision Item = null;
        bool IsNew = false;

        public Revision2EditView()
        {
            InitializeComponent();
        }
        public Revision2EditView(int ItemID)
        {
            Item = da.RevisionGet(ItemID);
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Load();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Save();
        }

        private void dgSpares_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {

        }

        private void btnSpareDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSpareAdd_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Save(true);
            Close();
        }

        void Load()
        {
            if (Item == null)
            {
                // IS NEW
                IsNew = true;
                Item = new revision();
                
                edtNumber.Text = da.revisionGetMaxId().ToString();
                edtDate.SelectedDate = DateTime.Now;            

            }
            else
            {
                edtNumber.Text = Item.id.ToString();
                edtDate.SelectedDate = Item.RevisionDate;            
            }                       
        }
        void Save()
        {
            Save(false);
        }
        void Save(bool OkPressed)
        {
            if (IsNew && !OkPressed)
            {
                Delete();
            }
            else
            {
                Item.RevisionDate = edtDate.SelectedDate;
                if (IsNew)
                {
                    Item = da.RevisionCreate(Item);
                    IsNew = false;
                }
                else
                {
                    da.revisionEdit(Item);
                }
            }
        }
        void Delete()
        {
            da.RevisionDelete(Item.id);
        }
    }
}
