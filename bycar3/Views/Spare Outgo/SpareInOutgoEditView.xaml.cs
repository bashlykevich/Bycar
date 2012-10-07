using System;
using System.Windows;
using System.Windows.Controls;
using bycar;
using bycar3.External_Code;

namespace bycar3.Views.Spare_Outgo
{
    /// <summary>
    /// Interaction logic for SpareInOutgoEditView.xaml
    /// </summary>
    public partial class SpareInOutgoEditView : Window
    {
        #region DATA MEMBERS

        public int _SpareID = -1;
        public string _SpareName = "";
        public int _SpareInSpareIncomeID = -1;
        public decimal _AvailableQuantity = 0;
        public decimal _Price = 1;
        public int _SpareOutgoID = -1;
        public string CurrentCurrencyCode = "";
        private bool Calculating = false;
        public SpareInOutgoSelectView ParentWindow = null;

        #endregion DATA MEMBERS

        #region CUSTOM FUNCTIONS

        private void PrepareWindow()
        {
            edtSpareName.Text = _SpareName;
            edtPrice.Text = _Price.ToString();
            edtDiscount.Text = "0";
            edtQuantity.Text = "1";
            edtQuantityAvalilable.Content += " [" + _AvailableQuantity.ToString() + "]";
            CalculateSum();
            this.Title += " (" + CurrentCurrencyCode + ")";
        }

        private void CalculateByPrice()
        {
            decimal dDicscount = 0;
            decimal Price = 0;
            decimal.TryParse(edtPrice.Text, out Price);
            dDicscount = _Price - Price;
            edtDiscount.Text = dDicscount.ToString();
        }

        private void CalculateByDiscount()
        {
            decimal dDicscount = 0;
            decimal Price = 0;
            decimal.TryParse(edtDiscount.Text, out dDicscount);
            Price = _Price - dDicscount;
            edtPrice.Text = Price.ToString();
        }

        private void CalculateSum()
        {
            decimal Q = 0;
            decimal P = 0;
            decimal S = 0;
            decimal D = 0;
            decimal T = 0;

            decimal.TryParse(edtQuantity.Text, out Q);
            if (Q > _AvailableQuantity)
            {
                Q = _AvailableQuantity;
                edtQuantity.Text = Q.ToString();
            }
            decimal.TryParse(edtPrice.Text, out P);
            decimal.TryParse(edtDiscount.Text, out D);
            S = Q * P;
            T = S - D;
            edtSum1.Text = S.ToString();
            edtTotal.Text = T.ToString();
        }

        private void CalculateSum2()
        {
            decimal Quantity = 0;
            decimal Discount = 0;
            decimal Price = 0;
            decimal Total = 0;
            decimal Sum1 = 0;
            decimal.TryParse(edtQuantity.Text, out Quantity);
            decimal.TryParse(edtPrice.Text, out Price);
            decimal.TryParse(edtDiscount.Text, out Discount);
            Sum1 = Quantity * Price;
            Total = Sum1 - Discount;
            edtSum1.Text = Sum1.ToString();
            edtTotal.Text = Total.ToString();
        }

        private bool CreateOutgo()
        {
            try
            {
                decimal quantity = _AvailableQuantity + 1;
                decimal.TryParse(edtQuantity.Text, out quantity);
                if (quantity > _AvailableQuantity || quantity <= 0)
                    return false;
                DataAccess da = new DataAccess();
                decimal Price = decimal.Parse(edtPrice.Text);
                decimal Discount = 0;
                decimal.TryParse(edtDiscount.Text, out Discount);
                decimal BasicPrice = CurrencyHelper.GetBasicPrice(CurrentCurrencyCode, Price);
                _SpareID = da.SpareInSpareOutgoCreate(_SpareInSpareIncomeID, quantity, _SpareOutgoID, Price, BasicPrice, Discount, 0);
                SpareContainer.Instance.Update(da.GetSpareView(_SpareID));
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        #endregion CUSTOM FUNCTIONS

        // HANDLERS
        public SpareInOutgoEditView()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (CreateOutgo())
                this.Close();
            else
                MessageBox.Show("Проверьте правильность введенных данных!");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PrepareWindow();
        }

        private void edtPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Calculating)
            {
                Calculating = true;

                //CalculateByPrice();
                CalculateSum();
                Calculating = false;
            }
        }

        private void edtDiscount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Calculating)
            {
                Calculating = true;

                //CalculateByDiscount();
                CalculateSum();
                Calculating = false;
            }
        }

        private void edtQuantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Calculating)
            {
                Calculating = true;
                CalculateSum();
                Calculating = false;
            }
        }
    }
}