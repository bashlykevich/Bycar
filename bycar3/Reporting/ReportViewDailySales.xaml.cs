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
using CodeReason.Reports;
using System.IO;
using bycar;
using System.Data;
using System.Windows.Xps.Packaging;
using bycar3.Core;

namespace bycar3.Reporting
{
    /// <summary>
    /// Interaction logic for ReportViewDailySales.xaml
    /// </summary>
    public partial class ReportViewDailySales : Window
    {
        #region DATAMEMBERS
        DateTime date = DateTime.Now;
        DateTime dateTo = DateTime.Now;
        #endregion

        public ReportViewDailySales(DateTime dt, DateTime dtTo)
        {
            InitializeComponent();
            date = dt;
            dateTo = dtTo;
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
                dt.Columns.Add("SpareCodeShatem", typeof(string));
                dt.Columns.Add("SpareCode", typeof(string));                
                dt.Columns.Add("Q", typeof(int));
                dt.Columns.Add("P", typeof(double));                
                dt.Columns.Add("VAT", typeof(string));
                dt.Columns.Add("T", typeof(double));
                dt.Columns.Add("SaleDate", typeof(string));
                
                // забиваем таблицу данными                
                List<SpareInSpareOutgoView> LIST2 = da.GetSpareInSpareOutgoByPeriod(date, dateTo);
                decimal asum = 0;
                try
                {                    
                    for (int i = 0; i < LIST2.Count; i++)
                    {
                        asum += LIST2[i].total_sum;
                        dt.Rows.Add(new object[] { 
                            i+1,
                            LIST2[i].SpareName,
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
