using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using bycar;
using bycar3.Core;
using bycar3.External_Code;
using bycar3.Reporting;
using bycar3.Views;
using bycar3.Views.Common;
using bycar3.Views.Currency;
using bycar3.Views.Reporting;
using bycar3.Views.Request;
using bycar3.Views.Revision;
using bycar3.Views.Revision3;
using DataStreams.Csv;
using Microsoft.Win32;

namespace bycar3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region DATA MEMBERS

        private bycar3.Views.Main_window.UCSpares uc_Spares = null;
        private bycar3.Views.Main_window.UCMovements uc_Movements = null;
        private int _Workspace = 0;
        public bool RevisionModeOn = false;

        #endregion DATA MEMBERS

        private DataAccess da = new DataAccess();

        #region CUSTOM FUNCTIONS

        /* OLD
         *
         *
        /*
        Thread splash = null;
        void SplashThreadStart()
        {
            splash = new Thread(SplashWindowShow);
            splash.IsBackground = true;
            splash.SetApartmentState(ApartmentState.STA);
            splash.Start();
        }
        void SplashThreadStop()
        {
            if (splash != null)
            {
                splash.Abort();
                splash = null;
            }
        }
        void SplashWindowShow()
        {
            new SplashWindow().ShowDialog();
        }
         *  void Start()
        {
            // InitializeWindow();
            int? sfi = da.getProfileCurrent().DefSearchFieldIndex;
            edtSearchField.SelectedIndex = sfi.HasValue ? sfi.Value : 0;

            //LoadCurrencies();     ЗАЧЕМ???
            //ShowUCSpares(true);   ЗАЧЕМ???
            //PrintRemains();       ЗАЧЕМ???

            // SettingsLoad();

            this.Visibility = System.Windows.Visibility.Visible;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //PrintRemains();
            //DataAccess da = new DataAccess();
            /* v#1
            if (!da.AreCurrenciesUpToDate())
            {
                CurrenciesInput v = new CurrenciesInput(DateTime.Now);
                v.ShowDialog();
            }
            this.Visibility = System.Windows.Visibility.Visible;

            // DEBUG COMMAND
            //ImportSpareCodesFromCSV2();
        }
         */

        private void LoadCurrencies()
        {
            DataAccess da = new DataAccess();
            List<currency> lst = da.GetCurrency();
            edtCurrentCurrency.Items.Clear();
            foreach (currency i in lst)
            {
                edtCurrentCurrency.Items.Add(i.code);
                if (i.is_basic == 1)
                    edtCurrentCurrency.SelectedValue = i.code;
            }
        }

        private void ChangeBasicCurrency()
        {
            if (uc_Spares != null)
                uc_Spares.CurrentCurrencyName = edtCurrentCurrency.SelectedItem.ToString();
            /*
            if (!CurrencyChangeListenerOff)
            {
                MessageBoxResult res = MessageBox.Show("Пересчет приходов может занять некоторое время. Вы действительно хотите изменить основную валюту?", "Изменение основной валюты", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    DataAccess da = new DataAccess();
                    string NewBasicCurrencyCode = edtCurrentCurrency.SelectedValue.ToString();
                    da.UpdateBasicCurrencyCode(NewBasicCurrencyCode);
                    LoadCurrencies();
                }
            }*/
        }

        /*
        private void InitializeWindow()
        {
          
            int? sfi = da.getProfileCurrent().DefSearchFieldIndex;
            edtSearchField.SelectedIndex = sfi.HasValue ? sfi.Value : 0;
            LoadCurrencies();
            ShowUCSpares(true);
            PrintRemains();                                
        }*/

        private bool ItemCreate()
        {
            bool flag = false;
            switch (_Workspace)
            {
                case 0: // номенклатура
                    uc_Spares.SpareCreate();
                    break;

                case 1: // товародвижение
                    uc_Movements.ItemCreate();
                    break;
            }
            return flag;
        }

        private void ItemEdit()
        {
            switch (_Workspace)
            {
                case 0: // номенклатура
                    uc_Spares.SpareEdit();
                    break;

                case 1: // товародвижение
                    uc_Movements.ItemEdit();
                    break;
            }
        }

        private void ItemCopy()
        {
            switch (_Workspace)
            {
                case 0: // номенклатура
                    uc_Spares.SpareCopy();
                    break;

                case 1: // товародвижение
                    break;
            }
        }

        private void ItemDelete()
        {
            switch (_Workspace)
            {
                case 0: // номенклатура
                    uc_Spares.SpareDelete();
                    break;

                case 1: // товародвижение
                    uc_Movements.ItemDelete();
                    break;
            }
        }

        private void ImportBrands()
        {
            string FilePath = "";

            // вызов диалога выбора файла для импорта
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "Office 2003 XML Tables (.xml)|*.xml"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                FilePath = dlg.FileName;
                List<brand> items = null;
                items = Office2003XmlTable.getBrands(FilePath);

                int count = items.Count;

                // перенос списка полученных брендов в БД
                if (items != null)
                {
                    DataAccess da = new DataAccess();
                    foreach (brand i in items)
                    {
                        da.BrandCreate(i);
                    }
                    MessageBox.Show("Импортировано " + count + " записей.");
                }
            }
        }

        private void ImportSparesFromCSV()
        {
            ConsoleManager.Show();
            Console.WriteLine(DateTime.Now.ToShortTimeString() + " - started...");
            string FilePath = "";
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".csv"; // Default file extension

            //    dlg.Filter = "*.csv"; // Filter files by extension
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                DataAccess da = new DataAccess();

                // Open document
                FilePath = dlg.FileName;
                int counter = 0;
                int GlobalCnt = 0;
                using (CsvReader csvData = new CsvReader(FilePath))
                {
                    csvData.Settings.Delimiter = ';';
                    while (csvData.ReadRecord())
                    {
                        try
                        {
                            counter++;
                            GlobalCnt++;
                            string[] RawRecord = csvData.RawRecord.Split(';');
                            string GrandParentGroupName = RawRecord[0];
                            string ParentGroupName = RawRecord[1];
                            string BrandName = RawRecord[2];
                            string SpareCode = RawRecord[3];
                            string SpareName = RawRecord[4];
                            string CarModels = RawRecord[5];

                            if (!da.SpareExist(SpareCode, SpareName))
                            {
                                Console.WriteLine(GlobalCnt.ToString() + ": " + SpareName + " - " + SpareCode + " - creating");
                                da.SpareCreate(GrandParentGroupName, ParentGroupName, BrandName, SpareCode, SpareName, CarModels);
                            }
                            else
                            {
                                Console.WriteLine(GlobalCnt.ToString() + ": " + SpareName + " - " + SpareCode + " already exist");
                            }
                            if (counter > 1000)
                            {
                                counter = 0;
                                da = new DataAccess();
                                Console.WriteLine("Data access object refresh");
                            }
                        }
                        catch (Exception e1)
                        {
                            Console.WriteLine(GlobalCnt.ToString() + ": Exception failed: " + e1.Message);
                        }
                    }
                } // dispose of parser
            }
            Console.WriteLine(DateTime.Now.ToShortTimeString() + " - finished...");
            MessageBox.Show("Import finished!");
            Console.ReadLine();
            ConsoleManager.Hide();
            Application.Current.Shutdown();
        }

        private void ImportSpareCodesFromCSV()
        {
            ConsoleManager.Show();
            Console.WriteLine(DateTime.Now.ToShortTimeString() + " - started...");
            string FilePath = "";
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".csv"; // Default file extension

            //    dlg.Filter = "*.csv"; // Filter files by extension
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                DataAccess da = new DataAccess();

                // Open document
                FilePath = dlg.FileName;
                int counter = 0;
                int GlobalCnt = 0;
                using (CsvReader csvData = new CsvReader(FilePath))
                {
                    csvData.Settings.Delimiter = ';';
                    while (csvData.ReadRecord())
                    {
                        try
                        {
                            counter++;
                            GlobalCnt++;
                            string[] RawRecord = csvData.RawRecord.Split(';');
                            string CodeShatem = RawRecord[0];
                            string Code = RawRecord[1];

                            // delete chars after _
                            CodeShatem = CodeShatem.Split('_')[0];

                            if (da.SpareEdit(CodeShatem, Code))
                                Console.WriteLine(GlobalCnt.ToString() + ": " + CodeShatem + " ok!");
                            else
                                Console.WriteLine(GlobalCnt.ToString() + ": " + CodeShatem + " not found.");

                            if (counter > 1000)
                            {
                                counter = 0;
                                da = new DataAccess();
                                Console.WriteLine("Data access object refresh");
                            }
                        }
                        catch (Exception e1)
                        {
                            Console.WriteLine(GlobalCnt.ToString() + ": Exception failed: " + e1.Message);
                        }
                    }
                } // dispose of parser
            }
            Console.WriteLine(DateTime.Now.ToShortTimeString() + " - finished...");
            MessageBox.Show("Import finished!");
            Console.ReadLine();
            ConsoleManager.Hide();
            Application.Current.Shutdown();
        }

        private void ImportSpareCodesFromCSV2()
        {
            ConsoleManager.Show();
            Console.WriteLine(DateTime.Now.ToShortTimeString() + " - started...");
            string FilePath = "";
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".csv"; // Default file extension

            //    dlg.Filter = "*.csv"; // Filter files by extension
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                DataAccess da = new DataAccess();

                // Open document
                FilePath = dlg.FileName;
                using (CsvReader csvData = new CsvReader(FilePath))
                {
                    csvData.Settings.Delimiter = ';';
                    while (csvData.ReadRecord())
                    {
                        da.SpareEdit(csvData.ReadToEnd());
                    }
                } // dispose of parser
            }
            Console.WriteLine(DateTime.Now.ToShortTimeString() + " - finished...");
            MessageBox.Show("Import finished!");
            Console.ReadLine();
            ConsoleManager.Hide();
            Application.Current.Shutdown();
        }

        private void ImportGroups()
        {
            string FilePath = "";

            // вызов диалога выбора файла для импорта
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "Office 2003 XML Tables (.xml)|*.xml"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                FilePath = dlg.FileName;
                List<spare_group> items = null;
                items = Office2003XmlTable.getGroups(FilePath);

                int count = items.Count;

                // перенос списка полученных записе в БД
                if (items != null)
                {
                    MessageBox.Show("Импортировано " + count + " записей.");
                }
            }
        }

        private void ImportSpares()
        {
            string FilePath = "";

            // вызов диалога выбора файла для импорта
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "Office 2003 XML Tables (.xml)|*.xml"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                FilePath = dlg.FileName;
                int count = count = Office2003XmlTable.getSpares(FilePath);

                // перенос списка полученных записе в БД
                MessageBox.Show("Импортировано " + count + " записей.");
                ShowUCSpares(true);
            }
        }

        private void ImportAnalogues()
        {
            string FilePath = "";

            //bool FailFlag = false;
            // вызов диалога выбора файла для импорта
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "Office 2003 XML Tables (.xml)|*.xml"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                FilePath = dlg.FileName;
                int count = 0;

                count = Office2003XmlTable.getAnalogues(FilePath);

                // перенос списка полученных записе в БД
                //if (!FailFlag)
                //{
                MessageBox.Show("Импортировано " + count + " записей.");

                //}
            }
        }

        private void ImportRemains()
        {
            // вызов диалога выбора файла для импорта
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "Office 2003 XML Tables (.xml)|*.xml"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string mess = Office2003XmlTable.getRemains(dlg.FileName);
                MessageBox.Show(mess);
            }
        }

        private void ItemSearch()
        {
            // получение параметров
            string SearchText = edtSearchText.Text;

            int SearcFieldIndex = edtSearchField.SelectedIndex;

            //bool StrongSearch = false;
            switch (_Workspace)
            {
                case 0: // номенклатура

                    //uc_Spares.SparesSearchByText(SearcFieldIndex, SearchText, StrongSearch);
                    uc_Spares._SearchTextAndIndex(SearchText, SearcFieldIndex);
                    break;

                case 1: // товародвижение
                    break;
            }
        }

        public void CallRevisionWindow()
        {
            RevisionEditView v = new RevisionEditView();
            v.ShowDialog();
        }

        #endregion CUSTOM FUNCTIONS

        // ACTION HANDLERS
        // КОНСТРУКТОР MainWindow()
        public MainWindow()
        {
            //DateTime time1 = DateTime.Now;
            //DateTime t = DateTime.Now;

            // InitializeComponent() - auto-generated function
            InitializeComponent();
            /* В конструкторе класса главного окна инициализируем большой список,
             * подгружать остальную инфу будем при показе окна */
            //t = DateTime.Now; 
            SpareContainer.Instance.Update(); 
            //Log((DateTime.Now - t).TotalSeconds + " secs SpareContainer.Instance.Update()");

            //Start();
            /* + окно загружено,
             * + инициализируем user control'ы,
             * + загружаем и связываем дерево групп,
             * + связываем список деталей с гридом,
             * + отображаем суммарное количество деталей и деталей в остатках,
             * + связывем значения по умолчанию,
             * проверяем свежесть курсов валюты и загружаем свежие, если надо*/

            // v#2
            // ======== defs
            int? sfi = da.getProfileCurrent().DefSearchFieldIndex;
            edtSearchField.SelectedIndex = sfi.HasValue ? sfi.Value : 0;
            this.btnItemAdd.ToolTip = "Добавить новый товар в базу";
            this.btnItemDelete.ToolTip = "Редактировать выделенный товар";
            this.btnItemEdit.ToolTip = "Удалить из базы выделенный товар";

            LoadCurrencies();
            _Workspace = 0;
            // ======== user control
            string curr = da.getProfileCurrent().BasicCurrencyCode;
            uc_Spares = new Views.Main_window.UCSpares(this, curr); 
            mMainGrid.Children.Add(uc_Spares);

            // bind spares
            uc_Spares.dgSpares.DataContext = SpareContainer.Instance.Spares;
            // покажем контрол
            uc_Spares.Visibility = System.Windows.Visibility.Visible;
            PrintRemains();
            
            //TimeSpan time = DateTime.Now - time1;
            //string ts = time.TotalSeconds.ToString() + " seconds";
            //Log("MainWindow: " + ts);
        }

        private void mi_Contacts_Click(object sender, RoutedEventArgs e)
        {
            ContactsView v = new ContactsView();
            v.ShowDialog();
        }

        private void mi_Banks_Click(object sender, RoutedEventArgs e)
        {
            BanksView v = new BanksView();
            v.ShowDialog();
        }

        private void mi_AccountTypes_Click(object sender, RoutedEventArgs e)
        {
            AccountTypesView v = new AccountTypesView();
            v.ShowDialog();
        }

        private void mi_Accounts_Click(object sender, RoutedEventArgs e)
        {
            AccountsView v = new AccountsView();
            v.ShowDialog();
        }

        private void mi_Units_Click(object sender, RoutedEventArgs e)
        {
            UnitsView v = new UnitsView();
            v.ShowDialog();
        }

        private void mi_AdminUnits_Click(object sender, RoutedEventArgs e)
        {
            AdminUnitsView v = new AdminUnitsView();
            v.ShowDialog();
        }

        private void mi_Brands_Click(object sender, RoutedEventArgs e)
        {
            BrandsView v = new BrandsView();
            v.ShowDialog();
        }

        private void mi_CarMarks_Click(object sender, RoutedEventArgs e)
        {
            CarMarksView v = new CarMarksView();
            v.ShowDialog();
        }

        private void mi_CarProducers_Click(object sender, RoutedEventArgs e)
        {
            CarProducersView v = new CarProducersView();
            v.ShowDialog();
        }

        private void mi_Clients_Click(object sender, RoutedEventArgs e)
        {
            ClientsView v = new ClientsView();
            v.ShowDialog();
        }

        private void mi_Warehouses_Click(object sender, RoutedEventArgs e)
        {
            WarehousesView v = new WarehousesView();
            v.ShowDialog();
        }

        private void mi_VatRates_Click(object sender, RoutedEventArgs e)
        {
            VATRatesView v = new VATRatesView();
            v.ShowDialog();
        }

        private void mi_Currencies_Click(object sender, RoutedEventArgs e)
        {
            //  CurrenciesView v = new CurrenciesView();
            CurrenciesByDatesView v = new CurrenciesByDatesView();
            v.ShowDialog();
        }

        /*

        private void dgSpares_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedRowIndex = dgSpares.SelectedIndex;
            if (selectedRowIndex >= 0)
            {
                spare s = dgSpares.SelectedItem as spare;
                ReloadAnalogues(s.id);
                lblSpareName.Content = "Наименование: " + s.name;
                lblSpareBrand.Content = "Бренд: " + s.brand.name;
                lblSpareGroup.Content = "Группа: " + s.spare_group.name;
            }
        }

        */

        private void edtSearchText_MouseEnter(object sender, MouseEventArgs e)
        {
            if (edtSearchText.Text.Equals("Введите текст..."))
            {
                edtSearchText.Text = "";
                edtSearchText.FontStyle = FontStyles.Normal;
                edtSearchText.FontWeight = FontWeights.Bold;
                edtSearchText.Foreground = Brushes.Black;
            }
        }

        private void SpareSearch()
        {
            string searchString = edtSearchText.Text;
            int searchFieldIndex = edtSearchField.SelectedIndex;

            //ReloadSpares(searchFieldIndex, searchString);
            ClearSearchTextBox();
        }

        private void ClearSearchTextBox()
        {
            edtSearchText.Text = "Введите текст...";
            edtSearchText.FontStyle = FontStyles.Italic;
            edtSearchText.FontWeight = FontWeights.Light;
            edtSearchText.Foreground = Brushes.LightGray;
        }

        private void btnSpareSearch_Click(object sender, RoutedEventArgs e)
        {
            SpareSearch();
        }

        private void btnWorkspaceSpares_Click(object sender, RoutedEventArgs e)
        {
            ShowUCMovements(false);
            ShowUCSpares(true);
            uc_Spares._GroupID = 1;
        }

        private void btnWorkspaceMovements_Click(object sender, RoutedEventArgs e)
        {
            ShowUCSpares(false);
            ShowUCMovements(true);
        }

        private void ShowUCSpares(bool show)
        {
            if (uc_Spares == null)
            {
                uc_Spares = new Views.Main_window.UCSpares();
                uc_Spares.ParentWindow = this;
                mMainGrid.Children.Add(uc_Spares);
                uc_Spares.CurrentCurrencyName = edtCurrentCurrency.SelectedItem.ToString();

                //uc_Spares.LoadSpares();
                uc_Spares.LoadGroups(false);
            }
            uc_Spares.Visibility = show ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            if (show)
            {
                this.btnItemAdd.ToolTip = "Добавить новый товар в базу";
                this.btnItemDelete.ToolTip = "Редактировать выделенный товар";
                this.btnItemEdit.ToolTip = "Удалить из базы выделенный товар";
                btnItemAdd.IsEnabled = true;
                btnItemEdit.IsEnabled = true;
                btnItemDelete.IsEnabled = true;
            }
            _Workspace = 0;
            PrintRemains();
        }

        private void ShowUCMovements(bool show)
        {
            if (uc_Movements == null)
            {
                uc_Movements = new Views.Main_window.UCMovements();
                uc_Movements.ParentWindow = this;
                mMainGrid.Children.Add(uc_Movements);
            }
            uc_Movements.Visibility = show ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            uc_Movements.LoadSales();
            _Workspace = 1;
        }

        private void btnItemAdd_Click(object sender, RoutedEventArgs e)
        {
            ItemCreate();
            PrintRemains();
        }

        private void btnItemEdit_Click(object sender, RoutedEventArgs e)
        {
            ItemEdit();
            PrintRemains();
        }

        private void btnItemCopy_Click(object sender, RoutedEventArgs e)
        {
            ItemCopy();
        }

        private void btnItemDelete_Click(object sender, RoutedEventArgs e)
        {
            ItemDelete();
            PrintRemains();
        }

        private void mi_File_Import_Brands_Click(object sender, RoutedEventArgs e)
        {
            ImportBrands();
        }

        private void mi_File_Import_Spares_Click(object sender, RoutedEventArgs e)
        {
            ImportSpares();
        }

        private void mi_File_Import_Groups_Click(object sender, RoutedEventArgs e)
        {
            ImportGroups();
        }

        private void mi_File_Import_Analogues_Click(object sender, RoutedEventArgs e)
        {
            ImportAnalogues();
        }

        private void edtSearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            ItemSearch();
        }

        private void miAbout_About_Click(object sender, RoutedEventArgs e)
        {
            AboutBox v = new AboutBox();
            v.ShowDialog();
        }

        private void edtShowRests_Click(object sender, RoutedEventArgs e)
        {
            uc_Spares._RemainsOnly = edtShowRests.IsChecked.Value;
        }

        private void mi_Settings_RevisionOn_Click(object sender, RoutedEventArgs e)
        {
            CallRevisionWindow();
            /*RevisionModeOn = mi_Settings_RevisionOn.IsChecked;
            uc_Spares.RevisionMode(RevisionModeOn);
            if (RevisionModeOn)
                spRevisionPanel.Visibility = System.Windows.Visibility.Visible;
            else
                spRevisionPanel.Visibility = System.Windows.Visibility.Hidden;*/
        }

  

        private void edtSearchField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (uc_Spares != null)
                uc_Spares._SearchFieldIndex = edtSearchField.SelectedIndex;
        }

        private void edtSearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SmartSpareSearch();
        }

        private void edtSearchText_TextInput(object sender, TextCompositionEventArgs e)
        {
            //ItemSearch();
        }

        private void edtCurrentCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeBasicCurrency();
        }

        private void mi_Settings_Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsView v = new SettingsView();
            v.ShowDialog();
        }

        private void mi_Reports_Remains_Click(object sender, RoutedEventArgs e)
        {
            ReportFilterRemainsView v = new ReportFilterRemainsView();
            v.ShowDialog();
        }

        private void mi_Reports_Incomes_Click(object sender, RoutedEventArgs e)
        {
            ReportFilterIncomesView v = new ReportFilterIncomesView();
            v.ShowDialog();
        }

        private void mi_Reports_Outgoes_Click(object sender, RoutedEventArgs e)
        {
            ReportFilterOutgoesView v = new ReportFilterOutgoesView();
            v.ShowDialog();
        }

        private void mi_File_Import_Remains_Click(object sender, RoutedEventArgs e)
        {
            ImportRemains();
        }

        private void mi_Settings_AnaloguesFix_Click(object sender, RoutedEventArgs e)
        {
            DataAccess da = new DataAccess();
            MessageBox.Show(da.FixAnalogues());
        }

        private void mi_SimplePrint_Click(object sender, RoutedEventArgs e)
        {
            SimpleReport r = new SimpleReport();
            r.ShowDialog();
        }

        private void mi_Reports_Test_Click(object sender, RoutedEventArgs e)
        {
            //Reporter.GenerateTTNFromSpareIncomeId(25);
        }

        private void mi_File_FixBP_Click(object sender, RoutedEventArgs e)
        {
        }

        private void mi_File_Request_Click(object sender, RoutedEventArgs e)
        {
            RequestEditView v = new RequestEditView();
            v.ShowDialog();
        }

        private void btnRevision_Click(object sender, RoutedEventArgs e)
        {            
            Revision3EditView v = new Revision3EditView();            
            v.ShowDialog();
        }

        private string code = "";

        private void SmartSpareSearch()
        {
            if (uc_Spares.grBasket.Visibility != System.Windows.Visibility.Visible)
                return;
            string SearchCode = edtSearchText.Text;
            SpareView sv = null;
            edtSearchText.Clear();
            List<SpareView> tmp = SpareContainer.Instance.GetSparesStrict(0, SearchCode);
            if (tmp.Count > 0)
            {
                sv = tmp[0];
            }
            else
            {
                //edtSearchString.Content = "НЕ НАЙДЕНО!";
                if (MessageBox.Show("Товар не найден. Добавить сейчас новый?", "Добавление товара", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    sv = Marvin.Instance.SpareCreate();
                else
                    return;
            }
            if (sv != null)
            {
                uc_Spares.AddSpareToBasket(sv);
            }
            else
            {
                //edtSearchString.Content = "error";
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Delete)
                MessageBox.Show(message);
            /*
            if (uc_Spares.grBasket.Visibility == System.Windows.Visibility.Visible)
            {
                Key k = e.Key;
                if (k == Key.Enter)
                    SmartSpareSearch();
                else
                {
                    // digits
                    if (k >= Key.D0 && k <= Key.D9)
                    {
                        char c = k.ToString()[1];
                        code += c;
                    }
                    else
                        code += k;
                }
                lbSparesQ.Content = code;
            }
            else*/
            edtSearchText.Focus();
        }

        private void btnSearchClear_Click(object sender, RoutedEventArgs e)
        {
            if (edtSearchText.Text.Length > 0)
                edtSearchText.Text = "";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SettingsSave();
            Application.Current.Shutdown();
        }

        private void SettingsLoad()
        {
            //ShowUCSpares(Settings.Settings.Default.CurrentWorkspace == 0);
            //ShowUCMovements(Settings.Settings.Default.CurrentWorkspace != 0);
            //edtSearchText.Text = Settings.Settings.Default.SearchText;
            //edtSearchField.SelectedIndex = Settings.Settings.Default.SearchFieldIndex;
            //ItemSearch();
        }

        private void SettingsSave()
        {
            //Settings.Settings.Default.CurrentWorkspace = uc_Spares.IsVisible ? 0 : 1;
            //Settings.Settings.Default.SearchText = edtSearchText.Text;
            //Settings.Settings.Default.SearchFieldIndex = edtSearchField.SelectedIndex;
            //Settings.Settings.Default.Save();
        }

        public void PrintRemains()
        {
            lbSparesQ.Content = "Все наименования: "
                                + SpareContainer.Instance.Spares.Count.ToString("N", CultureInfo.CreateSpecificCulture("ru-RU"))
                                + ". ";
            lbSparesQ.Content += "Наименования с остатком: "
                                + SpareContainer.Instance.Remains.Count.ToString("N", CultureInfo.CreateSpecificCulture("ru-RU"))
                                + ". ";

            //lbSparesQ.Content += "Сумма остатков: " + SpareContainer.Instance.Remains.Sum(x => x.QRest) + ". ";
            lbSparesQ.Content += "Сумма остатков: "
                            + SpareContainer.Instance.RemainsSum().ToString("N", CultureInfo.CreateSpecificCulture("ru-RU"))
                            + " единиц базовой валюты.";

            // + "; остатков: " + SpareContainer.Instance.Remains.Count;
            //settings_profile p = da.getProfileCurrent();
            //if (p.UseScanner == 1)
            //    edtSearchText.SearchMode = UIControls.SearchMode.Delayed;
            //else
            //    edtSearchText.SearchMode = UIControls.SearchMode.Instant;
        }

        public string LastSearch = "";

        private void SearchTextBox_Search(object sender, RoutedEventArgs e)
        {
            ItemSearch2();
        }

        private void ItemSearch2()
        {
            // получение параметров
            string SearchText = edtSearchText.Text;
            if (SearchText == LastSearch)
                return;

            int SearcFieldIndex = edtSearchField.SelectedIndex;

            //bool StrongSearch = false;
            switch (_Workspace)
            {
                case 0: // номенклатура

                    //uc_Spares.SparesSearchByText(SearcFieldIndex, SearchText, StrongSearch);
                    uc_Spares._SearchTextAndIndex(SearchText, SearcFieldIndex);
                    LastSearch = SearchText;
                    break;

                case 1: // товародвижение
                    break;
            }
        }
        string message = "";
        void Log(string s)
        {
            message += DateTime.Now.ToString("hh:mm:ss.fff") + " > " + s + "\n";
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            settings_profile settings = da.getProfileCurrent();
            if (settings.DefaultUserID > 0)
            {
                Marvin.Instance.CurrentUser = da.AdminUnitGet(settings.DefaultUserID);
            }
            else
            {
                LoginView v1 = new LoginView();
                v1.ShowDialog();
                if (!v1.Res)
                {
                    App.Current.Shutdown();

                    //Close();
                    return;
                }
            }
            this.Title += " - " + Marvin.Instance.CurrentUser.name;
            // check currencies up-to-date
            if (!da.AreCurrenciesUpToDate())
            {
                CurrenciesInput v = new CurrenciesInput(DateTime.Now);
                v.ShowDialog();
             }            
        }

        private void mi_Reports_DailySales_Click(object sender, RoutedEventArgs e)
        {
            DateTime dt = DateTime.Now;

            // показать диалог выбора даты отчета
            SelectReportDataView v = new SelectReportDataView();
            bool? res = v.ShowDialog();
            if (v.ReportDate.HasValue && v.ReportDateTo.HasValue)
                Reporter.GenerateDailySalesReport(v.ReportDate.Value, v.ReportDateTo.Value, v.WarehouseID);
        }

        private void mi_Reports_SpareSalesByCode_Click(object sender, RoutedEventArgs e)
        {
            DateTime dt = DateTime.Now;

            // показать диалог выбора даты отчета
            SelectReportSalesByCodeDate v = new SelectReportSalesByCodeDate();
            bool? res = v.ShowDialog();
            if (v.SpareID > 0 && v.DateTo.HasValue && v.DateFrom.HasValue)
                Reporter.GenerateSpareSalesReportByPeriod(v.SpareID, v.DateFrom.Value, v.DateTo.Value, v.WarehouseID);
        }

        private void mi_Reports_RequestList_Click(object sender, RoutedEventArgs e)
        {
            Reporter.GenerateRequestReport();
        }

        private void btnBasket_Click(object sender, RoutedEventArgs e)
        {
            if (uc_Spares.grBasket.Visibility == System.Windows.Visibility.Collapsed)
            {
                uc_Spares.LoadBasket();
                uc_Spares.treeSpareGroups.Visibility = System.Windows.Visibility.Collapsed;
                uc_Spares.grBasket.Visibility = System.Windows.Visibility.Visible;
                DataAccess db = new DataAccess();
                int num = db.GetMaxSaleID();
                uc_Spares.edtBasketNumDate.Text = "№" + num + " " + DateTime.Now.ToShortDateString();
            }
            else
            {
                uc_Spares.grBasket.Visibility = System.Windows.Visibility.Collapsed;
                uc_Spares.treeSpareGroups.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void btnBasket_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            edtSearchText.Focus();
        }

        private void LogOut()
        {
            MessageBoxResult res = MessageBox.Show("Программа будет закрыта. Вы уверены?", "Выход", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                settings_profile settings = da.getProfileCurrent();
                settings.DefaultUserID = 0;
                da.ProfileEdit(settings);
                App.Current.Shutdown();
            }
        }

        private void miLogout_Click(object sender, RoutedEventArgs e)
        {
            LogOut();
        }
    }
}