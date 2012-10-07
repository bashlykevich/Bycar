using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Xps.Packaging;
using bycar;
using bycar3.Core;
using CodeReason.Reports;

namespace bycar3.Reporting
{
    /// <summary>
    /// Interaction logic for ReportViewRevision.xaml
    /// </summary>
    public partial class ReportViewRevision : Window
    {
        private List<SpareView> items = null;
        private DateTime d;

        public ReportViewRevision(List<SpareView> itms, DateTime dt)
        {
            InitializeComponent();
            DataAccess da = new DataAccess();
            items = itms;
            d = dt;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            try
            {
                ReportDocument reportDocument = new ReportDocument();
                StreamReader reader = new StreamReader(new FileStream(@"Templates\RevisionReport.xaml", FileMode.Open, FileAccess.Read));
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
                dt.Columns.Add("t4", typeof(string));
                dt.Columns.Add("t5", typeof(string));
                dt.Columns.Add("t6", typeof(string));
                dt.Columns.Add("t7", typeof(string));

                // забиваем таблицу данными
                if (items == null)
                {
                    items = da.GetSpares();
                }
                try
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        dt.Rows.Add(new object[] {
                                    (i+1).ToString(),
                                    items[i].name,
                                    items[i].code,
                                    items[i].codeShatem,
                                    items[i].QRest,
                                    items[i].q_rest
                                });
                    }
                }
                catch (Exception exc)
                {
                    Marvin.Instance.Log(exc.Message);
                    throw exc;
                }
                string strDate = d.Day.ToString();
                string mnth = "";
                switch (d.Month)
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
                strDate += " " + mnth + " " + d.Year + " г.";

                data.ReportDocumentValues.Add("p1", strDate); // print date is now

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