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
using bycar3.External_Code;
using bycar3.Core;

namespace bycar3.Views.Common
{
    /// <summary>
    /// Interaction logic for SimpleSpareSelectView.xaml
    /// </summary>
    public partial class SimpleSpareSelectView : Window
    {
        public SimpleSpareSelectView()
        {
            InitializeComponent();
        }
        void LoadSpares()
        {
            LoadSpares(null, "");
        }
        //public int SpareID = 0;
        public SpareView SV = null;
        DataAccess da = new DataAccess();
        void LoadSpares(int? searchFieldIndex, string searchString)
        {
            da = new DataAccess();
            List<SpareView> items = new List<SpareView>();
            if (searchFieldIndex.HasValue && !searchString.Equals(""))
            {
                items = SpareContainer.Instance.GetSpares(searchFieldIndex.Value, searchString);
            }
            else
            {
                items = SpareContainer.Instance.Spares;
            }
            dgSpares.DataContext = items;
        }

        void SpareSearch()
        {
            if (edtSearchText != null)
            {
                string searchString = edtSearchText.Text;
                int searchFieldIndex = edtSearchField.SelectedIndex;
                LoadSpares(searchFieldIndex, searchString);
            }
        }

        void SpareAdd()
        {
            Marvin.Instance.SpareCreate();
            LoadSpares();
        }

        void SpareEdit()
        {
            if (dgSpares.SelectedItem == null)
                return;
            int SpareID = (dgSpares.SelectedItem as SpareView).id;
            Marvin.Instance.SpareEdit(SpareID);
            LoadSpares();
        }

        void SpareDelete()
        {
            int id = 0;
            SpareView b = null;
            if (dgSpares.SelectedItem != null)
            {
                object sel = dgSpares.SelectedItem;
                b = (SpareView)sel;
                id = b.id;
            }
            if (id > 0)
            {
                MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выделенную запись?", "Удаление!", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    DataAccess da = new DataAccess();
                    da.SpareDelete(id);
                    SpareContainer.Instance.Remove(id);
                    LoadSpares();
                }
            }
        }
        string GetSelectedSpareName()
        {
            string result = "";
            try
            {
                if (dgSpares.SelectedItem != null)
                {
                    object sel = dgSpares.SelectedItem;
                    result = ((SpareView)sel).name;
                }
                else
                {
                    //MessageBox.Show("Сначала выберите деталь из списка");
                }
            }
            catch (Exception)
            {
                LoadSpares();
                return "";
            }
            return result;
        }

        int GetSelectedSpareId()
        {
            int result = 0;
            try
            {
                if (dgSpares.SelectedItem != null)
                {
                    object sel = dgSpares.SelectedItem;
                    result = ((SpareView)sel).id;
                }
                else
                {
                    //MessageBox.Show("Сначала выберите деталь из списка");
                }
            }
            catch (Exception)
            {
                LoadSpares();
                return -1;
            }
            return result;
        }
        void ShowSelectedSpareName()
        {
            edtOfferingName.Content = GetSelectedSpareName();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSpares();
        }

        private void btnSpareSearch_Click(object sender, RoutedEventArgs e)
        {
            SpareSearch();
        }

        private void btnSpareAdd_Click(object sender, RoutedEventArgs e)
        {
            SpareAdd();
        }

        private void btnSpareEdit_Click(object sender, RoutedEventArgs e)
        {
            SpareEdit();
        }

        private void btnSpareDelete_Click(object sender, RoutedEventArgs e)
        {
            SpareDelete();
        }

        private void dgSpares_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(dgSpares.SelectedItem != null)
                SV = dgSpares.SelectedItem as SpareView;
            Close();
        }

        private void dgSpares_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowSelectedSpareName();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void edtSearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            SpareSearch();
        }

        private void edtSearchText_TextInput(object sender, TextCompositionEventArgs e)
        {
            SpareSearch();
        }

        private void edtSearchField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SpareSearch();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            edtSearchText.Focus();
        }
        private void SearchTextBox_Search(object sender, RoutedEventArgs e)
        {
            SpareSearch();
        } 
    }
}
