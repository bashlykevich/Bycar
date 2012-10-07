using System;
using System.Windows;
using bycar;

namespace bycar3.Reporting
{
    /// <summary>
    /// Логика взаимодействия для ReportViewSpareOutgoTTNAppendix.xaml
    /// </summary>
    public partial class ReportViewSpareOutgoTTNAppendix : Window
    {
        #region DATAMEMBERS

        private int SpareOutgoId = -1;
        private spare_outgo Outgo = null;

        #endregion DATAMEMBERS

        public ReportViewSpareOutgoTTNAppendix(int outgoId)
        {
            InitializeComponent();
            DataAccess da = new DataAccess();
            SpareOutgoId = outgoId;
            Outgo = da.SpareOutgoGet(outgoId);
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            /*
            if (Outgo == null)
                return;
            try
            {
                ReportDocument reportDocument = new ReportDocument();

                StreamReader reader = new StreamReader(new FileStream(@"Templates\SpareOutgoTTNAppendixReport.xaml", FileMode.Open, FileAccess.Read));
                reportDocument.XamlData = reader.ReadToEnd();
                reportDocument.XamlImagePath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Templates\");
                reader.Close();

                ReportData data = new ReportData();
                double ts = 0;
                double vs = 0;
                double asum = 0;
                DataAccess da = new DataAccess();
                string BCC = da.getBasicCurrencyCode();

                //Income.currencyReference.Load();
                //string CCC = Income.currency.code;

                // Таблица ТОВАРЫ В НАКЛАДНОЙ
                DataTable dt = new DataTable("mtable");

                // описываем столбцы таблицы
                dt.Columns.Add("SpareName", typeof(string));
                dt.Columns.Add("UnitName", typeof(string));
                dt.Columns.Add("Quantity", typeof(int));
                dt.Columns.Add("Price", typeof(double));
                dt.Columns.Add("Amount", typeof(double));
                dt.Columns.Add("VAT", typeof(string));
                dt.Columns.Add("VATAmount", typeof(double));
                dt.Columns.Add("Total", typeof(double));

                // забиваем таблицу данными
                List<ReportOutgo> list = new List<ReportOutgo>();
                list = da.GetReportOutgoes(SpareOutgoId);
                try
                {
                    string CompanyName = da.getProfileCurrent().CompanyName;
                    for (int i = 0; i < list.Count; i++)
                    {
                        ts += list[i].PriceOut.Value;
                        vs += list[i].VatRate;
                        asum += list[i].IncomePrice.Value;
                        dt.Rows.Add(new object[] {
                            list[i].SpareName,
                            list[i].UnitName,
                            list[i].quantity,
                            list[i].IncomePrice,
                            list[i].quantity*list[i].IncomePrice,
                            list[i].VatName,
                            list[i].VatRate, // поправить!!!!!
                            list[i].PriceOut
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

                data.ReportDocumentValues.Add("ReportDate", strDate); // print date is now

                //data.ReportDocumentValues.Add("IncomeNumber", Income.id);
                data.ReportDocumentValues.Add("VATAmountSumStr", str_vs);
                data.ReportDocumentValues.Add("TotalSumStr", str_ts);
                data.ReportDocumentValues.Add("AmountSum", asum);
                data.ReportDocumentValues.Add("VATAmountSum", vs);
                data.ReportDocumentValues.Add("TotalSum", ts);
                data.ReportDocumentValues.Add("accepter", Outgo.accepter);
                data.ReportDocumentValues.Add("address", Outgo.address);
                data.ReportDocumentValues.Add("customer", Outgo.customer);
                data.ReportDocumentValues.Add("deliverer", Outgo.deliverer);
                data.ReportDocumentValues.Add("driver", Outgo.driver);
                data.ReportDocumentValues.Add("trailer", Outgo.trailer);
                data.ReportDocumentValues.Add("tripsheet", Outgo.tripsheet);
                data.ReportDocumentValues.Add("truck", Outgo.truck);
                data.ReportDocumentValues.Add("truckowner", Outgo.truckowner);
                Outgo.currencyReference.Load();
                data.ReportDocumentValues.Add("CurrencyName", Outgo.currency.name);
                settings_profile profile = da.getProfileCurrent();
                string p1 = profile.CompanyName + ", " + profile.AddressJur;
                p1 = Regex.Replace(p1, " +", " ");
                data.ReportDocumentValues.Add("param1", p1);
                data.ReportDocumentValues.Add("param2", Outgo.unloading);
                data.ReportDocumentValues.Add("param3", Outgo.basement);
                data.ReportDocumentValues.Add("param4", Outgo.description);
                data.ReportDocumentValues.Add("param5", profile.CompanyHead);
                data.ReportDocumentValues.Add("param6", p1 = Regex.Replace(profile.UNN, " +", " "));
                data.ReportDocumentValues.Add("param7", Outgo.unn);

                data.DataTables.Add(dt);

                DateTime dateTimeStart = DateTime.Now; // start time measure here

                XpsDocument xps = reportDocument.CreateXpsDocument(data);
                documentViewer.Document = xps.GetFixedDocumentSequence();
            }
            catch (Exception ex)
            {
                // show exception
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.GetType() + "\r\n" + ex.StackTrace, ex.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Stop);
            }*/
        }
    }
}