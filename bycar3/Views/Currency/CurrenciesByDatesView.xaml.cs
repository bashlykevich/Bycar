using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using bycar;
using bycar3.NbrbServiceReference;

namespace bycar3.Views.Currency
{
    /// <summary>
    /// Interaction logic for CurrenciesByDatesView.xaml
    /// </summary>
    public partial class CurrenciesByDatesView : Window
    {
        // CUSTOM FUNCTIONS
        private void LoadRates()
        {
            LoadRates(DateTime.Now);
        }

        private void LoadRates(DateTime dt)
        {
            edtDate.SelectedDate = dt;

            // искать курсы на заданную даты в базе
            // если не найдено, то предложить загрузить их из интернета
            if (!LoadRatesFromDB(dt))
                LoadRatesFromWeb(dt);
        }

        private bool LoadRatesFromDB(DateTime dt)
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
                edtRUR.Text = (r3.rate*100).ToString();
            else
                res = false;
            return res;
        }

        private void LoadRatesFromWeb(DateTime date)
        {
            try
            {
                ExRatesSoapClient ws = new ExRatesSoapClient();
                DataSet ds = ws.ExRatesDaily(date);
                DataTable dt = ds.Tables["DailyExRatesOnDate"];
                DataRowCollection rows = dt.Rows;
                int rowIndexUsd = 2;
                int rowIndexEuro = 3;
                int rowIndexRur = 21;

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

        private void AddRates()
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
            r3.rate = decimal.Parse(edtRUR.Text)/100;
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