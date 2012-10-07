﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;
using bycar;
using CodeReason.Reports;

namespace bycar3.Reporting
{
    /// <summary>
    /// Interaction logic for ReportViewSpareSalesByPeriod.xaml
    /// </summary>
    public partial class ReportViewSpareSalesByPeriod : Window
    {
        #region DATAMEMBERS

        private DateTime dateFrom = DateTime.Now;
        private DateTime dateTo = DateTime.Now;
        private SpareView Spare = null;
        private int WarehouseID = 0;

        #endregion DATAMEMBERS

        public ReportViewSpareSalesByPeriod(DateTime df, DateTime dt, int SpareID, int WID)
        {
            InitializeComponent();
            dateTo = dt;
            dateFrom = df;
            WarehouseID = WID;
            DataAccess db = new DataAccess();
            Spare = db.GetSpareView(SpareID);
            if (Spare == null)
                return;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            try
            {
                ReportDocument reportDocument = new ReportDocument();

                StreamReader reader = new StreamReader(new FileStream(@"Templates\SpareSalesByCodePeriodReport.xaml", FileMode.Open, FileAccess.Read));
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
                dt.Columns.Add("WarehouseName", typeof(string));
                dt.Columns.Add("OutgoNum", typeof(string));
                dt.Columns.Add("AccountName", typeof(string));
                dt.Columns.Add("OutgoDate", typeof(string));
                dt.Columns.Add("Q", typeof(int));
                dt.Columns.Add("P", typeof(double));
                dt.Columns.Add("VAT", typeof(string));
                dt.Columns.Add("T", typeof(double));

                // забиваем таблицу данными
                List<SpareInSpareOutgoView> LIST2 = da.GetSpareInSpareOutgoByCodePeriod(Spare.id, dateFrom, dateTo, WarehouseID);
                decimal asum = 0;
                try
                {
                    for (int i = 0; i < LIST2.Count; i++)
                    {
                        asum += LIST2[i].total_sum;
                        string AN = LIST2[i].AccountName;
                        string od = LIST2[i].OutgoDate.Value.ToShortDateString();
                        string on = "нет";
                        int OutgoID = LIST2[i].spare_outgo_id;
                        spare_outgo so = da.SpareOutgoGet(OutgoID);
                        if (so != null)
                            on = so.IDN.ToString();
                        dt.Rows.Add(new object[] {
                            i+1,
                            LIST2[i].WarehouseName,
                            on,
                            AN,
                            od,
                            LIST2[i].quantity,
                            LIST2[i].purchase_price,
                            LIST2[i].VatRateName,
                            LIST2[i].total_sum
                        });
                    }
                }
                catch (Exception exc)
                {
                    throw exc;
                }
                string str_ts = RSDN.RusCurrency.Str(asum, "BYR");
                string strDate = dateFrom.Day.ToString();
                string mnth = "";
                switch (dateFrom.Month)
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
                strDate += " " + mnth + " " + dateFrom.Year + " г.";

                data.ReportDocumentValues.Add("ReportDate1", strDate); // print date is now

                // =======================
                strDate = dateTo.Day.ToString();
                mnth = "";
                switch (dateTo.Month)
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
                strDate += " " + mnth + " " + dateTo.Year + " г.";
                data.ReportDocumentValues.Add("ReportDate2", strDate); // print date is now

                data.ReportDocumentValues.Add("SpareName", Spare.name);
                data.ReportDocumentValues.Add("SpareCodeShatem", Spare.codeShatem);
                data.ReportDocumentValues.Add("SpareCode", Spare.code);
                data.ReportDocumentValues.Add("asum", asum);
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
    }
}