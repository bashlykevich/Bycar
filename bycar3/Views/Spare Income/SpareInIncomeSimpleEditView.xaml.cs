using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using bycar;
using bycar3.External_Code;

namespace bycar3.Views.Spare_Income
{
    /// <summary>
    /// Interaction logic for SpareInIncomeSimpleEditView.xaml
    /// </summary>
    public partial class SpareInIncomeSimpleEditView : Window
    {
        public SpareInIncomeSimpleEditView()
        {
            InitializeComponent();
        }

        #region MEMBERS

        private DataAccess da = new DataAccess();
        public int _OfferingID = -1;
        public int _SpareID = -1;
        public string _SpareName = "";
        public int _SpareIncomeID = -1;
        public int _OfferingNumber = 0;
        public string CurrentCurrencyCode = "";
        public bool IsSimpleInput = false;
        private bool EditMode = false;

        private decimal sum = 0;

        #endregion MEMBERS

        #region FUNCTONS

        private void LoadOffering()
        {
            if (_SpareName.Length > 20)
            {
                _SpareName = _SpareName.Remove(20);
                _SpareName += "...";
            }
            if (_OfferingID > 0)
            {
                spare_in_spare_income i = da.InOfferingGet(_OfferingID);
                i.spareReference.Load();
                i.vat_rateReference.Load();

                //edtMakeup.Text = ((int)i.Markup).ToString();
                edtPrice.Text = i.PIn.ToString();
                edtQ.Text = i.QIn.ToString();
                Title = i.spare.name;

                //edtVAT.SelectedItem = i.vat_rate.name;
                decimal PriceFull = i.POut.Value;
                edtPriceFull.Text = PriceFull.ToString();
                edtTotalSum.Text = i.S.ToString();
            }
            else
            {
                edtQ.Text = "0";
                edtPriceFull.Text = "0";

                //edtMakeup.Text = "0";
                edtPrice.Text = "0";
                Title = _SpareName;
            }
        }

        private void CalculateSum(int param)
        {
            if (!EditMode)
            {
                EditMode = true;
                decimal quantity = 0;
                decimal fullprice = 0;
                decimal.TryParse(edtQ.Text, out quantity);
                decimal.TryParse(edtPriceFull.Text, out fullprice);

                switch (param)
                {
                    case 0: // иземенено количество
                        sum = quantity * fullprice;
                        break;

                    case 4: // иземенено отпускная цена
                        sum = quantity * fullprice;
                        break;
                }
                sum = Math.Round(sum, 2);
                edtTotalSum.Text = sum.ToString();
                edtPrice.Text = fullprice.ToString();
                EditMode = false;
            }
        }

        private bool CreateItem()
        {
            spare_in_spare_income offering = getItemFromFields();
            string vat = "Без НДС";//edtVAT.SelectedItem.ToString();
            if (_SpareID > 0 && offering != null)
            {
                da.InOfferingCreate(offering, _SpareID, _SpareIncomeID, vat);
                SpareContainer.Instance.Update(_SpareID);
                return true;
            }
            else
                return false;
        }

        private bool EditItem()
        {
            spare_in_spare_income offering = getItemFromFields();
            string vat = "Без НДС";//edtVAT.SelectedItem.ToString();
            if (offering != null)
            {
                _SpareID = da.InOfferingGet(_OfferingID).spare.id;
                da.InOfferingEdit(offering, vat);
                SpareContainer.Instance.Update(_SpareID);
                return true;
            }
            else
                return false;
        }

        private bool SaveItem()
        {
            if (this._OfferingID > 0)
            {
                return EditItem();
                //return false;
            }
            else
            {
                return CreateItem();
            }
        }

        private spare_in_spare_income getItemFromFields()
        {
            spare_in_spare_income item = new spare_in_spare_income();
            try
            {
                // set currency
                item.CurrencyID = da.GetCurrency(this.CurrentCurrencyCode).id;
                item.id = this._OfferingID;
                item.Markup = 0;//Double.Parse(edtMakeup.Text);
                item.num = _OfferingNumber + 1;
                decimal p = decimal.Parse(edtPriceFull.Text);
                item.POut = p;
                item.PIn = p;
                item.QIn = decimal.Parse(edtQ.Text);
                item.S = sum;
                item.QRest = item.QIn;
            }
            catch (Exception)
            {
                MessageBox.Show("Проверьте правильность введенных данных");
                item = null;
            }
            return item;
        }

        #endregion FUNCTONS

        private void edtQ_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateSum(0);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOffering();
            edtQ.Focus();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (sum > 0)
            {
                if (SaveItem())
                    Close();
            }
            else
            {
                MessageBox.Show("Проверьте правильность заполнения полей!");
            }
        }

        private void edtPriceFull_TextChanged(object sender, TextChangedEventArgs e)
        {
            //CheckPriceFull();
            CalculateSum(4);
        }

        private void edtPriceFull_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //CheckPriceFull();
            CalculateSum(4);
        }

        private void edtPriceFull_KeyDown(object sender, KeyEventArgs e)
        {
            //CheckPriceFull();
            CalculateSum(4);
        }

        private void edtQ_GotFocus(object sender, RoutedEventArgs e)
        {
            if (edtQ.Text == "0")
            {
                edtQ.SelectAll();
            }
        }

        private void edtPriceFull_GotFocus(object sender, RoutedEventArgs e)
        {
            if (edtPriceFull.Text == "0")
            {
                edtPriceFull.SelectAll();
            }
        }

        private void edtQ_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                edtPriceFull.Focus();
        }

        private void edtPriceFull_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                btnOk.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
    }
}