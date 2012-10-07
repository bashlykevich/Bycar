using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using bycar;
using bycar3.External_Code;

namespace bycar3.Views.Revision
{
    /// <summary>
    /// Interaction logic for RevisionEditView.xaml
    /// </summary>
    public partial class RevisionEditView : Window
    {
        private DataAccess da = null;
        private int _SpareIncomeID = 0;
        private bool Ready = false;

        private void Initialize()
        {
            da = new DataAccess();
            _SpareIncomeID = da.getRemainsInputID();
            LoadOfferings();
            Ready = true;
        }

        private void LoadComboboxCurrency()
        {
            List<currency> items = da.GetCurrency();
            foreach (currency i in items)
            {
                edtCurrency.Items.Add(i.code);
            }
            edtCurrency.SelectedIndex = 0;
        }

        public void LoadOfferings()
        {
            da = new DataAccess();
            dgSpares.DataContext = da.GetRemains(this._SpareIncomeID);
        }

        public void LoadOfferings(int searchFieldIndex, string searchString)
        {
            da = new DataAccess();
            dgSpares.DataContext = da.GetRemains(this._SpareIncomeID, searchFieldIndex, searchString);
        }

        private void AddOffering()
        {
            SpareInRevisionSelectView v = new SpareInRevisionSelectView();
            v._ParentWindow = this;
            v._OfferingNumber = dgSpares.Items.Count;
            v._SpareIncomeID = _SpareIncomeID;
            v.CurrencyRateCode = edtCurrency.SelectedItem.ToString();
            v.ShowDialog();
        }

        private void DeleteOffering()
        {
            int offeringId = getSelectedOfferingId();
            if (offeringId > 0)
            {
                int SpareId = getSelectedSpareId();
                da.InOfferingDelete(offeringId);
                SpareContainer.Instance.Update(SpareId);
                LoadOfferings();
            }
        }

        private int getSelectedOfferingId()
        {
            int result = 0;
            try
            {
                if (dgSpares.SelectedItem != null)
                {
                    object sel = dgSpares.SelectedItem;
                    result = ((SpareInSpareIncomeView)sel).id;
                }
                else
                {
                    MessageBox.Show("Сначала выберите деталь из списка");
                }
            }
            catch (Exception)
            {
                LoadOfferings();
            }
            return result;
        }

        private int getSelectedSpareId()
        {
            int result = 0;
            try
            {
                if (dgSpares.SelectedItem != null)
                {
                    object sel = dgSpares.SelectedItem;
                    result = ((SpareInSpareIncomeView)sel).SpareID.Value;
                }
                else
                {
                    MessageBox.Show("Сначала выберите деталь из списка");
                }
            }
            catch (Exception)
            {
                LoadOfferings();
            }
            return result;
        }

        private void SpareSearch()
        {
            if (edtSearchText.Text.Length > 0)
            {
                string searchString = edtSearchText.Text;
                int searchFieldIndex = edtSearchField.SelectedIndex;
                LoadOfferings(searchFieldIndex, searchString);
            }
            else
            {
                LoadOfferings();
            }
        }

        private void SaveEditedOffering(int index)
        {
            SpareInSpareIncomeView s = dgSpares.Items[index] as SpareInSpareIncomeView;
            decimal q = s.QRest.Value;
            decimal p = s.PIn.Value;
            da = new DataAccess();
            currency_rate CRate = da.getCurrencyRate(edtCurrency.SelectedItem.ToString());
            decimal PriceBasic = p / CRate.rate;
            da.InOfferingEdit(s.id, q, p, PriceBasic);
        }

        // HANDLERS
        public RevisionEditView()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
            LoadComboboxCurrency();
        }

        private void btnSpareAdd_Click(object sender, RoutedEventArgs e)
        {
            AddOffering();
        }

        private void btnSpareDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteOffering();
        }

        private void edtSearchField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Ready)
                if (edtSearchText.Text.Length > 0)
                    SpareSearch();
        }

        private void edtSearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Ready)
                SpareSearch();
        }

        private void dgSpares_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            int index = e.Row.GetIndex();
            SpareInSpareIncomeView s = dgSpares.Items[index] as SpareInSpareIncomeView;
            int SpareId = s.SpareID.Value;
            decimal q = s.QRest.Value;
            decimal p = s.POut.Value;
            string ColumnName = e.Column.Header.ToString();
            string val = (e.EditingElement as TextBox).Text;
            if (ColumnName.Contains("Количество"))
            {
                decimal.TryParse(val, out q);
            }
            if (ColumnName.Contains("Цена"))
            {
                decimal.TryParse(val, out p);
            }
            da = new DataAccess();
            decimal BasicPrice = CurrencyHelper.GetBasicPrice(edtCurrency.SelectedItem.ToString(), p);
            da.InOfferingEdit(s.id, q, p, BasicPrice);
            SpareContainer.Instance.Update(SpareId);
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