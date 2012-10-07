using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;
using bycar;
using bycar3.Core;
using CodeReason.Reports;

namespace bycar3.Reporting
{
    /// <summary>
    /// Interaction logic for ReportViewDailySales.xaml
    /// </summary>
    public partial class ReportViewDailySales : Window
    {
        #region DATAMEMBERS

        private DateTime date = DateTime.Now;
        private DateTime dateTo = DateTime.Now;
        private int warehouseID;

        #endregion DATAMEMBERS

        public ReportViewDailySales(DateTime dt, DateTime dtTo, int wid)
        {
            InitializeComponent();
            date = dt;
            dateTo = dtTo;
            warehouseID = wid;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            try
            {
                ReportDocument reportDocument = new ReportDocument();

                StreamReader reader = new StreamReader(new FileStream(@"Templates\DailySalesReport.xaml", FileMode.Open, FileAccess.Read));
                reportDocument.XamlData = reader.ReadToEnd();
                reportDocument.XamlImagePath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Templates\");
                reader.Close();

                ReportData data = new ReportData();
                DataAccess da = new DataAccess();
                string BCC = da.getBasicCurrencyCode();

                // Таблица ТОВАРЫ В НАКЛАДНОЙ
                DataTable dt = new DataTable("mtable");

                // описываем столбцы таблицы
                dt.Columns.Add("Num", typeof(int));
                dt.Columns.Add("SpareName", typeof(string));
                dt.Columns.Add("WarehouseName", typeof(string));
                dt.Columns.Add("SpareCodeShatem", typeof(string));
                dt.Columns.Add("SpareCode", typeof(string));
                dt.Columns.Add("Q", typeof(int));
                dt.Columns.Add("P", typeof(double));
                dt.Columns.Add("VAT", typeof(string));
                dt.Columns.Add("T", typeof(double));
                dt.Columns.Add("SaleDate", typeof(string));

                // забиваем таблицу данными
                List<SpareInSpareOutgoView> LIST2 = warehouseID > 0 ? da.GetSpareInSpareOutgoByPeriod(date, dateTo, warehouseID) : da.GetSpareInSpareOutgoByPeriod(date, dateTo);
                decimal asum = 0;
                try
                {
                    for (int i = 0; i < LIST2.Count; i++)
                    {
                        asum += LIST2[i].total_sum;
                        dt.Rows.Add(new object[] {
                            i+1,
                            LIST2[i].SpareName,
                            LIST2[i].WarehouseName,
                            LIST2[i].codeShatem,
                            LIST2[i].code,
                            LIST2[i].quantity,
                            LIST2[i].purchase_price,
                            LIST2[i].VatRateName,
                            LIST2[i].total_sum,
                            LIST2[i].OutgoDate.Value.ToString("dd.MM.yyyy")
                        });
                    }
                }
                catch (Exception exc)
                {
                    Marvin.Instance.Log(exc.Message);
                    throw exc;
                }
                string str_ts = RSDN.RusCurrency.Str(asum, "BYR");

                string strDate = "";
                if (date == dateTo)
                {
                    strDate = date.ToString("dd.MM.yyyy");
                }
                else
                {
                    string strDateFrom = date.ToString("dd.MM.yyyy");
                    string strDateTo = dateTo.ToString("dd.MM.yyyy");
                    strDate = strDateFrom + " - " + strDateTo;
                }
                data.ReportDocumentValues.Add("ReportDate", strDate); // print date is now
                data.ReportDocumentValues.Add("asum", asum);
                data.DataTables.Add(dt);

                DateTime dateTimeStart = DateTime.Now; // start time measure here
                XpsDocument xps = reportDocument.CreateXpsDocument(data);
                documentViewer.Document = xps.GetFixedDocumentSequence();
            }
            catch (Exception ex)
            {
                // show exception
                Marvin.Instance.Log(ex.Message);
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.GetType() + "\r\n" + ex.StackTrace, ex.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }
    }
}