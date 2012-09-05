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
using bycar3.Views.Common;
using bycar.Utils;
using System.Reflection;
using bycar3.External_Code;
using bycar3.Views.Reporting.OfferingMovement;

namespace bycar3.Views.Spare_Income
{
    /// <summary>
    /// Interaction logic for SpareIncomeEditView.xaml
    /// </summary>
    public partial class SpareIncomeEditView : Window
    {
        #region DATA MEMBERS
        DataAccess da = new DataAccess();
        public int _id = -1;
        bool _isNew = false;
        
        #endregion

        // CUSTOM FUNCTIONS
        void CheckCurrencyRateExistance(string CurrencyName)
        {

        }

        // HANDLERS
        public SpareIncomeEditView()
        {
            InitializeComponent();
            edtNumber.Text = da.SpareIncomeGetMaxId().ToString();            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            edtDate.SelectedDate = DateTime.Now;            
            LoadComboboxCurrency();
            LoadComboboxWarehouse();
            LoadComboboxAccount();
            LoadItem();

/*            DataAccess db = new DataAccess();
            settings_profile sp = db.getProfileCurrent();
            bool val = sp.IncomeSmart.HasValue?(sp.IncomeSmart.Value == 1?true:false) :false;
            cbSmartIncome.IsChecked = val;
 */
        }        
        private void LoadComboboxCurrency()
        {
            List<currency> items = da.GetCurrency();
            foreach (currency i in items)
            {
                edtCurrency.Items.Add(i.code);
            }
            //edtCurrency.SelectedIndex = 0;
            settings_profile profile = da.getProfileCurrent();
            edtCurrency.SelectedItem = profile.DefaultIncomeCurrencyCode.Replace(" ", String.Empty);
        }
        private void LoadComboboxWarehouse()
        {
            edtWarehouse.Items.Clear();
            List<warehouse> items = da.GetWarehouses();
            foreach (warehouse i in items)
            {
                edtWarehouse.Items.Add(i.name);
            }
            edtWarehouse.SelectedIndex = 0;
        }
        private void LoadComboboxAccount()
        {
            //1 load all
            edtAccount.Items.Clear();
            List<account> items = da.GetAllAccounts();
            foreach (account i in items)
            {
                edtAccount.Items.Add(i.name);
            }            
        }

        private void btnAccountSelect_Click(object sender, RoutedEventArgs e)
        {
            //2 select window
            SelectView v = new SelectView();
            v.ClassName = (new account()).ToString();
            v.ShowDialog();
            LoadComboboxAccount();
            edtAccount.SelectedItem = v.RES;
        }

