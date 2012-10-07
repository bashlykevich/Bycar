using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;
using bycar;
using bycar3.External_Code;
using CodeReason.Reports;

namespace bycar3.Reporting
{
    internal class ReportItem
    {
        public int SpareID;
        public string SpareName;
        public string UnitName;
        public decimal Quantity;
        public decimal Price;
        public decimal Amount;
        public string VAT;
        public decimal VATAmount;
        public decimal Total;
        public decimal total_sum;
        public decimal VatRate;
    }

    /// <summary>
    /// Interaction logic for ReportViewSalesCheck.xaml
    /// </summary>
    public partial class ReportViewSalesCheck : Window
    {
        #region DATAMEMBERS

        private int SpareOutgoId = -1;
        private spare_outgo Outgo = null;
        private Sale sale = null;

        #endregion DATAMEMBERS

        public ReportViewSalesCheck(int outgoId)
        {
            InitializeComponent();
            DataAccess da = new DataAccess();
            SpareOutgoId = outgoId;
            Outgo = da.SpareOutgoGet(outgoId);
        }

        public ReportViewSalesCheck(Sale s)
        {
            InitializeComponent();
            DataAccess da = new DataAccess();
            sale = s;
        }

        private void BuildReportByOutgo()
        {
            if (Outgo == null)
                return;
            try
            {
                ReportDocument reportDocument = new ReportDocument();

                StreamReader reader = new StreamReader(new FileStream(@"Templates\SalesCheckReport.xaml", FileMode.Open, FileAccess.Read));
                reportDocument.XamlData = reader.ReadToEnd();
                reportDocument.XamlImagePath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Templates\");
                reader.Close();

                ReportData data = new ReportData();
                decimal ts = 0;
                decimal vs = 0;
                decimal asum = 0;
                DataAccess da = new DataAccess();
                string BCC = da.getBasicCurrencyCode();

                //Income.currencyReference.Load();
                //string CCC = Income.currency.code;

                // Таблица ТОВАРЫ В НАКЛАДНОЙ
                DataTable dt = new DataTable("mtable");

                // описываем столбцы таблицы
                dt.Columns.Add("SpareName", typeof(string));
                dt.Columns.Add("UnitName", typeof(string));
                dt.Columns.Add("Quantity", typeof(string));
                dt.Columns.Add("Price", typeof(string));
                dt.Columns.Add("Amount", typeof(string));
                dt.Columns.Add("VAT", typeof(string));
                dt.Columns.Add("VATAmount", typeof(string));
                dt.Columns.Add("Total", typeof(string));
                decimal TotalAmount = 0;

                // забиваем таблицу данными
                List<ReportOutgo> list = new List<ReportOutgo>();
                list = da.GetReportOutgoes(SpareOutgoId);

                List<SpareInSpareOutgoView> LIST2 = da.SpareInSpareOutgoViewGet(SpareOutgoId);
                try
                {
                    string CompanyName = da.getProfileCurrent().CompanyName;
                    for (int i = 0; i < LIST2.Count; i++)
                    {
                        //decimal Total = list[i].total_sum;
                        //decimal VatSum = Total * list[i].VatRate.Value / 100;
                        //decimal Sum = Total - VatSum;
                        //decimal Price = Sum / list[i].quantity;
                        TotalAmount += list[i].quantity * list[i].PriceOut.Value;

                        //ts += Total;
                        //vs += VatSum;
                        //asum += Sum;

                        dt.Rows.Add(new object[] {
                            list[i].SpareName,
                            list[i].UnitName,
                            list[i].quantity.ToString("0.##"),
                            list[i].PriceOut.Value.ToString("0.##"),
                            (list[i].PriceOut.Value*list[i].quantity).ToString("0.##"),
                            list[i].VatName,
                            0,
                            0
                        });
                    }
                }
                catch (Exception exc)
                {
                    throw exc;
                }
                string str_ts = RSDN.RusCurrency.Str(ts, "BYR");
                string str_vs = RSDN.RusCurrency.Str(vs, "BYR");

                // set constant document values
                //string strDate = Outgo.created_on.Value.GetDateTimeFormats('d')[3];
                string strDate = Outgo.created_on.Value.Day.ToString();
                string mnth = "";
                switch (Outgo.created_on.Value.Month)
                {
                    case 1:
                        mnth = "января";
                        break;

                    case 2:
                        mnth = "февраля";
                        break;

                    case 3:
                        mnth = "марта";
                        break;

                    case 4:
                        mnth = "апреля";
                        break;

                    case 5:
                        mnth = "мая";
                        break;

                    case 6:
                        mnth = "июня";
                        break;

                    case 7:
                        mnth = "июля";
                        break;

                    case 8:
                        mnth = "августа";
                        break;

                    case 9:
                        mnth = "сентября";
                        break;

                    case 10:
                        mnth = "октября";
                        break;

                    case 11:
                        mnth = "ноября";
                        break;

                    case 12:
                        mnth = "декабря";
                        break;
                }
                strDate += " " + mnth + " " + Outgo.created_on.Value.Year + " г.";
                data.ReportDocumentValues.Add("TotalAmount", TotalAmount.ToString("0.##"));
                data.ReportDocumentValues.Add("ReportDate", strDate); // print date is now

                //data.ReportDocumentValues.Add("IncomeNumber", Income.id);
                data.ReportDocumentValues.Add("VATAmountSumStr", str_vs);
                data.ReportDocumentValues.Add("TotalSumStr", str_ts);
                data.ReportDocumentValues.Add("AmountSum", asum.ToString("0.##"));
                data.ReportDocumentValues.Add("VATAmountSum", vs.ToString("0.##"));
                data.ReportDocumentValues.Add("TotalSum", ts.ToString("0.##"));
                data.ReportDocumentValues.Add("accepter", Outgo.accepter);
                data.ReportDocumentValues.Add("address", Outgo.address);
                SpareOutgoView sov = da.GetSpareOutgoView(Outgo.id);
                data.ReportDocumentValues.Add("customer", sov.AccountName);
                data.ReportDocumentValues.Add("deliverer", Outgo.deliverer);
                data.ReportDocumentValues.Add("driver", Outgo.driver);
                data.ReportDocumentValues.Add("trailer", Outgo.trailer);
                data.ReportDocumentValues.Add("tripsheet", Outgo.tripsheet);
                data.ReportDocumentValues.Add("truck", Outgo.truck);
                data.ReportDocumentValues.Add("truckowner", Outgo.truckowner);
                Outgo.currencyReference.Load();
                data.ReportDocumentValues.Add("CurrencyName", Outgo.currency.name);
                settings_profile profile = da.getProfileCurrent();
                string p1 = profile.CompanyName;// +", " + profile.AddressJur;
                p1 = Regex.Replace(p1, " +", " ");
                data.ReportDocumentValues.Add("param1", p1);
                string p2 = sov.AccountName + ", " + Outgo.address;
                data.ReportDocumentValues.Add("param2", p2);
                data.ReportDocumentValues.Add("param3", Outgo.basement);
                data.ReportDocumentValues.Add("param4", Outgo.description);
                data.ReportDocumentValues.Add("param5", profile.CompanyHead);
                data.ReportDocumentValues.Add("param6", p1 = Regex.Replace(profile.UNN, " +", " "));
                data.ReportDocumentValues.Add("param7", Outgo.unn);
                data.ReportDocumentValues.Add("warrant", Outgo.warrant);

                data.DataTables.Add(dt);

                DateTime dateTimeStart = DateTime.Now; // start time measure here

                XpsDocument xps = reportDocument.CreateXpsDocument(data);
                documentViewer.Document = xps.GetFixedDocumentSequence();
            }
            catch (Exception ex)
            {
                // show exception
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.GetType() + "\r\n" + ex.StackTrace, ex.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }

        private void BuildReportBySale()
        {
            ReportDocument reportDocument = new ReportDocument();

            StreamReader reader = new StreamReader(new FileStream(@"Templates\SalesCheckReport.xaml", FileMode.Open, FileAccess.Read));
            reportDocument.XamlData = reader.ReadToEnd();
            reportDocument.XamlImagePath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Templates\");
            reader.Close();

            ReportData data = new ReportData();
            decimal ts = 0;
            decimal vs = 0;
            decimal asum = 0;
            DataAccess da = new DataAccess();
            string BCC = da.getBasicCurrencyCode();

            // Таблица ТОВАРЫ В НАКЛАДНОЙ
            DataTable dt = new DataTable("mtable");

            // описываем столбцы таблицы
            dt.Columns.Add("SpareName", typeof(string));
            dt.Columns.Add("UnitName", typeof(string));
            dt.Columns.Add("Quantity", typeof(string));
            dt.Columns.Add("Price", typeof(string));
            dt.Columns.Add("Amount", typeof(string));
            dt.Columns.Add("VAT", typeof(string));
            dt.Columns.Add("VATAmount", typeof(string));
            dt.Columns.Add("Total", typeof(string));

            // забиваем таблицу данными

            List<ReportOutgo> listx = new List<ReportOutgo>();
            listx = da.GetReportOutgoes(sale);
            List<ReportItem> list = new List<ReportItem>();
            foreach (ReportOutgo s in listx)
            {
                if (list.FirstOrDefault(x => x.SpareID == s.SpareID) == null)
                {
                    ReportItem ri = new ReportItem();
                    ri.SpareName = s.SpareName;
                    ri.UnitName = s.UnitName;
                    ri.Quantity = s.quantity;
                    ri.total_sum = s.total_sum;
                    ri.VatRate = s.VatRate.Value;
                    ri.VAT = s.VatName;
                    ri.SpareID = s.SpareID;
                    ri.Price = CurrencyHelper.GetPrice("BYR", s.PriceOutBasic.Value);

                    list.Add(ri);
                }
                else
                {
                    list.FirstOrDefault(x => x.SpareID == s.SpareID).Quantity += s.quantity;
                }
            }
            if (list.Count == 0)
                return;

            //SpareOutgoId = list[0].OutgoID;
            //Outgo = da.SpareOutgoGet(SpareOutgoId);
            decimal TotalAmount = 0;
            string CompanyName = da.getProfileCurrent().CompanyName;
            for (int i = 0; i < list.Count; i++)
            {
                //decimal Total = list[i].total_sum;
                //decimal VatSum = Total * list[i].VatRate / 100;
                //decimal Sum = Total - VatSum;
                //decimal Price = Sum / list[i].Quantity;
                TotalAmount += list[i].Quantity * list[i].Price;

                //ts += Total;
                //vs += VatSum;
                //asum += Sum;

                dt.Rows.Add(new object[] {
                            list[i].SpareName,
                            list[i].UnitName,
                            list[i].Quantity.ToString("0.##"),
                            list[i].Price.ToString("0.##"),
                            (list[i].Quantity*list[i].Price).ToString("0.##"),
                            list[i].VAT,
                            0,
                            0
                        });
            }
            string str_ts = RSDN.RusCurrency.Str(ts, "BYR");
            string str_vs = RSDN.RusCurrency.Str(vs, "BYR");

            // set constant document values
            //string strDate = Outgo.created_on.Value.GetDateTimeFormats('d')[3];
            DateTime date = sale.SaleDate;
            string strDate = date.Day.ToString();
            string mnth = "";
            switch (date.Month)
            {
                case 1:
                    mnth = "января";
                    break;

                case 2:
                    mnth = "февраля";
                    break;

                case 3:
                    mnth = "марта";
                    break;

                case 4:
                    mnth = "апреля";
                    break;

                case 5:
                    mnth = "мая";
                    break;

                case 6:
                    mnth = "июня";
                    break;

                case 7:
                    mnth = "июля";
                    break;

                case 8:
                    mnth = "августа";
                    break;

                case 9:
                    mnth = "сентября";
                    break;

                case 10:
                    mnth = "октября";
                    break;

                case 11:
                    mnth = "ноября";
                    break;

                case 12:
                    mnth = "декабря";
                    break;
            }
            strDate += " " + mnth + " " + date.Year + " г.";

            data.ReportDocumentValues.Add("ReportDate", strDate); // print date is now

            //data.ReportDocumentValues.Add("IncomeNumber", Income.id);
            data.ReportDocumentValues.Add("VATAmountSumStr", str_vs);
            data.ReportDocumentValues.Add("TotalSumStr", str_ts);
            data.ReportDocumentValues.Add("AmountSum", asum.ToString("0.##"));
            data.ReportDocumentValues.Add("TotalAmount", TotalAmount.ToString("0.##"));
            data.ReportDocumentValues.Add("VATAmountSum", vs.ToString("0.##"));
            data.ReportDocumentValues.Add("TotalSum", ts.ToString("0.##"));
            settings_profile profile = da.getProfileCurrent();
            string p1 = profile.CompanyName;// +", " + profile.AddressJur;
            p1 = Regex.Replace(p1, " +", " ");
            data.ReportDocumentValues.Add("param1", p1);
            data.ReportDocumentValues.Add("param5", profile.CompanyHead);
            data.ReportDocumentValues.Add("param6", p1 = Regex.Replace(profile.UNN, " +", " "));

            data.DataTables.Add(dt);

            DateTime dateTimeStart = DateTime.Now; // start time measure here

            XpsDocument xps = reportDocument.CreateXpsDocument(data);
            documentViewer.Document = xps.GetFixedDocumentSequence();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (sale == null)
                BuildReportByOutgo();
            else BuildReportBySale();
        }
    }
}