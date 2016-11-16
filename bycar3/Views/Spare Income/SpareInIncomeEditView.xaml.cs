using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using bycar;
using bycar3.External_Code;

namespace bycar3.Views.Spare_Income
{
    /// <summary>
    /// Interaction logic for SpareInIncomeEditView.xaml
    /// </summary>
    public partial class SpareInIncomeEditView : Window
    {
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
            LoadVATs();
            if (_SpareName.Length > 35)
            {
                _SpareName = _SpareName.Remove(35);
                _SpareName += "...";
            }
            if (_OfferingID > 0)
            {
                spare_in_spare_income i = da.InOfferingGet(_OfferingID);
                i.spareReference.Load();
                i.vat_rateReference.Load();
                edtMakeup.Text = i.Markup.ToString();
                edtPrice.Text = i.PIn.ToString();
                edtQ.Text = i.QIn.ToString();
                edtSpareName.Content = i.spare.name;
                edtVAT.SelectedItem = i.vat_rate.name;
                decimal PriceFull = i.POut.Value;
                edtPriceFull.Text = PriceFull.ToString();
                edtTotalSum.Text = i.S.ToString();
            }
            else
            {
                edtQ.Text = "0";
                edtPriceFull.Text = "0";
                edtMakeup.Text = "0";
                edtPrice.Text = "0";
                edtSpareName.Content = _SpareName;
            }
        }

        private void CheckQuantity()
        {
            double x = 0;
            if (Double.TryParse(edtQ.Text, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out x))
                bQuantity.Visibility = System.Windows.Visibility.Hidden;
            else
                bQuantity.Visibility = System.Windows.Visibility.Visible;
            if (x >= 0)
                bQuantity.Visibility = System.Windows.Visibility.Hidden;
            else
                bQuantity.Visibility = System.Windows.Visibility.Visible;
        }

        private void CheckPrice()
        {
            double x = 0;
            if (Double.TryParse(edtPrice.Text, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out x))
                bPrice.Visibility = System.Windows.Visibility.Hidden;
            else
                bPrice.Visibility = System.Windows.Visibility.Visible;
            if (x >= 0)
                bPrice.Visibility = System.Windows.Visibility.Hidden;
            else
                bPrice.Visibility = System.Windows.Visibility.Visible;
        }

        private void CheckPriceFull()
        {
            double x = 0;
            if (Double.TryParse(edtPriceFull.Text, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out x))
                bPriceFull.Visibility = System.Windows.Visibility.Hidden;
            else
                bPriceFull.Visibility = System.Windows.Visibility.Visible;
            if (x >= 0)
                bPriceFull.Visibility = System.Windows.Visibility.Hidden;
            else
                bPriceFull.Visibility = System.Windows.Visibility.Visible;
        }

        private void CheckMakeup()
        {
            double x = -1;
            if (Double.TryParse(edtMakeup.Text, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out x))
                bMakeup.Visibility = System.Windows.Visibility.Hidden;
            else
                bMakeup.Visibility = System.Windows.Visibility.Visible;
            if (x >= 0)
                bMakeup.Visibility = System.Windows.Visibility.Hidden;
            else
                bMakeup.Visibility = System.Windows.Visibility.Visible;
        }

        private void CalculateSum(int param)
        {
            if (bPrice.Visibility == System.Windows.Visibility.Hidden
                && bQuantity.Visibility == System.Windows.Visibility.Hidden

                //                && bMakeup.Visibility == System.Windows.Visibility.Hidden
                && bPriceFull.Visibility == System.Windows.Visibility.Hidden)
            {
                if (!EditMode)
                {
                    EditMode = true;
                    decimal quantity = 0;
                    decimal price = 0;
                    decimal markup = 0;
                    decimal fullprice = 0;
                    decimal.TryParse(edtQ.Text, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out quantity);
                    decimal.TryParse(edtPrice.Text, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out price);
                    decimal.TryParse(edtMakeup.Text, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out markup);
                    decimal.TryParse(edtPriceFull.Text, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out fullprice);

                    // get VAR RATE
                    decimal RATE = 0;
                    decimal.TryParse(edtVAT.SelectedItem.ToString().Replace('%', ' '), out RATE);
                    decimal s1 = 0;
                    decimal sm = 0;
                    decimal sv = 0;
                    switch (param)
                    {
                        case 0: // иземенено количество
                            s1 = quantity * price;
                            sm = s1 * markup / 100;
                            sv = s1 * RATE / 100;
                            sum = s1 + sm + sv;
                            fullprice = quantity == 0 ? 0 : (sum / quantity);
                            edtPriceFull.Text = fullprice.ToString();
                            sum = fullprice * quantity;
                            break;

                        case 1: // иземенено цена за единицу
                            s1 = quantity * price;
                            sm = s1 * markup / 100;
                            sv = s1 * RATE / 100;
                            sum = s1 + sm + sv;
                            fullprice = quantity == 0 ? 0 : (sum / quantity);
                            edtPriceFull.Text = fullprice.ToString();
                            sum = fullprice * quantity;
                            break;

                        case 2: // иземенено процент надбавки
                            s1 = quantity * price;
                            sm = s1 * markup / 100;
                            sv = s1 * RATE / 100;
                            sum = s1 + sm + sv;
                            fullprice = quantity == 0 ? 0 : (sum / quantity);
                            edtPriceFull.Text = fullprice.ToString();
                            sum = fullprice * quantity;
                            break;

                        case 3: // иземенено НДС
                            s1 = quantity * price;
                            sm = s1 * markup / 100;
                            sv = s1 * RATE / 100;
                            sum = s1 + sm + sv;
                            fullprice = quantity == 0 ? 0 : (sum / quantity);
                            edtPriceFull.Text = fullprice.ToString();
                            sum = fullprice * quantity;
                            break;

                        case 4: // иземенено отпускная цена
                            sum = quantity * fullprice;

                            //if (price != null)
                            markup = (int)(100 * fullprice / price - 100 - RATE);

                            //else
                            //  markup = 0;
                            edtMakeup.Text = markup.ToString();
                            break;
                    }
                    decimal fpbefore = fullprice;
                    sum = Math.Round(sum, 2);
                    fullprice = Math.Round(fullprice, 2);
                    edtTotalSum.Text = sum.ToString();
                    if (fpbefore != fullprice)
                        edtPriceFull.Text = fullprice.ToString();
                    EditMode = false;
                }
            }
            else
            {
                edtTotalSum.Text = "0";
            }
        }

