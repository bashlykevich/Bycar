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
using CodeReason.Reports;
using System.IO;
using System.Data;
using System.Windows.Xps.Packaging;
using bycar3.Core;

namespace bycar3.Reporting
{
    /// <summary>
    /// Interaction logic for ReportViewRequest.xaml
    /// </summary>
    public partial class ReportViewRequest : Window
    {
        List<SpareView> items = null;
        public ReportViewRequest()
        {
            InitializeComponent();
            DataAccess da = new DataAccess();
        }
        public ReportViewRequest(List<SpareView> itms)
        {
            InitializeComponent();
            DataAccess da = new DataAccess();
            items = itms;
        }

        private void Window_Activated(object sender, EventArgs e)
        {

            try
            {
                ReportDocument reportDocument = new ReportDocument();
                StreamReader reader = new StreamReader(new FileStream(@"Templates\RequestReport.xaml", FileMode.Open, FileAccess.Read));
                reportDocument.XamlData = reader.ReadToEnd();
                reportDocument.XamlImagePath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Templates\");
                reader.Close();

                ReportData data = new ReportData();
                DataAccess da = new DataAccess();

                // Таблица ТОВАРЫ В НАКЛАДНОЙ
                DataTable dt = new DataTable("mtable");
                // описываем столбцы таблицы                            
                dt.Columns.Add("t1", typeof(string));
                dt.Columns.Add("t2", typeof(string));
                dt.Columns.Add("t3", typeof(string));
                dt.Columns.Add("t4", typeof(string));
                dt.Columns.Add("t5", typeof(string));
                dt.Columns.Add("t6", typeof(string));
                dt.Columns.Add("t7", typeof(string));
                dt.Columns.Add("t8", typeof(string));

                decimal sum1 = 0;
                // забиваем таблицу данными   
                if (items == null)
                {
                    items = da.GetSparesDemand();
                }
                int maxLength = 40;
                try
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        string nm = items[i].name;
                        nm = nm.Substring(0, Math.Min(nm.Length, maxLength));
                        if (items[i].name.Length > maxLength)
                            nm += "...";
                        decimal r1 = items[i].q_demand.Value;
                        decimal r2 = (decimal)items[i].QRest.Value;
                        decimal r = (r1 - r2);
                        sum1 += r;
                        dt.Rows.Add(new object[] { 
                                    (i+1).ToString(),
                                    nm,
                                    items[i].BrandName,
                                    items[i].codeShatem,
                                    items[i].code,                                    
                                    items[i].q_demand,
                                    items[i].QRest,                                    
                                    r
                                });
                    }
                }
                catch (Exception exc)
                {
                    Marvin.Instance.Log(exc.Message);
                    throw exc;
                }
                string strDate = DateTime.Now.Day.ToString();
                string mnth = "";
                switch (DateTime.Now.Month)
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
                strDate += " " + mnth + " " + DateTime.Now.Year + " г.";

                data.ReportDocumentValues.Add("p1", strDate); // print date is now
                data.ReportDocumentValues.Add("sum1", sum1);

                data.DataTables.Add(dt);

                DateTime dateTimeStart = DateTime.Now; // start time measure here

                XpsDocument xps = reportDocument.CreateXpsDocument(data);
                documentViewer.Document = xps.GetFixedDocumentSequence();
            }
            catch (Exception ex)
            {
                Marvin.Instance.Log(ex.Message);
                // show exception
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.GetType() + "\r\n" + ex.StackTrace, ex.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }
    }
}
