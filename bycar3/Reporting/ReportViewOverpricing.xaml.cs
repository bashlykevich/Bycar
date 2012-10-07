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
    /// Interaction logic for ReportViewOverpricing.xaml
    /// </summary>
    public partial class ReportViewOverpricing : Window
    {
        #region DATAMEMBERS

        private int ItemID = -1;
        private overpricing Outgo = null;

        #endregion DATAMEMBERS

        public ReportViewOverpricing(int id)
        {
            InitializeComponent();
            DataAccess da = new DataAccess();
            ItemID = id;
            Outgo = da.OverpricingGet(id);
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (Outgo == null)
                return;
            try
            {
                ReportDocument reportDocument = new ReportDocument();

                StreamReader reader = new StreamReader(new FileStream(@"Templates\OverpricingReport.xaml", FileMode.Open, FileAccess.Read));
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
                dt.Columns.Add("t9", typeof(string));
                dt.Columns.Add("t10", typeof(string));

                decimal sum1 = 0;
                decimal sum2 = 0;
                decimal sum3 = 0;

                // забиваем таблицу данными
                List<SpareInOverpricingView> LIST2 = da.OverpricingOfferingGet(ItemID);
                try
                {
                    string CompanyName = da.getProfileCurrent().CompanyName;
                    for (int i = 0; i < LIST2.Count; i++)
                    {
                        sum1 += LIST2[i].quantity.Value;
                        sum2 += LIST2[i].sumOld.Value;
                        sum3 += LIST2[i].sumNew.Value;

                        dt.Rows.Add(new object[] {
                            LIST2[i].SpareName,
                            LIST2[i].UnitName,
                            LIST2[i].quantity,
                            LIST2[i].purchasePrice,
                            LIST2[i].percentOld,
                            LIST2[i].priceOld,
                            LIST2[i].sumOld,
                            LIST2[i].percentNew,
                            LIST2[i].priceNew,
                            LIST2[i].sumNew
                        });
                    }
                }
                catch (Exception exc)
                {
                    Marvin.Instance.Log(exc.Message);
                    throw exc;
                }
                string strDate = Outgo.createdOn.Value.Day.ToString();
                string mnth = "";
                switch (Outgo.createdOn.Value.Month)
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
                strDate += " " + mnth + " " + Outgo.createdOn.Value.Year + " г.";
                data.ReportDocumentValues.Add("p1", Outgo.id);
                data.ReportDocumentValues.Add("p2", strDate); // print date is now
                data.ReportDocumentValues.Add("sum1", sum1);
                data.ReportDocumentValues.Add("sum2", sum2);
                data.ReportDocumentValues.Add("sum3", sum3);

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