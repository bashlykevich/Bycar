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
                dt.Columns.Add("MarkupBasic", typeof(double));
                dt.Columns.Add("VAT", typeof(double));
                dt.Columns.Add("VATBasic", typeof(double));
                dt.Columns.Add("SalePrice", typeof(double));
                dt.Columns.Add("Amount", typeof(double));
                // забиваем таблицу данными
                List<ReportIncome> list = new List<ReportIncome>();                
                list = da.GetReportIncomes(SpareIncomeID);
                try
                {
                    string CompanyName = da.getProfileCurrent().CompanyName;                    
                    for (int i = 0; i < list.Count; i++)
                    {
                        decimal Price = list[i].PIn.Value;
                        decimal PriceFull = list[i].POut.Value;
                        decimal Vat = Price * list[i].VatRate / 100;                        
                        decimal Total = list[i].S.Value;                                                
                        decimal up = Price * list[i].Markup / 100;
                        ts += Total;
                                               
                        dt.Rows.Add(
                            new object[] {
                            // Number int
                            i+1,
                            //dt.Columns.Add("SpareName", typeof(string));
                            list[i].SpareName,
                            list[i].codeShatem,
                            list[i].code,
                //dt.Columns.Add("UnitName", typeof(string));
                            list[i].UnitName,
                //dt.Columns.Add("Quantity", int
                            list[i].QIn,
                //dt.Columns.Add("Price", double
                            Price,
                //dt.Columns.Add("Markup", double
                            list[i].Markup,
                //dt.Columns.Add("MarkupBasic", double
                            up,
                //dt.Columns.Add("VAT", double
                            list[i].VatRate,
                //dt.Columns.Add("VATBasic", double
                            Vat,
                //dt.Columns.Add("SalePrice", double
                            PriceFull,
                //dt.Columns.Add("Amount", double
                            Total
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
