using System;
using System.Windows;
using System.Windows.Controls;
using bycar;
using bycar3.External_Code;

namespace bycar3.Views.Invoice
{
    /// <summary>
    /// Interaction logic for SpareInInvoiceEditView.xaml
    /// </summary>
    public partial class SpareInInvoiceEditView : Window
    {
        #region MEMBERS

        // V2.0
        public decimal Q = 0;

        public decimal P = 0;
        private decimal S = 0;

        //int VRId = 0;               // Vat Rate ID
        private decimal VRSum = 0;              // Vat Rate Sum

        private int VRate = 0;
        private decimal TS = 0;

        // OLD

        private DataAccess da = new DataAccess();
        public int _OfferingID = -1;
        public int _SpareID = -1;
        public string _SpareName = "";
        public int _InvoiceID = -1;
        public int _SpareInSpareIncomeID = -1;

        //        public int _OfferingNumber = 0;
        //      public string CurrentCurrencyCode = "";
        private bool EditMode = false;

        //double sum = 0;

        #endregion MEMBERS

        #region FUNCTONS

        private void LoadOffering()
        {
            EditMode = true;
            LoadVATs();
            if (_SpareName.Length > 35)
            {
                _SpareName = _SpareName.Remove(35);
                _SpareName += "...";
            }
            edtSpareName.Content = _SpareName;

            S = Q * P;
            VRate = 0;
            VRSum = 0;
            TS = S;

            /*
            if (_OfferingID > 0)
            {
            //    spare_in_invoice i = da.InvoiceOfferingGet(_OfferingID);
            //    i.spareReference.Load();
            //    i.vat_rateReference.Load();
                //double PriceFull = i.total_sum.Value / i.quantity.Value;
                // Q
                //Q = i.quantity.Value;
                // P
                //P = i.price.Value;
                // S
                //S = P * Q;
                // VRId
                //VRId = i.vat_rate.id;
                // VRate
                //VRate = (int)i.vat_rate.rate;
                // VRSum
                //VRSum = S * VRate / 100;
                // TS
                //TS = S + VRSum;
                //edtVAT.SelectedItem = i.vat_rate.name;
            }*/
            BindValues();
            EditMode = false;
        }

        private bool CreateItem()
        {
            spare_in_invoice offering = getItemFromFields();

            //offering.BasicPrice = offering.price_full / CurrencyRate.rate;
            //offering.BasicPrice = CurrencyHelper.GetBasicPrice("BYR", offering.price);
            string vat = edtVAT.SelectedItem.ToString();
            if (_SpareID > 0 && offering != null)
            {
                da.InvoiceOfferingCreate(offering, _SpareID, _InvoiceID, vat, _SpareInSpareIncomeID);
                SpareContainer.Instance.Update(da.GetSpareView(_SpareID));
                return true;
            }
            else
                return false;
        }

        private bool EditItem()
        {
            spare_in_invoice offering = getItemFromFields();

            //offering.BasicPrice = CurrencyHelper.GetBasicPrice(CurrentCurrencyCode, offering.price_full.Value);
            string vat = edtVAT.SelectedItem.ToString();
            if (offering != null)
            {
                _SpareID = da.InOfferingGet(_OfferingID).spare.id;

                //da.InvoiceOfferingEdit(offering, vat);
                SpareContainer.Instance.Update(da.GetSpareView(_SpareID));
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

        private spare_in_invoice getItemFromFields()
        {
            spare_in_invoice item = new spare_in_invoice();
            try
            {
                BindValues();

                if (this._OfferingID > 0)
                    item.id = this._OfferingID;
                item.quantity = (int)Q;
                item.price = P;
                item.total_sum = S;
                item.vat_rate_sum = VRSum;
                item.total_with_vat = TS;
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

        public SpareInInvoiceEditView()
        {
            InitializeComponent();
        }

        private void edtQ_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateSum(0);
        }

        private void edtPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateSum(1);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOffering();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (TS > 0)
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
            decimal res = 0;
            decimal.TryParse(edtVAT.SelectedItem.ToString().Replace('%', ' '), out res);
            VRate = (int)res;
            CalculateSum(3);
        }

        private void CalculateSum(int ind)
        {
            if (!EditMode)
            {
                decimal.TryParse(edtQ.Text, out Q);

                decimal.TryParse(edtPrice.Text, out P);
                S = Q * P;
                VRSum = S * VRate / 100;
                TS = S + VRSum;
                BindValues();
            }
        }

        private void BindValues()
        {
            edtQ.Text = Q.ToString();
            edtPrice.Text = P.ToString();
            edtSum.Text = S.ToString();
            edtVatRateSum.Text = VRSum.ToString();
            edtTotalSum.Text = TS.ToString();
        }
    }
}