        private bool CreateItem()
        {
            spare_in_spare_income offering = getItemFromFields();
            string vat = edtVAT.SelectedItem.ToString();
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
            string vat = edtVAT.SelectedItem.ToString();
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
                item.Markup = decimal.Parse(edtMakeup.Text, CultureInfo.InvariantCulture);
                item.num = _OfferingNumber + 1;
                decimal p = decimal.Parse(edtPrice.Text, CultureInfo.InvariantCulture);
                item.PIn = p;
                item.QIn = decimal.Parse(edtQ.Text, CultureInfo.InvariantCulture);
                p = decimal.Parse(edtPriceFull.Text, CultureInfo.InvariantCulture);
                item.POut = p;
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

        private void LoadVATs()
        {
            edtVAT.Items.Clear();
            var items = da.GetVATRates();
            foreach (vat_rate i in items)
            {
                edtVAT.Items.Add(i.name);
            }
            edtVAT.SelectedItem = "Без НДС";
        }

        private void CreateVatRate()
        {
            EditMode = true;
            VATRatesEditView v = new VATRatesEditView();
            v._id = -1;
            v.ShowDialog();
            LoadVATs();
            EditMode = false;
            CalculateSum(0);
        }

        #endregion FUNCTONS

        // HANDLERS
        public SpareInIncomeEditView()
        {
            InitializeComponent();
        }

        private void edtQ_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckQuantity();
            CalculateSum(0);
        }

        private void edtPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckPrice();
            CalculateSum(1);
        }

        private void edtMakeup_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckMakeup();
            CalculateSum(2);
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

        private void btnAddVAT_Click(object sender, RoutedEventArgs e)
        {
            CreateVatRate();
        }

        private void edtVAT_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateSum(3);
        }

        private void edtQ_GotFocus(object sender, RoutedEventArgs e)
        {
            if (edtQ.Text == "0")
            {
                edtQ.SelectAll();
            }
        }

        private void edtPrice_GotFocus(object sender, RoutedEventArgs e)
        {
            if (edtPrice.Text == "0")
            {
                edtPrice.SelectAll();
            }
        }

        private void edtMakeup_GotFocus(object sender, RoutedEventArgs e)
        {
            if (edtMakeup.Text == "0")
            {
                edtMakeup.SelectAll();
            }
        }

        private void edtQ_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                edtPrice.Focus();
        }

        private void edtPrice_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                edtMakeup.Focus();
        }

        private void edtMakeup_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                edtPriceFull.Focus();
        }

        private void edtPriceFull_KeyUp(object sender, KeyEventArgs e)
        {
            string before = edtPriceFull.Text;
            if (e.Key == Key.Enter)
                btnOk.Focus();
            string after = edtPriceFull.Text;
        }

        private void edtPriceFull_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            string before = edtPriceFull.Text;
            CheckPriceFull();
            CalculateSum(4);
            string after = edtPriceFull.Text;
        }

        private void edtPriceFull_GotFocus(object sender, RoutedEventArgs e)
        {
            string before = edtPriceFull.Text;
            if (edtPriceFull.Text == "0")
            {
                edtPriceFull.SelectAll();
            }
            string after = edtPriceFull.Text;
        }

        private void edtPriceFull_KeyDown(object sender, KeyEventArgs e)
        {
            string before = edtPriceFull.Text;
            CheckPriceFull();
            CalculateSum(4);
            string after = edtPriceFull.Text;
        }

        private void edtPriceFull_TextChanged(object sender, TextChangedEventArgs e)
        {
            string before = edtPriceFull.Text;
            CheckPriceFull();
            CalculateSum(4);
            string after = edtPriceFull.Text;
        }
    }
}