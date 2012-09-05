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
using bycar3.NbrbServiceReference;
using System.Data;

namespace bycar3.Views.Currency
{
    /// <summary>
    /// Interaction logic for CurrenciesByDatesView.xaml
    /// </summary>
    public partial class CurrenciesByDatesView : Window
    {
        // CUSTOM FUNCTIONS
        void LoadRates()
        {
            LoadRates(DateTime.Now);
        }
        void LoadRates(DateTime dt)
        {
            edtDate.SelectedDate = dt;
            // искать курсы на заданную даты в базе            
            // если не найдено, то предложить загрузить их из интернета
            if (!LoadRatesFromDB(dt))
                LoadRatesFromWeb(dt);
        }

        bool LoadRatesFromDB(DateTime dt)
        {
            bool res = true;
            DataAccess da = new DataAccess();
            // USD
            currency_rate r1 = da.getCurrencyRate("USD", dt);
            if (r1 != null)
                edtUSD.Text = r1.rate.ToString();
            else
                res = false;
            // EURO
            currency_rate r2 = da.getCurrencyRate("EUR", dt);
            if (r2 != null)
                edtEURO.Text = r2.rate.ToString();
            else
                res = false;
            // RUR
            currency_rate r3 = da.getCurrencyRate("RUR", dt);
            if (r3 != null)
                edtRUR.Text = r3.rate.ToString();
            else
                res = false;
            return res;
        }

        void LoadRatesFromWeb(DateTime date)
        {
            try
            {
                ExRatesSoapClient ws = new ExRatesSoapClient();
                DataSet ds = ws.ExRatesDaily(date);
                DataTable dt = ds.Tables["DailyExRatesOnDate"];
                DataRowCollection rows = dt.Rows;
                int rowIndexUsd = 4;
                int rowIndexEuro = 5;
                int rowIndexRur = 15;

                edtEURO.Text = rows[rowIndexEuro]["Cur_OfficialRate"].ToString();
                edtUSD.Text = rows[rowIndexUsd]["Cur_OfficialRate"].ToString();
                edtRUR.Text = rows[rowIndexRur]["Cur_OfficialRate"].ToString();
                AddRates();
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось загрузить курсы из интернета! Проверьте подключение.");
            }
        }

        void AddRates()
        {
            DataAccess da = new DataAccess();
            List<currency_rate> rates = new List<currency_rate>();
            // USD
            currency c1 = da.GetCurrency("USD");
            currency_rate r1 = new currency_rate();
            r1.currency = c1;
            r1.rate = decimal.Parse(edtUSD.Text);
            r1.rate_date = edtDate.SelectedDate.Value;

            // EURO
            currency c2 = da.GetCurrency("EUR");
            currency_rate r2 = new currency_rate();
            r2.currency = c2;
            r2.rate = decimal.Parse(edtEURO.Text);
            r2.rate_date = edtDate.SelectedDate.Value;

            // RUR
            currency c3 = da.GetCurrency("RUR");
            currency_rate r3 = new currency_rate();
            r3.currency = c3;
            r3.rate = decimal.Parse(edtRUR.Text);
            r3.rate_date = edtDate.SelectedDate.Value;

            rates.Add(r1);
            rates.Add(r2);
            rates.Add(r3);
            da.CurrencyRatesCreate(rates);
        }

        // HANDLERS
        public CurrenciesByDatesView()
        {
            InitializeComponent();
        }
        
        private void btnRatesInput_Click(object sender, RoutedEventArgs e)
        {
            CurrenciesInput v = new CurrenciesInput(edtDate.SelectedDate.Value);
            v.ShowDialog();
        }

        private void edtDate_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (edtDate.SelectedDate.Value.Date > DateTime.Now.Date)            
                MessageBox.Show("Курсы будущих периодов неизвестны!");
             else
                LoadRates(edtDate.SelectedDate.Value);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRates();
        }        
    }
}
