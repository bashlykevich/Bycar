using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using bycar;

//using Excel = Microsoft.Office.Interop.Excel;

namespace bycar3.Views.Reporting
{
    /// <summary>
    /// Interaction logic for ReportFilterRemainsView.xaml
    /// </summary>
    public partial class ReportFilterRemainsView : Window
    {
        // CUSTOM FUNCTIONS
        private void LoadComboboxCurrencies()
        {
            DataAccess da = new DataAccess();
            List<currency> x = da.GetCurrency();
            edtCurrency.Items.Clear();
            foreach (currency c in x)
                edtCurrency.Items.Add(c.code);
            edtCurrency.SelectedItem = da.getBasicCurrencyCode();
        }

        /*
        void GenerateReportRemains()
        {
            List<SpareInSpareIncomeView> list = new List<SpareInSpareIncomeView>();
            DataAccess da = new DataAccess();
            list = da.GetReportRemains();
            try
            {
                string BCC = da.getBasicCurrencyCode();
                string CCC = edtCurrency.SelectedItem.ToString();
                Excel.Workbook eWorkbook;
                Excel.Worksheet eWorksheet;
                eWorkbook = new Excel.Application().Workbooks.Add(Missing.Value);
                eWorksheet = eWorkbook.ActiveSheet as Excel.Worksheet;

                int row = 1;
                string headerDate = "";
                if (DateTime.Now.Day < 10)
                    headerDate += "0";
                headerDate += DateTime.Now.Day + ".";
                if (DateTime.Now.Month < 10)
                    headerDate += "0";
                headerDate += DateTime.Now.Month + ".";
                headerDate += DateTime.Now.Year;

                string header1 = "Остатки на " + headerDate + ".";
                string rng1 = "$A" + row + ":$I" + row;
                Excel.Range r = eWorksheet.get_Range(rng1);
                r.Merge(Type.Missing);
                eWorksheet.Cells[row, 1] = header1;
                row+=2;

                int col = 1;
                int colQ = 0;
                int colS = 0;
                eWorksheet.Cells[row, col++] = " № п/п";
                if (cbName.IsChecked.Value)
                    eWorksheet.Cells[row, col++] = " Наименование товара                             ";
                if (cbUnit.IsChecked.Value)
                    eWorksheet.Cells[row, col++] = " Ед.изм. ";
                if (cbQ.IsChecked.Value)
                {
                    eWorksheet.Cells[row, col++] = " Количество";
                    colQ = col - 1;
                }
                if (cbPrice.IsChecked.Value)
                    eWorksheet.Cells[row, col++] = " Цена покупная, " + CCC;
                if (cbPers.IsChecked.Value)
                    eWorksheet.Cells[row, col++] = " Торговая надбавка, " + CCC;
                if (cbVat.IsChecked.Value)
                    eWorksheet.Cells[row, col++] = " НДС, %";
                if (cbPriceFull.IsChecked.Value)
                    eWorksheet.Cells[row, col++] = " Цена розничная, " + CCC;
                if (cbTotal.IsChecked.Value)
                {
                    eWorksheet.Cells[row, col++] = " Сумма, " + CCC;
                    colS = col - 1;
                }

                string rng = "$A1:$I" + row;
                eWorksheet.Range[rng].Font.Bold = true;
                eWorksheet.Range[rng].HorizontalAlignment = Excel.Constants.xlCenter;
                eWorksheet.Columns.AutoFit();
                row++;

                int TableStartRow = row - 1;
                double TotalAmount = 0;
                int TotalQ = 0;

                // DATA OUTPUT
                for (int i = 0; i < list.Count; i++)
                {
                    col = 1;
                    eWorksheet.Cells[row, col++] = (i + 1).ToString();
                    if (cbName.IsChecked.Value)
                        eWorksheet.Cells[row, col++] = list[i].SpareName;
                    if (cbUnit.IsChecked.Value)
                        eWorksheet.Cells[row, col++] = list[i].UnitName;
                    if (cbQ.IsChecked.Value)
                        eWorksheet.Cells[row, col++] = list[i].QRest;
                    TotalQ += (int)list[i].QRest;
                    double Price1 = CurrencyHelper.GetBasicPrice(list[i].CurrencyCode, list[i].Price.Value);
                    double Price = CurrencyHelper.GetPrice(CCC, Price1);
                    int tmp = (int)(Price * 100);
                    Price = (double)tmp / 100;

                    double PriceFull1 = list[i].BasicPrice.Value;
                    double PriceFull = CurrencyHelper.GetPrice(CCC, PriceFull1);

                    tmp = (int)(PriceFull * 100);
                    PriceFull = (double)tmp / 100;

                    double VatRate = da.getVatRate(list[i].VatID);
                    double Vat = Price * VatRate / 100;
                    double Total1 = CurrencyHelper.GetBasicPrice(list[i].CurrencyCode, list[i].TotalSum);
                    double Total = CurrencyHelper.GetPrice(CCC, Total1);
                    tmp = (int)(Total * 100);
                    Total = (double)tmp / 100;
                    TotalAmount += Total;
                    if (cbPrice.IsChecked.Value)
                        eWorksheet.Cells[row, col++] = Price;
                    if (cbPers.IsChecked.Value)
                        eWorksheet.Cells[row, col++] = (PriceFull - Price - Vat).ToString();
                    if (cbVat.IsChecked.Value)
                        eWorksheet.Cells[row, col++] = list[i].VatName;
                    if (cbPriceFull.IsChecked.Value)
                        eWorksheet.Cells[row, col++] = PriceFull;
                    if (cbTotal.IsChecked.Value)
                        eWorksheet.Cells[row, col++] = Total;
                    row++;
                }
                if (cbTotal.IsChecked.Value)
                {
                    eWorksheet.Cells[row, colS] = TotalAmount;
                    eWorksheet.Cells[row, colQ] = TotalQ;
                    eWorksheet.Cells[row, 2] = "Итого:";
                    string rngx = "$A" + row + ":$I" + row;
                    eWorksheet.Range[rngx].Font.Bold = true;
                }
                int TableFinishRow = row;
                string rng0 = "$A" + TableStartRow + ":$I" + TableFinishRow;
                Excel.Range r0 = eWorksheet.get_Range(rng0);
                r0.Borders.Weight = 2;

                row++;
                string footer1 = "Всего на сумму: " + RSDN.RusCurrency.Str(TotalAmount, CCC);
                string rngf1 = "$A" + row + ":$I" + row;
                Excel.Range rf1 = eWorksheet.get_Range(rngf1);
                rf1.Merge(Type.Missing);
                eWorksheet.Cells[row, 1] = footer1;

                eWorkbook.Application.Visible = true;
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }*/

        // HANDLERS

        public ReportFilterRemainsView()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            //GenerateReportRemains();
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadComboboxCurrencies();
        }
    }
}