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
using System.Data;
using System.IO;
using System.Windows.Xps.Packaging;
using CodeReason.Reports;
using bycar;


namespace bycar3.Views.Reporting.OfferingMovement
{
    /// <summary>
    /// Логика взаимодействия для OfferingMovementReportView.xaml
    /// </summary>
    public partial class OfferingMovementReportView : Window
    {
        #region DATA MEMBERS
        int SpareIncomeID = -1;
        spare_income Income = null;
        #endregion
        public OfferingMovementReportView()
        {
            InitializeComponent();
        }
        public void PrepareSpareIncome(int SpareIncomeId)
        {
            this.SpareIncomeID = SpareIncomeId;
            DataAccess da = new DataAccess();
            Income = da.SpareIncomeGet(SpareIncomeID);
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (Income == null)
                return;
            try
            {
                ReportDocument reportDocument = new ReportDocument();

                StreamReader reader = new StreamReader(new FileStream(@"Templates\OfferingMovementReport.xaml", FileMode.Open, FileAccess.Read));
                reportDocument.XamlData = reader.ReadToEnd();
                reportDocument.XamlImagePath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Templates\");
                reader.Close();

                ReportData data = new ReportData();

                // set constant document values
                data.ReportDocumentValues.Add("IncomeDate", Income.si_date.GetDateTimeFormats('d')[3]); // print date is now
                data.ReportDocumentValues.Add("IncomeNumber", Income.num);

                decimal ts = 0;
                decimal _S = 0;
                decimal _MS = 0;
                decimal _VS = 0;                
                DataAccess da = new DataAccess();
                string BCC = da.getBasicCurrencyCode();
                Income.currencyReference.Load();
                string CCC = Income.currency.code;

                // Таблица ТОВАРЫ В НАКЛАДНОЙ
                DataTable dt = new DataTable("OfferingsInMovement");
                // описываем столбцы таблицы
                dt.Columns.Add("Number", typeof(int));
                dt.Columns.Add("SpareName", typeof(string));
                dt.Columns.Add("SpareCodeShatem", typeof(string));
                dt.Columns.Add("SpareCode", typeof(string));
                dt.Columns.Add("UnitName", typeof(string));
                dt.Columns.Add("Quantity", typeof(int));
                dt.Columns.Add("Price", typeof(double));                
                dt.Columns.Add("Markup", typeof(double));                
                dt.Columns.Add("VAT", typeof(double));
                dt.Columns.Add("Sum", typeof(double));
                dt.Columns.Add("MarkupBasic", typeof(double));
                dt.Columns.Add("VATBasic", typeof(double));                
                dt.Columns.Add("Amount", typeof(double));
                // забиваем таблицу данными
                List<ReportIncome> list = new List<ReportIncome>();                
                list = da.GetReportIncomes(SpareIncomeID);
                try
                {
                    string CompanyName = da.getProfileCurrent().CompanyName;                    
                    for (int i = 0; i < list.Count; i++)
                    {
                        decimal Q = list[i].QIn;
                        decimal P = list[i].PIn.Value;
                        decimal S = P * Q;
                        decimal M = list[i].Markup;
                        decimal V = list[i].VatRate;
                        decimal T = list[i].S.Value;
                        decimal MS = S * M / 100;
                        decimal VS = S * V / 100;
                        //MS = Math.Round(MS * 100) / 100;
                        //VS = Math.Round(VS * 100) / 100;

                        _MS += MS;
                        _VS += VS;
                        _S += S;
                        ts += T;
                                               
                        dt.Rows.Add(
                            new object[] {                            
                            i+1,                            
                            list[i].SpareName,
                            list[i].codeShatem,
                            list[i].code,                
                            list[i].UnitName,                
                            Q,                
                            P,
                            M,
                            V,
                            S,
                            MS,
                            VS,
                            T
                             });
                    }
                }
                catch (Exception exc)
                {
                    throw exc;
                }
                string sts = RSDN.RusCurrency.Str(ts, CCC);
                data.ReportDocumentValues.Add("StringSum", sts);
                settings_profile profile = da.getProfileCurrent();
                data.ReportDocumentValues.Add("TS", _S);
                data.ReportDocumentValues.Add("TMS", _MS);
                data.ReportDocumentValues.Add("TVS", _VS);
                data.ReportDocumentValues.Add("TTS", ts);
                data.ReportDocumentValues.Add("param1", profile.CompanyHead);
                data.ReportDocumentValues.Add("param2", profile.CompanyName);
                if (Income.base_doc_date.HasValue)
                    data.ReportDocumentValues.Add("BaseDocDate", Income.base_doc_date.Value.GetDateTimeFormats('d')[3]);
                data.ReportDocumentValues.Add("BaseDoc", Income.base_doc);
                string AccName = "";
                if (Income.account == null)
                    Income.accountReference.Load();
                if (Income.account != null)
                    AccName = Income.account.name;
                data.ReportDocumentValues.Add("AccountName", AccName);
                data.DataTables.Add(dt);
                
                DateTime dateTimeStart = DateTime.Now; // start time measure here

                XpsDocument xps = reportDocument.CreateXpsDocument(data);
                documentViewer.Document = xps.GetFixedDocumentSequence();

                // show the elapsed time in window title
                //Title += " - создано за " + (DateTime.Now - dateTimeStart).TotalMilliseconds + " миллисекунд";
            }
            catch (Exception ex)
            {
                // show exception
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.GetType() + "\r\n" + ex.StackTrace, ex.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }
    }
}
