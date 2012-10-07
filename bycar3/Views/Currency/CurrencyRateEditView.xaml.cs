using System;
using System.Windows;
using bycar;

namespace bycar3.Views.Currency
{
    /// <summary>
    /// Interaction logic for CurrencyRateEditView.xaml
    /// </summary>
    public partial class CurrencyRateEditView : Window
    {
        // DATA MEMBERS
        public float Rate = 1;

        public DateTime RateDate = DateTime.Now;
        public int RateID = 0;
        public int CurrencyID = 0;
        public string CurrencyName = "";
        public string BasicCurrencyName = "";

        // CUSTOM FUNCTIONS
        private void SaveItem()
        {
            if (RateID < 1)
            {
                CreateRate();
            }
            else
                EditRate();
            Close();
        }

        private void EditRate()
        {
            DataAccess da = new DataAccess();
            currency_rate r = new currency_rate();
            r.id = this.RateID;
            r.currency = da.GetCurrency(CurrencyID);
            r.rate_date = edtDate.SelectedDate.Value;
            decimal rt = 1;
            decimal.TryParse(edtRate.Text, out rt);
            r.rate = rt;
            da.CurrencyRateEdit(r);
        }

        private void CreateRate()
        {
            DataAccess da = new DataAccess();
            currency_rate r = new currency_rate();
            r.currency = da.GetCurrency(CurrencyID);
            r.rate_date = edtDate.SelectedDate.Value;
            decimal rt = 1;
            decimal.TryParse(edtRate.Text, out rt);
            r.rate = rt;
            da.CurrencyRateCreate(r);
        }

        private void Initialize()
        {
            //            if (RateID > 0)
            //{
            //            }
            //            else
            //            {
            edtDate.SelectedDate = RateDate;
            edtRate.Text = Rate.ToString();
            edtComment.Content = CurrencyName + " = 1 " + BasicCurrencyName;

            //            }
        }

        // HANDLERS
        public CurrencyRateEditView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            SaveItem();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}