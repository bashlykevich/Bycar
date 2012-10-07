using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

//using Excel = Microsoft.Office.Interop.Excel;
using bycar;

namespace bycar3.Views.Reporting
{
    /// <summary>
    /// Interaction logic for ReportFilterOutgoesView.xaml
    /// </summary>
    public partial class ReportFilterOutgoesView : Window
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
        void GenerateReportOutgoes()
        {
            List<ReportOutgo> list = new List<ReportOutgo>();
            DataAccess da = new DataAccess();
            list = da.GetReportOutgoes(edtStart.SelectedDate.Value, edtFinish.SelectedDate.Value);
            try
            {
                string BCC = da.getBasicCurrencyCode();
                string CCC = edtCurrency.SelectedItem.ToString();
                Excel.Workbook eWorkbook;
                Excel.Worksheet eWorksheet;
                eWorkbook = new Excel.Application().Workbooks.Add(Missing.Value);
                eWorksheet = eWorkbook.ActiveSheet as Excel.Worksheet;

                int row = 1;
                string headerDate1 = "";
                if (edtStart.SelectedDate.Value.Day < 10)
                    headerDate1 += "0";
                headerDate1 += edtStart.SelectedDate.Value.Day + ".";
                if (edtStart.SelectedDate.Value.Month < 10)
                    headerDate1 += "0";
                headerDate1 += edtStart.SelectedDate.Value.Month + ".";
                headerDate1 += edtStart.SelectedDate.Value.Year;

                string headerDate2 = "";
                if (edtFinish.SelectedDate.Value.Day < 10)
                    headerDate2 += "0";
                headerDate2 += edtFinish.SelectedDate.Value.Day + ".";
                if (edtFinish.SelectedDate.Value.Month < 10)
                    headerDate2 += "0";
                headerDate2 += edtFinish.SelectedDate.Value.Month + ".";
                headerDate2 += edtFinish.SelectedDate.Value.Year;

                string header1 = "Расход за период с " + headerDate1 + " по " + headerDate2;
                string rng1 = "$A" + row + ":$H" + row;
                Excel.Range r = eWorksheet.get_Range(rng1);
                r.Merge(Type.Missing);
                eWorksheet.Cells[row, 1] = header1;
                row += 2;

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
                    eWorksheet.Cells[row, col++] = " Цена, " + CCC;
                if (cbVat.IsChecked.Value)
                    eWorksheet.Cells[row, col++] = " НДС, %";
                if (cbPriceFull.IsChecked.Value)
                    eWorksheet.Cells[row, col++] = " Цена с НДС, " + CCC;
                if (cbTotal.IsChecked.Value)
                {
                    eWorksheet.Cells[row, col++] = " Сумма с НДС, " + CCC;
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
                        eWorksheet.Cells[row, col++] = list[i].quantity;
                    TotalQ += (int)list[i].quantity;

                    double xTotalAmount = list[i].PriceOutBasic.Value;
                    double _TotalAmount = CurrencyHelper.GetPrice(CCC, xTotalAmount);
                    int tmp = (int)(_TotalAmount * 100);
                    _TotalAmount = (double)tmp / 100;

                    double VatRate = list[i].VatRate;
                    double _Vat = _TotalAmount * VatRate / 100;

                    double _Price = _TotalAmount - _Vat;

                    if (cbPrice.IsChecked.Value)
                        eWorksheet.Cells[row, col++] = _Price;
                    if (cbVat.IsChecked.Value)
                        eWorksheet.Cells[row, col++] = list[i].VatRate;
                    if (cbPriceFull.IsChecked.Value)
                        eWorksheet.Cells[row, col++] = _TotalAmount;
                    if (cbTotal.IsChecked.Value)
                        eWorksheet.Cells[row, col++] = _TotalAmount * list[i].quantity;
                    TotalAmount += _TotalAmount;
                    row++;
                }
                if (cbTotal.IsChecked.Value)
                {
                    eWorksheet.Cells[row, colS] = TotalAmount;
                    eWorksheet.Cells[row, colQ] = TotalQ;
                    eWorksheet.Cells[row, 2] = "Итого:";
                    string rngx = "$A" + row + ":$H" + row;
                    eWorksheet.Range[rngx].Font.Bold = true;
                }
                int TableFinishRow = row;
                string rng0 = "$A" + TableStartRow + ":$H" + TableFinishRow;
                Excel.Range r0 = eWorksheet.get_Range(rng0);
                r0.Borders.Weight = 2;

                row++;
                string footer1 = "Всего на сумму: " + RSDN.RusCurrency.Str(TotalAmount, CCC);
                string rngf1 = "$A" + row + ":$H" + row;
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
        public ReportFilterOutgoesView()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            //GenerateReportOutgoes();
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            edtFinish.SelectedDate = DateTime.Now;
            edtStart.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day - 1);
            LoadComboboxCurrencies();
        }
    }
}