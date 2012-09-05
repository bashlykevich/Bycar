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

namespace bycar3.Views.Request
{
    /// <summary>
    /// Interaction logic for SpareInRequestSelectView.xaml
    /// </summary>
    public partial class SpareInRequestSelectView : Window
    {
        #region MEMBERS

        public RequestEditView ParentWindow = null;
        DataAccess da = null;
        public int _ParentItemID = -1;

        #endregion
        public SpareInRequestSelectView(RequestEditView pw)
        {
            InitializeComponent();
            ParentWindow = pw;
        }
        #region CUSTOM FUNCTION
        void LoadSpares()
        {
            dgSpares.DataContext = SpareContainer.Instance.Spares.ToList();
            dgSpares.SelectedIndex = 0;
        }

        void SpareSearch()
        {
            if (edtSearchText != null)
            {
                if (edtSearchText.Text.Length < 1)
                    LoadSpares();
                else
                    LoadSpares(edtSearchField.SelectedIndex, edtSearchText.Text);
            }
        }
        void LoadSpares(int SearchFieldIndex, string SearchText)
        {
            dgSpares.DataContext = SpareContainer.Instance.GetSpares(SearchFieldIndex, SearchText, true, 0, "");
            if (dgSpares.Items.Count > 0)
                dgSpares.SelectedIndex = 0;
        }
        void AddSelectedSpareToRequest()
        {
            // получаем ID запчасти
            if ((dgSpares.SelectedItem as SpareView) != null)
            {
                ParentWindow.items.Add((dgSpares.SelectedItem as SpareView));
                ParentWindow.RefreshSpares();
                ParentWindow.dgSpares.UpdateLayout();
                //ParentWindow.dgSpares.ScrollIntoView(dgSpares.Items[dgSpares.Items.Count - 1]);
            }
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSpares();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void dgIncomes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AddSelectedSpareToRequest();
        }

        private void edtSearchField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SpareSearch();
        }

        private void edtSearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            SpareSearch();
        }

        private void edtSearchText_TextInput(object sender, TextCompositionEventArgs e)
        {
            SpareSearch();
        }
        private void SearchTextBox_Search(object sender, RoutedEventArgs e)
        {
            SpareSearch();
        }
    }
}