        private void btnWarehouseSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectView v = new SelectView();
            v.ClassName = (new warehouse()).ToString();
            v.ShowDialog();
            LoadComboboxWarehouse();
            edtWarehouse.SelectedItem = v.RES;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (SaveItem())
            {
                _isNew = false;
                this.Close();
            }
            else
            {
                MessageBox.Show("Проверьте правильность заполнения полей (возможно, не указан контрагент)!");
            }
        }
        bool SaveItem()
        {            
            bool res = true;
            if (_id > 0)
            {
                EditItem();
            }
            else
            {
               _id = CreateItem();
            }            
            return res;
        }
        int CreateItem()
        {
            string _WarehouseName = edtWarehouse.SelectedItem.ToString();
            string _CurrencyCode = edtCurrency.SelectedItem.ToString();
            //3. create item
            string _AccountName = "";
            if(edtAccount.SelectedItem != null)
                _AccountName = edtAccount.SelectedItem.ToString();
            return da.SpareIncomeCreate(getItemFromFields(), _AccountName, _WarehouseName, _CurrencyCode);
        }
        void EditItem()
        {
            string _WarehouseName = edtWarehouse.SelectedItem.ToString();
            string _CurrencyName = edtCurrency.SelectedItem.ToString();
            string _AccountName = "";
            // 4. edit item
            if (edtAccount.SelectedItem != null)
                _AccountName = edtAccount.SelectedItem.ToString();
            da.SpareIncomeEdit(getItemFromFields(), _AccountName, _WarehouseName, _CurrencyName);
        }
        spare_income getItemFromFields()
        {
            spare_income item = new spare_income();
            item.id = _id;
            item.num = edtNumber.Text;
            int txtN = da.SpareIncomeGetMaxId();
            Int32.TryParse(edtNumber.Text, out txtN);
            item.IDN = txtN;                
            item.si_date = edtDate.SelectedDate.HasValue ? edtDate.SelectedDate.Value : DateTime.Now;
            item.base_doc = edtBasedOnDoc.Text;
            item.base_doc_date = edtBasedOnDate.SelectedDate.HasValue ? edtBasedOnDate.SelectedDate.Value : DateTime.Now;
            //item.cashless = edtCashless.IsChecked.HasValue ? (edtCashless.IsChecked.Value ? 1 : 0) : 0;
            return item;
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {            
            this.Close();
        }
        void DeleteNewCreated()
        {
            da = new DataAccess();
            da.SpareIncomeDelete(_id);
        }

        void AddNewOffering()
        {          
            SpareInIncomeSelectView v = new SpareInIncomeSelectView();
            v._ParentWindow = this;
            v._SpareIncomeID = this._id;
            v._OfferingNumber = dgSpares.Items.Count;
            v.CurrenctCurrencyCode = edtCurrency.SelectedItem.ToString();
            v.ShowDialog();
            LoadOfferings();            
        }

        private void btnSpareAdd_Click(object sender, RoutedEventArgs e)
        {
            // сохранить накладную
            SaveItem();
            //добавить новый товар в список
            AddNewOffering();
        }

        private void btnSpareEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgSpares.SelectedItem == null)
            {
                MessageBox.Show("Сначала выберите деталь из списка");
                return;
            }
            // сохранить накладную
            SaveItem();
            //редактировать товар в списке
            EditSelectedOffering();
        }
        void EditSelectedOffering()
        {          

            SpareInIncomeEditView v = new SpareInIncomeEditView();
            v._OfferingID = getSelectedOfferingId();
            v.CurrentCurrencyCode = edtCurrency.SelectedItem.ToString();
            v.ShowDialog();
            LoadOfferings();
        }
        int getSelectedOfferingId()
        {
            int result = 0;
            try
            {
                if (dgSpares.SelectedItem != null)
                {
                    object sel = dgSpares.SelectedItem;
                    result = ((SpareInSpareIncomeView)sel).id;
                }
                else
                {
                    MessageBox.Show("Сначала выберите деталь из списка");
                }
            }
            catch (Exception)
            {
                LoadOfferings();
            }
            return result;
        }
        private void btnSpareDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgSpares.SelectedItem == null)
            {
                MessageBox.Show("Сначала выберите деталь из списка");
                return;
            }
            MessageBoxResult res = MessageBox.Show("Будут удалены связанные с этим приходом отгрузки и переоценки. Желаете продолжить?", "Удаление", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                // сохранить накладную
                SaveItem();
                // удалить выделенный товар
                DeleteSelectedOffering();
            }
        }
        void DeleteSelectedOffering()
        {
            int offeringId = getSelectedOfferingId();
            da.InOfferingDelete(offeringId);
            
            //SpareView sv = da.GetSpareView(SpareID);
            //SpareContainer.Instance.Update(SpareViewItem);           
            
            LoadOfferings();
        }
        void LoadItem()
        {
            if (_id > 0)
            {
                //5. load item
                spare_income item = da.SpareIncomeGet(_id);
                if (item.account != null)
                {
                    edtAccount.SelectedItem = item.account.name;
                }
                edtWarehouse.SelectedItem = item.warehouse.name;
                edtCurrency.SelectedItem = item.currency.code;
                edtBasedOnDate.SelectedDate = item.base_doc_date.HasValue ? item.base_doc_date.Value : DateTime.Now;
                edtBasedOnDoc.Text = item.base_doc;
                edtDate.SelectedDate = item.si_date;
                edtNumber.Text = item.IDN.ToString();
                //edtCashless.IsChecked = item.cashless == 1 ? true : false;
                // загрузить товары в накладной
                LoadOfferings();

                ////dgSpares.IsEnabled = false;
                ////btnSpareDelete.IsEnabled = false;
                ////btnSpareEdit.IsEnabled = false;
            }
            else
            {
                _isNew = true;
                ////dgSpares.IsEnabled = true;
                ////btnSpareDelete.IsEnabled = true;
                ////btnSpareEdit.IsEnabled = true;
                //edtCashless.IsChecked = da.getProfileCurrent().DefaultIsCashless == 1 ? true : false;
            }
        }
        public decimal LoadOfferings()
        {
            decimal sum = 0;
            try
            {
                int n = 1;
                da = new DataAccess();
                List<SpareInSpareIncomeView> items = da.GetIncomesByIncomeID(this._id);
                foreach (SpareInSpareIncomeView i in items)
                {
                    i.num = n;
                    sum += i.S.Value;
                    n++;
                }
                dgSpares.DataContext = items;
                edtSum.Content = "Сумма: " + sum.ToString();
                edtCount.Content = "Позиций в накладной: " + items.Count.ToString();
                if (items.Count > 0)
                    edtCurrency.IsEnabled = false;
                //OnAnyChangeCheck();
            }
            catch (Exception ex1)
            {
                MessageBox.Show(ex1.Message);
            }
            return sum;
        }

        private void dgSpares_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // сохранить накладную
            SaveItem();
            //редактировать товар в списке
            EditSelectedOffering();
        }

        private void edtCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckCurrencyRateExistance(edtCurrency.Text);
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            if (SaveItem())
            {
                //====GenerateReportIncomes();
                OfferingMovementReportView r = new OfferingMovementReportView();
                r.PrepareSpareIncome(this._id);
                r.ShowDialog();
            }
        }
        /*
        void ExportToExcel()
        {
            if (dgSpares.Items.Count == 0)
            {
                MessageBox.Show("В накладной нет ни одной позиции. Печать невозможна.");
                return;
            }
            List<ReportIncome> list = new List<ReportIncome>();
            DataAccess da = new DataAccess();
            list = da.GetReportIncomes(this._id);
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
                if (edtDate.SelectedDate.Value.Day < 10)
                    headerDate += "0";
                headerDate += edtDate.SelectedDate.Value.Day + ".";
                if (edtDate.SelectedDate.Value.Month < 10)
                    headerDate += "0";
                headerDate += edtDate.SelectedDate.Value.Month + ".";
                headerDate += edtDate.SelectedDate.Value.Year;

                string header1 = "Реестр розничных цен на товар №" + this._id + " от " + headerDate;                
                string rng1 = "$A" + row + ":$K" + row;
                Excel.Range r = eWorksheet.get_Range(rng1);
                r.Merge(Type.Missing); 
                eWorksheet.Cells[row,1] = header1;
                row++;

                string CompanyName = da.getProfileCurrent().CompanyName;
                string header2 = "Приложение к ТТН №" + this._id + " от " + headerDate + ", " + CompanyName;
                string rng2 = "$A" + row + ":$K" + row;
                Excel.Range r2 = eWorksheet.get_Range(rng2);
                r2.Merge(Type.Missing);                
                eWorksheet.Cells[row,1] = header2;
                row+=2;

                
                int col = 1;
                eWorksheet.Cells[row, col++] = " № п/п";
                //                if (cbName.IsChecked.Value)
                eWorksheet.Cells[row, col++] = " Наименование товара                             ";
                //if (cbUnit.IsChecked.Value)
                eWorksheet.Cells[row, col++] = " Ед.изм. ";
                //if (cbQ.IsChecked.Value)
                eWorksheet.Cells[row, col++] = " Количество";
                //if (cbPrice.IsChecked.Value)
                eWorksheet.Cells[row, col++] = " Цена пок., " + CCC;
                //if (cbPers.IsChecked.Value)
                eWorksheet.Cells[row, col++] = " Надбавка, %";
                eWorksheet.Cells[row, col++] = " Надбавка, " + CCC;
                //if (cbVat.IsChecked.Value)
                eWorksheet.Cells[row, col++] = " НДС, %";
                eWorksheet.Cells[row, col++] = " НДС, " + CCC;
                //if (cbPriceFull.IsChecked.Value)
                eWorksheet.Cells[row, col++] = " Цена розн., " + CCC;
                //if (cbTotal.IsChecked.Value)
                eWorksheet.Cells[row, col++] = " Сумма, " + CCC;

                string rng = "$A1:$K" + row;                
                eWorksheet.Range[rng].Font.Bold = true;
                eWorksheet.Range[rng].HorizontalAlignment = Excel.Constants.xlCenter;
                eWorksheet.Columns.AutoFit();
                row++;

                int TableStartRow = row-1;
                double TotalAmount = 0;
                int TotalQ = 0;
                // DATA OUTPUT
                for (int i = 0; i < list.Count; i++)
                {
                    col = 1;
                    eWorksheet.Cells[row, col++] = (i + 1).ToString();
                    //if (cbName.IsChecked.Value)
                    eWorksheet.Cells[row, col++] = list[i].SpareName;
                    //if (cbUnit.IsChecked.Value)
                    eWorksheet.Cells[row, col++] = list[i].UnitName;
                    //if (cbQ.IsChecked.Value)
                    eWorksheet.Cells[row, col++] = list[i].quantity;
                    TotalQ += (int)list[i].quantity;
                    //double Price1 = CurrencyHelper.GetBasicPrice(list[i].CurrencyCode, list[i].Price.Value);
                    //double Price = CurrencyHelper.GetPrice(CCC, Price1);
//                    int tmp = (int)(Price * 100);
                    //Price = (double)tmp / 100;
                    double Price = list[i].Price.Value;                                        
                    double PriceFull = list[i].PriceFull.Value;
                    double Vat = Price * list[i].VatRate / 100;
                    //double Total1 = CurrencyHelper.GetBasicPrice(list[i].CurrencyCode, list[i].TotalSum);
                    //double Total = CurrencyHelper.GetPrice(CCC, Total1);                    
                    double Total = list[i].TotalSum;
                    TotalAmount += Total;
                    //if (cbPrice.IsChecked.Value)
                    eWorksheet.Cells[row, col++] = Price;
                    //if (cbPers.IsChecked.Value)
                    double up = Price*list[i].Markup.Value/100;
                    eWorksheet.Cells[row, col++] = list[i].Markup.Value;
                    eWorksheet.Cells[row, col++] = (up).ToString();
                    //if (cbVat.IsChecked.Value)
                    eWorksheet.Cells[row, col++] = list[i].VatRate.ToString();
                    eWorksheet.Cells[row, col++] = Vat;
                    //if (cbPriceFull.IsChecked.Value)
                    eWorksheet.Cells[row, col++] = PriceFull;
                    //if (cbTotal.IsChecked.Value)
                    eWorksheet.Cells[row, col++] = Total;                    
                    row++;
                }
                //if (cbTotal.IsChecked.Value)
                {
                    eWorksheet.Cells[row, 11] = TotalAmount;
                    eWorksheet.Cells[row, 4] = TotalQ;
                    eWorksheet.Cells[row, 2] = "Итого:";
                    string rngx = "$A" + row + ":$K" + row;                
                    eWorksheet.Range[rngx].Font.Bold = true;

                }

                int TableFinishRow = row;
                string rng0 = "$A" + TableStartRow + ":$K" + TableFinishRow;
                Excel.Range r0 = eWorksheet.get_Range(rng0);
                r0.Borders.Weight = 2;

                row++;
                string footer1 = "Всего на сумму: " + RSDN.RusCurrency.Str(TotalAmount, CCC);
                string rngf1 = "$A" + row + ":$K" + row;
                Excel.Range rf1 = eWorksheet.get_Range(rngf1);
                rf1.Merge(Type.Missing);
                eWorksheet.Cells[row, 1] = footer1;
                row++;

                string footer2 = "Товар получил: ";
                string rngf2 = "$A" + row + ":$K" + row;
                Excel.Range rf2 = eWorksheet.get_Range(rngf2);
                rf2.Merge(Type.Missing);
                eWorksheet.Cells[row, 1] = footer2;

                eWorkbook.Application.Visible = true;
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }*/

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_isNew && _id > 0)
            {
                DeleteNewCreated();
            }
            /*int val = cbSmartIncome.IsChecked.HasValue?(cbSmartIncome.IsChecked.Value?1:0):0;
            DataAccess db = new DataAccess();
            settings_profile sp = db.getProfileCurrent();
            sp.IncomeSmart = val;
            db.ProfileEdit(sp);*/
        }
        void OnAnyChangeCheck()
        {
            edtCurrency.IsEnabled = !dgSpares.HasItems;
        }

        private void btnSmartIncome_Click(object sender, RoutedEventArgs e)
        {
            if (this._id < 1)            
                SaveItem();  
          
            SpareIncomeSmart v = new SpareIncomeSmart(this._id);
            v.ShowDialog();
            LoadOfferings();
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            if (this._id < 1)
                SaveItem();  

            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".csv";
            dlg.Filter = "csv documents (.csv)|*.csv";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                SpareIncomeSmart v = new SpareIncomeSmart(this._id, filename);
                v.ShowDialog();
                LoadOfferings();
            }
        }                
    }
}
