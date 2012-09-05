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
using System.Text.RegularExpressions;
using bycar3.Core;

namespace bycar3.Reporting
{
    /// <summary>
    /// Interaction logic for ReportViewInvoice.xaml
    /// </summary>
    public partial class ReportViewInvoice : Window
    {
        #region DATAMEMBERS
        int InvoiceId = -1;
        invoice Outgo = null;
        #endregion


        public ReportViewInvoice(int id)
        {
            InitializeComponent();
            DataAccess da = new DataAccess();
            InvoiceId = id;
            Outgo = da.InvoiceGet(id);
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (Outgo == null)
                return;
            try
            {
                ReportDocument reportDocument = new ReportDocument();

                StreamReader reader = new StreamReader(new FileStream(@"Templates\InvoiceReport.xaml", FileMode.Open, FileAccess.Read));
                reportDocument.XamlData = reader.ReadToEnd();
                reportDocument.XamlImagePath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Templates\");
                reader.Close();

                ReportData data = new ReportData();                
                decimal asum = 0;
                DataAccess da = new DataAccess();
                string BCC = da.getBasicCurrencyCode();
                //Income.currencyReference.Load();
                //string CCC = Income.currency.code;

                // Таблица ТОВАРЫ В НАКЛАДНОЙ
                DataTable dt = new DataTable("mtable");
                // описываем столбцы таблицы            
                dt.Columns.Add("Num", typeof(int));
                dt.Columns.Add("SpareName", typeof(string));
                dt.Columns.Add("UnitName", typeof(string));
                dt.Columns.Add("Q", typeof(int));
                dt.Columns.Add("P", typeof(double));                
                dt.Columns.Add("VR", typeof(string));                
                dt.Columns.Add("TS", typeof(double));
                // забиваем таблицу данными                
                List<SpareInInvoiceView> LIST2 = da.GetSparesByInvoiceID(InvoiceId);
                try
                {
                    string CompanyName = da.getProfileCurrent().CompanyName;
                    for (int i = 0; i < LIST2.Count; i++)
                    {                        
                        asum += LIST2[i].TotalWithVat.Value;
                        dt.Rows.Add(new object[] { 
                            i+1,
                            (LIST2[i].SpareName +" (" + LIST2[i].SpareCodeShatem+ ")"),
                            "шт.",
                            LIST2[i].quantity,
                            LIST2[i].price.Value,
                            LIST2[i].VatRateName,                            
                            LIST2[i].TotalWithVat
                        });
                    }                    
                }
                catch (Exception exc)
                {
                    Marvin.Instance.Log(exc.Message);
                    throw exc;
                }
                string str_ts = RSDN.RusCurrency.Str(asum, "BYR");
                //string str_vs = RSDN.RusCurrency.Str(vs, "BYR");
                // set constant document values
                //string strDate = Outgo.created_on.Value.GetDateTimeFormats('d')[3];
                string strDate = Outgo.InvoiceDate.Value.Day.ToString();
                string mnth = "";
                switch (Outgo.InvoiceDate.Value.Month)
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
                strDate += " " + mnth + " " + Outgo.InvoiceDate.Value.Year + " г.";
                data.ReportDocumentValues.Add("ReportDate", strDate); // print date is now
                
                data.ReportDocumentValues.Add("TTS", asum);                
                settings_profile profile = da.getProfileCurrent();
                ProfileBankAccountView ProfileBankAccount = da.getProfileBankAccountCurrent();
                string w1 = "не указано";
                string w2 = "не указано";
                string w3 = "не указано";
  
                if (ProfileBankAccount != null)
                {
                    w1 = ProfileBankAccount.BankAccount;;
                    w2 = ProfileBankAccount.BankName;
                    w3 = ProfileBankAccount.BankMFO;

                }
                //string p1 = profile.CompanyName + ", " + profile.AddressJur + ", УНП " + Regex.Replace(profile.UNN, " +", " ");
                //p1 = Regex.Replace(p1, " +", " ");

                // ПРАВИЛЬНЫЕ ПАРАМЕТРЫ
                invoice inv = Outgo;
                string q1 = profile.CompanyName + ", " + profile.AddressJur + ", УНН:" + profile.UNN;
                data.ReportDocumentValues.Add("p1", q1);
                string q2 = w1;
                data.ReportDocumentValues.Add("p2", q2);
                string q3 = w2 + ", МФО:" + w3;
                data.ReportDocumentValues.Add("p3", q3);
                string q4 = profile.CompanyName;
                data.ReportDocumentValues.Add("p4", q4);
                string q5 = profile.LoadPoint;
                data.ReportDocumentValues.Add("p5", q5);
                string q6 = inv.AccountName + ", " + inv.AccountAddress + ", УНН:" + inv.AccountUNN;
                data.ReportDocumentValues.Add("p6", q6);
                string BankAccount = "р/с не указан";
                if (inv.BankAccountID != null)
                {
                    BankAccountView ba = da.BankAccountView(inv.BankAccountID.Value);
                    BankAccount = ba.BankAccount;
                }
                string q7 = BankAccount + ", " + inv.AccountBankName + ", " + inv.AccountBankMFO;
                data.ReportDocumentValues.Add("p7", q7);
                string q8 = inv.AccountName + ", " + inv.AccountAddress;
                data.ReportDocumentValues.Add("p8", q8);
                string q9 = inv.AccountAddress;
                data.ReportDocumentValues.Add("p9", q9);
                string q0 = str_ts;
                data.ReportDocumentValues.Add("p0", q0);
                string q10 = inv.InvoiceNumber.ToString();
                data.ReportDocumentValues.Add("p10", q10);
                string q11 = strDate;
                data.ReportDocumentValues.Add("p11", q11);

                // СТАРЫЕ ПАРАМЕТРЫ
                // Поставщик и его адрес:	
                /*
                data.ReportDocumentValues.Add("param1", p1);
                // Номер инвойса
                string param2 = Outgo.id.ToString();
                data.ReportDocumentValues.Add("param2", param2);
                // Номер счета:
                string param3 = Outgo.AccountBankNum;
                data.ReportDocumentValues.Add("param3", param3);
                // дата
                string param4 = strDate;
                data.ReportDocumentValues.Add("param4", param4);
                // Банк:
                string param5 = "########";
                data.ReportDocumentValues.Add("param5", param5);
                // Грузоотправитель:
                string param6 = profile.CompanyName;
                data.ReportDocumentValues.Add("param6", param6);
                // Ст. отправления:
                string param7 = profile.AddressFact;
                data.ReportDocumentValues.Add("param7", param7);
                // Плательщик и его адрес:
                string param8 = Outgo.AccountName;
                data.ReportDocumentValues.Add("param8", param8);
                // Номер счета и банк:
                string param9 = Outgo.AccountBankNum + ", " + Outgo.AccountBankName;
                data.ReportDocumentValues.Add("param9", param9);
                // Грузополучатель:
                string param10 = Outgo.AccountName;
                data.ReportDocumentValues.Add("param10", param10);
                // Ст. назначения, число мест, вес:
                string param11 = "############";
                data.ReportDocumentValues.Add("param11", param11);
                */
                
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
