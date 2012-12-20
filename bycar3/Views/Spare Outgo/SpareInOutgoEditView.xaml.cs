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
            Price = System.Xml.XmlConvert.ToDecimal(edtPrice.Text);
            dDicscount = _Price - Price;
            edtDiscount.Text = dDicscount.ToString();
        }

        private void CalculateByDiscount()
        {
            decimal dDicscount = 0;
            decimal Price = 0;
            dDicscount = System.Xml.XmlConvert.ToDecimal(edtDiscount.Text);  
            Price = _Price - dDicscount;
            edtPrice.Text = Price.ToString();
        }

        void CheckField(TextBox edt)
        {
            if (System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator == ".")
                if (edt.Text.Contains(","))
                    edt.Text = edt.Text.Replace(",", ".");
                else
                    if (System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator == ",")
                        if (edt.Text.Contains("."))
                            edt.Text = edt.Text.Replace(".", ",");
        }
        private void CalculateSum()
        {
            decimal Q = 0;
            decimal P = 0;
            decimal S = 0;
            decimal D = 0;
            decimal T = 0;

            CheckField(edtQuantity);
            CheckField(edtDiscount);
            CheckField(edtPrice);
            
            if(decimal.TryParse(edtQuantity.Text, out Q))
                Q = System.Xml.XmlConvert.ToDecimal(edtQuantity.Text);
            
            if (decimal.TryParse(edtPrice.Text, out P))
                P = System.Xml.XmlConvert.ToDecimal(edtPrice.Text);
            if (decimal.TryParse(edtDiscount.Text, out D))
                D = System.Xml.XmlConvert.ToDecimal(edtDiscount.Text);  
            if (Q > _AvailableQuantity)
            {
                Q = _AvailableQuantity;
                edtQuantity.Text = Q.ToString();
            }
                     
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
            
            Quantity = System.Xml.XmlConvert.ToDecimal(edtQuantity.Text);
            
            Price = System.Xml.XmlConvert.ToDecimal(edtPrice.Text);
            Discount = System.Xml.XmlConvert.ToDecimal(edtDiscount.Text); 
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
                quantity = System.Xml.XmlConvert.ToDecimal(edtQuantity.Text);                
                if (quantity > _AvailableQuantity || quantity <= 0)
                    return false;
                DataAccess da = new DataAccess();
                decimal Price = decimal.Parse(edtPrice.Text);
                decimal Discount = 0;
                Discount = System.Xml.XmlConvert.ToDecimal(edtDiscount.Text);
                decimal BasicPrice = CurrencyHelper.GetBasicPrice(CurrentCurrencyCode, Price);
                _SpareID = da.SpareInSpareOutgoCreate(_SpareInSpareIncomeID, quantity, _SpareOutgoID, Price, BasicPrice, Discount, 0);
                SpareContainer.Instance.Update(_SpareID);
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