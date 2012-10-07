using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using bycar;
using bycar3.External_Code;

namespace bycar3.Views.Spare_Outgo
{
    /// <summary>
    /// Interaction logic for SpareInOutgoSelectView.xaml
    /// </summary>
    public partial class SpareInOutgoSelectView : Window
    {
        #region MEMBERS

        public int _SpareOutgoID = -1;
        public string CurrentCurrencyCode = "";
        public SpareOutgoEditView ParentWindow = null;

        #endregion MEMBERS

        #region CUSTOM FUNCTION

        private void LoadAllSpares()
        {
            if (dgSpares != null)
            {
                dgSpares.DataContext = SpareContainer.Instance.Spares.Where(i => i != null).Where(i => i.QRest > 0).ToList();
                dgSpares.SelectedIndex = 0;
            }
        }

        private void LoadSpares()
        {
            if (edtSearchText != null)
            {
                if (edtSearchText.Text.Length < 1)
                {
                    LoadAllSpares();
                }
                else
                    LoadSpares(edtSearchField.SelectedIndex, edtSearchText.Text);
            }
            else
            {
                LoadAllSpares();
            }
        }

        private void LoadSpares(int SearchFieldIndex, string SearchText)
        {
            dgSpares.DataContext = SpareContainer.Instance.GetSpares(SearchFieldIndex, SearchText, true, 0, "");
            if (dgSpares.Items.Count > 0)
                dgSpares.SelectedIndex = 0;
        }

        private void LoadIncomes(int SpareID)
        {
            DataAccess da = new DataAccess();
            List<SpareInSpareIncomeView> lst = da.GetIncomes(SpareID);
            foreach (SpareInSpareIncomeView i in lst)
            {
                decimal pr1 = CurrencyHelper.GetPrice(CurrentCurrencyCode, i.POutBasic.Value);
                if (CurrentCurrencyCode == "BYR")
                {
                    decimal tmpd = Math.Round(pr1 / 50, 0);
                    pr1 = tmpd * 50;
                }
                i.DF_PriceInCurrency = pr1;
            }
            dgIncomes.DataContext = lst;
        }

        private void SpareSelectionChange()
        {
            if (dgSpares.SelectedItem != null)
            {
                int SpareID = (dgSpares.SelectedItem as SpareView).id;
                LoadIncomes(SpareID);
            }
            else
            {
                dgIncomes.DataContext = null;
                dgIncomes.UpdateLayout();
            }
        }

        private void CreateOutgo()
        {
            if (dgIncomes.SelectedItem != null)
            {
                SpareInOutgoEditView v = new SpareInOutgoEditView();
                v._SpareName = (dgIncomes.SelectedItem as SpareInSpareIncomeView).SpareName;
                v._SpareInSpareIncomeID = (dgIncomes.SelectedItem as SpareInSpareIncomeView).id;
                v._AvailableQuantity = (dgIncomes.SelectedItem as SpareInSpareIncomeView).QRest.Value;
                v._SpareOutgoID = _SpareOutgoID;
                v._Price = (dgIncomes.SelectedItem as SpareInSpareIncomeView).DF_PriceInCurrency.Value;
                v.CurrentCurrencyCode = this.CurrentCurrencyCode;
                v.ShowDialog();
                int SpareID = (dgIncomes.SelectedItem as SpareInSpareIncomeView).SpareID.Value;
                DataAccess da = new DataAccess();
                SpareContainer.Instance.Update(da.GetSpareView(SpareID));
                if (ParentWindow != null)
                {
                    ParentWindow.LoadOfferings();
                    ParentWindow.dgSpares.UpdateLayout();
                    ParentWindow.dgSpares.ScrollIntoView(dgSpares.Items[dgSpares.Items.Count - 1]);
                }
                LoadSpares();
            }
        }

        #endregion CUSTOM FUNCTION

        // HANDLERS
        public SpareInOutgoSelectView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAllSpares();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void dgSpares_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SpareSelectionChange();
        }

        private void dgIncomes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CreateOutgo();
            SpareSelectionChange();
        }

        private void edtSearchField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSpares();
        }

        private void edtSearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadSpares();
        }

        private void edtSearchText_TextInput(object sender, TextCompositionEventArgs e)
        {
            LoadSpares();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            edtSearchText.Focus();
        }

        private void SearchTextBox_Search(object sender, RoutedEventArgs e)
        {
            LoadSpares();
        }
    }
}