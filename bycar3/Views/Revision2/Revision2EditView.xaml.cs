using System;
using System.Windows;
using System.Windows.Controls;
using bycar;

namespace bycar3.Views.Revision2
{
    /// <summary>
    /// Interaction logic for Revision2EditView.xaml
    /// </summary>
    public partial class Revision2EditView : Window
    {
        private DataAccess da = new DataAccess();
        private revision Item = null;
        private bool IsNew = false;

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

        private void Load()
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

        private void Save()
        {
            Save(false);
        }

        private void Save(bool OkPressed)
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

        private void Delete()
        {
            da.RevisionDelete(Item.id);
        }
    }
}