using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using bycar;
using bycar3.External_Code;

namespace bycar3.Views.Invoice
{
    /// <summary>
    /// Interaction logic for SpareInInvoiceSelectView2.xaml
    /// </summary>
    public partial class SpareInInvoiceSelectView2 : Window
    {
        #region MEMBERS

        public int _InvoiceID = -1;
        public string CurrentCurrencyCode = "";

        #endregion MEMBERS

        #region CUSTOM FUNCTION

        private void LoadSpares()
        {
            dgSpares.DataContext = SpareContainer.Instance.Spares.Where(i => i.QRest > 0).ToList();
            dgSpares.SelectedIndex = 0;
        }

        private void SpareSearch()
        {
            if (edtSearchText != null)
            {
                if (edtSearchText.Text.Length < 1)
                    LoadSpares();
                else
                    LoadSpares(edtSearchField.SelectedIndex, edtSearchText.Text);
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
            CurrentCurrencyCode = "BYR";
            DataAccess da = new DataAccess();
            List<SpareInSpareIncomeView> lst = da.GetIncomes(SpareID);
            foreach (SpareInSpareIncomeView i in lst)
            {
                i.DF_PriceInCurrency = CurrencyHelper.GetPrice(CurrentCurrencyCode, i.POutBasic.Value);
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
        }

        private void CreateOutgo()
        {
            int SpareID = (dgIncomes.SelectedItem as SpareInSpareIncomeView).SpareID.Value;
            SpareInInvoiceEditView v = new SpareInInvoiceEditView();
            v._SpareName = (dgIncomes.SelectedItem as SpareInSpareIncomeView).SpareName;
            v.Q = (dgIncomes.SelectedItem as SpareInSpareIncomeView).QRest.Value;
            v.P = (dgIncomes.SelectedItem as SpareInSpareIncomeView).DF_PriceInCurrency.Value;
            v._SpareID = SpareID;
            v._InvoiceID = this._InvoiceID;
            v._SpareInSpareIncomeID = (dgIncomes.SelectedItem as SpareInSpareIncomeView).id;

            //v._AvailableQuantity = (dgIncomes.SelectedItem as SpareInSpareIncomeView).QRest.Value;
            //v._SpareOutgoID = _SpareOutgoID;
            //v._Price = (dgIncomes.SelectedItem as SpareInSpareIncomeView).PriceInCurrency.Value;
            //v.CurrentCurrencyCode = this.CurrentCurrencyCode;
            v.ShowDialog();
            DataAccess da = new DataAccess();
            SpareContainer.Instance.Update(SpareID);
            dgSpares.DataContext = SpareContainer.Instance.Spares.Where(i => i.QRest > 0).ToList();
        }

        #endregion CUSTOM FUNCTION

        public SpareInInvoiceSelectView2()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSpares();
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