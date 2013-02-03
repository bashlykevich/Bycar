using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using bycar;
using bycar3.Core;
using bycar3.External_Code;
using bycar3.Views.Spare_Outgo;
using System.Collections;
using System.ComponentModel;
using System.Windows.Data;

namespace bycar3.Views.Main_window
{
    /// <summary>
    /// Interaction logic for UCSpares.xaml
    /// </summary>
    public partial class UCSpares : UserControl
    {
        #region MEMBERS

        private DataAccess da = new DataAccess();
        private IList items;
        private string currentCurrencyName = "";

        //XXXList<SpareView> SPARES = null;
        public MainWindow ParentWindow = null;

        private List<BasketView> BasketItems = new List<BasketView>();
        private int SelectedSpareID = 0;

        public string CurrentCurrencyName
        {
            get { return currentCurrencyName; }
            set
            {
                currentCurrencyName = value;

                //dgIncomes.Columns[3].Header = "Цена (" + currentCurrencyName + ")";
                //CurrentRate = da.getLastCurrencyRateByCode(currentCurrencyName);
                if (dgSpares.SelectedItem != null)
                {
                    int SpareID = ((SpareView)dgSpares.SelectedItem).id;
                    LoadIncomes(SpareID);
                }
            }
        }

        public bool RevisionModeOn = false;

        private int _searchFieldIndex = 0;

        public int _SearchFieldIndex
        {
            get { return _searchFieldIndex; }
            set
            {
                _searchFieldIndex = value;
                LoadSpares();
            }
        }

        private string _searchText = "";

        public void _SearchTextAndIndex(string SText, int SIndex)
        {
            _searchFieldIndex = SIndex;
            _SearchText = SText;
        }

        public string _SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                LoadSpares();
            }
        }

        private bool _remainsOnly = false;

        public bool _RemainsOnly
        {
            get { return _remainsOnly; }
            set
            {
                _remainsOnly = value;
                LoadSpares();
            }
        }

        private int _groupID = 0; // all spares

        public int _GroupID
        {
            get { return _groupID; }
            set
            {
                _groupID = value;
                LoadSpares();
            }
        }

        private string _brandName = "";

        public string _BrandName
        {
            get { return _brandName; }
            set
            {
                _brandName = value;
                LoadSpares();
            }
        }

        private void _GroupIDBrandName(int GroupID, string BrandName)
        {
            _groupID = GroupID;
            _brandName = BrandName;
            LoadSpares();
        }

        #endregion MEMBERS

        #region CUSTOM FUNCTIONS

        // ======= ГРУППЫ =========================================
        // загрузить список групп в дерево

        public void LoadGroups(bool expand)
        {
            LoadGroups();
        }

        // создать группу
        private void GroupCreate()
        {
            SpareGroupEditView v = new SpareGroupEditView();
            if (treeSpareGroups.SelectedItem != null)
            {
                TreeViewItem tvi = (TreeViewItem)treeSpareGroups.SelectedItem;
                v._parentName = tvi.Header.ToString();
            }
            else
            {
                v._parentName = null;
            }
            v.ShowDialog();
        }

        // редактировать группу
        private void GroupEdit()
        {
            SpareGroupEditView v = new SpareGroupEditView();
            if (treeSpareGroups.SelectedItem != null)
            {
                TreeViewItem tvi = (TreeViewItem)treeSpareGroups.SelectedItem;
                string name = tvi.Header.ToString();
                spare_group g = da.GetSpareGroups().FirstOrDefault(sg => sg.name == name);
                v._groupName = name;
                v.edtName.Text = name;
                v.edtDescr.Text = g.description;
                v.ShowDialog();
                LoadGroups(true);
            }
        }

        // удалить группу
        private void GroupDelete()
        {
            if (treeSpareGroups.SelectedItem != null)
            {
                TreeViewItem tvi = (TreeViewItem)treeSpareGroups.SelectedItem;
                string name = tvi.Header.ToString();
                spare_group g = da.GetSpareGroups().FirstOrDefault(sg => sg.name == name);
                MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выделенную группу и все её подгруппы?", "Удаление группы", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    da.SpareGroupDelete(g);
                    LoadGroups(true);
                }
            }
        }

        // построение дерева
        private List<spare_group> NotRootGroups = null;

        private void BuildTreeBrunch(TreeViewItem root, int parent_id, bool expand)
        {
            if (NotRootGroups == null)
                NotRootGroups = da.GetSpareGroups().Where(g => g.ParentGroup != null).ToList();
            List<spare_group> list = NotRootGroups.Where(g => g.ParentGroup.id == parent_id).OrderBy(g => g.name).ToList();
            foreach (spare_group g in list)
            {
                TreeViewItem tvi = new TreeViewItem();
                tvi.Header = g.name;
                if (g.IsBrand)
                    tvi.Name = "BrandTVIId" + g.id;
                else
                    tvi.Name = "TVIId" + g.id;
                BuildTreeBrunch(tvi, g.id, expand);
                tvi.IsExpanded = expand;
                root.Items.Add(tvi);
            }
        }

        /*
        public List<string> getBrandsInSpareGroup(string groupName)
        {
            List<string> items = new List<string>();

            //List<SpareView> spares = getSparesByGroupName(groupName);
            List<SpareView> spares = SpareContainer.Instance.Spares.Where(x => x.GroupName == groupName).ToList();
            List<brand> brands = da.GetBrands();
            foreach (brand i in brands)
            {
                if (spares.Where(x => x.BrandID == i.id).Count()>0)
                {
                    items.Add(i.name);
                }
            }
            items.Sort();
            return items;

            return new List<string>();
        }*/

        // выбрана другая группа

        private void GroupsTreeViewSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            spare_group group = e.NewValue as spare_group;
            DataAccess da = new DataAccess();

            // Если выбрана не группа, а брэнд, отобразить список групп с двумя наложенными фильтрами:
            //  1. Выбранный брэнд
            //  2. Родительская для выранного брэнда группа
            if (group.IsBrand)
            {
                string brandName = group.name;
                int groupID = group.ParentGroup.id;
                _GroupIDBrandName(groupID, brandName);

            }
            else
            {
                int GroupID = group.id;
                _GroupIDBrandName(GroupID, "");
            }

            dgSpares.SelectedIndex = 0;
        }

        private void CreateSpareMovementOut()
        {
            // вызвать диалог добавление прихода
            SpareOutgoEditView v = new SpareOutgoEditView();
            v._id = -1;
            v.ShowDialog();
        }

        private bool CreateSpareInSpareOutgo()
        {
            if (dgIncomes.SelectedItem != null)
            {
                SpareInOutgoEditView v = new SpareInOutgoEditView();
                v._SpareName = (dgIncomes.SelectedItem as SpareInSpareIncomeView).SpareName;
                v._SpareInSpareIncomeID = (dgIncomes.SelectedItem as SpareInSpareIncomeView).id;
                v._AvailableQuantity = (int)(dgIncomes.SelectedItem as SpareInSpareIncomeView).QRest.Value;
                da = new DataAccess();

                // получаем ID текущей открытой накладной
                spare_outgo CurrentOutgo = da.SpareOutgoOpened();
                if (CurrentOutgo == null)
                {
                    MessageBox.Show("Не указана текущая открытая накладная!");
                    return false;
                }
                v._SpareOutgoID = CurrentOutgo.id;
                CurrentOutgo.currencyReference.Load();
                v.CurrentCurrencyCode = CurrentOutgo.currency.code;
                decimal BasicPrice = (dgIncomes.SelectedItem as SpareInSpareIncomeView).POutBasic.Value;
                decimal Price = CurrencyHelper.GetPrice(v.CurrentCurrencyCode, BasicPrice);
                v._Price = Price;
                v.ShowDialog();
            }
            return true;
        }

        private int GotoMode = 0;

        private void GoToSpare()
        {
            if (dgAnalogues.SelectedItem == null)
                return;
            SpareView SpareViewToGo = (dgAnalogues.SelectedItem as SpareView);
            int SpareID = SpareViewToGo.id;
            ParentWindow.LastSearch = "";
            if (ParentWindow.edtSearchText.Text != "")
                ParentWindow.edtSearchText.Text = "";

            //ParentWindow.edtSearchText.
            _searchText = "";
            _brandName = "";
            _groupID = 0;
            _remainsOnly = false;
            if (ParentWindow.edtShowRests.IsChecked.HasValue)
                if (ParentWindow.edtShowRests.IsChecked.Value)
                    ParentWindow.edtShowRests.IsChecked = false;
            LoadSpares();
            SpareView s = SpareContainer.Instance.Spares.FirstOrDefault(x => x.id == SpareID);
            dgSpares.SelectedItem = s;
            dgSpares.ScrollIntoView(s);
        }

        // ======= ЗАПЧАСТИ =========================================
        // загрузить список запчастей с сервера

        public void LoadSpares()
        {
            LoadSparesInBackground();
        }

        public List<SpareView> FilterSpares(
           int SearchFieldIndex,
           string SearchText,
           bool RemainsOnly,
           int GroupID,
           string BrandName)
        {
            List<SpareView> ResultList = new List<SpareView>();
            int BrandID = 0;
            if (BrandName.Length > 0)
                BrandID = da.GetBrandIDByName(BrandName);

            if (GroupID == 0)
            {
                ResultList = SpareContainer.Instance.Spares;
            }
            else
                if (GroupID > 0 && (!(BrandID > 0)))
                {
                    if (GroupID > 0)
                    {
                        List<SpareView> tmps = SpareContainer.Instance.Spares.ToList();
                        List<SpareView> tmps1 = tmps.Where(x => x != null).ToList();
                        ResultList = tmps1.Where(x => (x.GroupID == GroupID)
                            || (x.spare_group1_id == GroupID)
                            || (x.spare_group2_id == GroupID)
                            || (x.spare_group3_id == GroupID)).ToList();
                    }
                }
                else
                    if (BrandID > 0)
                    {
                        if (GroupID > 0)
                        {
                            List<SpareView> tmps = SpareContainer.Instance.Spares.ToList();
                            List<SpareView> tmps1 = tmps.Where(x => x != null).ToList();
                            ResultList = tmps1.Where(x => ((x.GroupID == GroupID)
                            || (x.spare_group1_id == GroupID)
                            || (x.spare_group2_id == GroupID)
                            || (x.spare_group3_id == GroupID))
                            && (x.BrandID == BrandID)).ToList();
                        }
                    }
            //bool RemainsOnly,
            if (RemainsOnly)
            {
                ResultList = ResultList.Where(i => i.QRest > 0).ToList();
            }

            //int SearchFieldIndex,
            //string SearchText,
            if (SearchText.Length > 0)
            {
                // разобрать в завимосости от выбранного для поиска поле
                switch (SearchFieldIndex)
                {
                    case 0:// ПОИСК ПО КОДУ
                        ResultList = ResultList.Where(s => s.code != null).Where(s => s.code.ToLower().Contains(SearchText.ToLower())).ToList();
                        break;

                    case 1:// ПОИСК ПО НАИМЕНОВАНИЮ
                        ResultList = ResultList.Where(s => s.name.ToLower().Contains(SearchText.ToLower())).ToList();
                        break;

                    case 2:// ПОИСК ПО КОДУ ШАТЕ-М
                        ResultList = ResultList.Where(s => s.codeShatem != null).Where(s => s.codeShatem.ToLower().Contains(SearchText.ToLower())).ToList();
                        break;
                }
            }

            return ResultList.ToList();
        }
        BackgroundWorker BackgroundLoad;
        void LoadSparesInBackground()
        {
            ParentWindow.edtStatus.Content = "загрузка...";
            SelectedSpareID = 0;
            if (dgSpares.SelectedItem != null)
                SelectedSpareID = (dgSpares.SelectedItem as SpareView).id;

            BackgroundLoad = new BackgroundWorker();
            BackgroundLoad.DoWork += new DoWorkEventHandler(BackgroundLoad_DoWork);
            BackgroundLoad.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundLoad_RunWorkerCompleted);
            if (BackgroundLoad.IsBusy)
                BackgroundLoad.CancelAsync();
            BackgroundLoad.RunWorkerAsync();

        }
        private void BackgroundLoad_DoWork(object sender, DoWorkEventArgs e)
        {
            items = FilterSpares(_searchFieldIndex, _searchText, _remainsOnly, _groupID, _brandName);
        }
        private void BackgroundLoad_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            dgSpares.DataContext = items;
            if (dgSpares.HasItems && GotoMode != 4)
            {
                SpareView s = dgSpares.Items[0] as SpareView;
                dgSpares.SelectedItem = s;
                dgSpares.UpdateLayout();
                dgSpares.ScrollIntoView(s);
            }
            GotoMode = 0;
            ParentWindow.edtStatus.Content = "ok";
        }

        BackgroundWorker BackgroundShowDetails;
        void ShowSpareDetailsInBackground(IList selectedItems)
        {
            ParentWindow.edtStatus.Content = "загрузка";
            BackgroundShowDetails = new BackgroundWorker();
            BackgroundShowDetails.DoWork += new DoWorkEventHandler(BackgroundShowDetails_DoWork);
            BackgroundShowDetails.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundShowDetails_RunWorkerCompleted);
            if (BackgroundShowDetails.IsBusy)
                BackgroundShowDetails.CancelAsync();
            BackgroundShowDetails.RunWorkerAsync(selectedItems);

        }
        private void BackgroundShowDetails_DoWork(object sender, DoWorkEventArgs e)
        {
            // DETAILED INFO
            IList selectedItems = e.Argument as IList;
            if (selectedItems.Count == 0)
            {
                DetailSpareInfo_Brand = "";
                DetailSpareInfo_Group = "";
                DetailSpareInfo_Name = "";
                Detail_Incomes = null;
                Detail_Analogues = null;
            }
            else
            {
                SpareView selected = selectedItems[0] as SpareView;
                DetailSpareInfo_Brand = selected.BrandName;
                DetailSpareInfo_Name = selected.name;
                // построим путь по группам к запчасти
                // текущая группа
                DataAccess db = new DataAccess();
                spare s = db.GetSpare(selected.id);
                string gp = s.spare_group.name;
                // родительская
                if (s.spare_group1 != null)
                    gp = s.spare_group1.name + "/" + gp;
                // дедушка
                if (s.spare_group2 != null)
                    gp = s.spare_group2.name + "/" + gp;
                // прадедушка
                if (s.spare_group3 != null)
                    gp = s.spare_group3.name + "/" + gp;
                DetailSpareInfo_Group = gp;

                // INCOMES & ANALOGS WINDOWS
                // anlogues                
                Detail_Analogues = db.GetAnalogues(selected.id);
                
                // incomes   
                Detail_Incomes = db.GetIncomes(selected.id);
                foreach (SpareInSpareIncomeView i in Detail_Incomes)
                {
                    decimal POutBasic = 0;
                    decimal PInBasic = 0;
                    if (!i.POutBasic.HasValue)
                    {
                        string IncomeCurrencyCode = i.CurrencyCode;

                        decimal PIn = i.PIn.Value;
                        POutBasic = CurrencyHelper.GetBasicPrice(IncomeCurrencyCode, PIn);
                    }
                    else
                    {
                        POutBasic = i.POutBasic.Value;
                    }
                    if (i.PInBasic.HasValue)
                    {
                        PInBasic = i.PInBasic.Value;
                    }
                    else
                    {
                        string IncomeCurrencyCode = i.CurrencyCode;
                        decimal PIn = i.PIn.Value;
                        PInBasic = CurrencyHelper.GetBasicPrice(IncomeCurrencyCode, PIn);
                    }
                    i.DF_PriceInCurrency = CurrencyHelper.GetPrice(currentCurrencyName, POutBasic);
                    i.DF_PriceInCurrencyIn = CurrencyHelper.GetPrice(currentCurrencyName, PInBasic);

                    string strDate = "";
                    if (i.SpareIncomeDate.Value.Day < 10)
                        strDate += "0";
                    strDate += i.SpareIncomeDate.Value.Day + ".";
                    if (i.SpareIncomeDate.Value.Month < 10)
                        strDate += "0";
                    strDate += i.SpareIncomeDate.Value.Month + ".";
                    strDate += i.SpareIncomeDate.Value.Year;
                    i.DF_Date = strDate;
                }                
            }
        }
        string DetailSpareInfo_Brand;
        string DetailSpareInfo_Group;
        string DetailSpareInfo_Name;
        IList Detail_Incomes;
        IList Detail_Analogues;
        private void BackgroundShowDetails_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            edtSpareBrand.Text = DetailSpareInfo_Brand;
            edtSpareGroup.Text = DetailSpareInfo_Group;
            edtSpareName.Text = DetailSpareInfo_Name;

            dgAnalogues.DataContext = Detail_Analogues;
            dgIncomes.DataContext = Detail_Incomes;

            ParentWindow.edtStatus.Content = "ok";
        }

        private void ScrollToFirstSpare()
        {
            if (dgSpares.Items.Count > 0)
            {
                SpareView s = dgSpares.Items[0] as SpareView;
                dgSpares.SelectedItem = s;
                dgSpares.UpdateLayout();
                dgSpares.ScrollIntoView(s);
            }
        }

        // создать товар
        public void SpareCreate()
        {
            Marvin.Instance.SpareCreate();
        }

        // редактировать товар
        public void SpareEdit()
        {
            if (dgSpares.SelectedItem == null)
                return;
            int SpareID = (dgSpares.SelectedItem as SpareView).id;
            Marvin.Instance.SpareEdit(SpareID);
        }

        // удалить товар
        public void SpareDelete()
        {
            if (dgSpares.SelectedItems.Count > 0)
            {
                MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выделенные записи?", "Удаление", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    foreach (SpareView item in dgSpares.SelectedItems)
                    {
                        Marvin.Instance.SpareDelete(item);
                    }
                    LoadSpares();
                    LoadGroups(false);
                }
            }
        }

        private bool DeleteItemByName(TreeViewItem root, string ItemHeader, string ParentItemName)
        {
            if (root.Name == ParentItemName)
            {
                foreach (TreeViewItem item in root.Items)
                {
                    if ((string)item.Header == ItemHeader)
                    {
                        root.Items.Remove(item);
                        return true;
                    }
                }
            }
            foreach (TreeViewItem item in root.Items)
            {
                if (item.HasItems)
                {
                    bool res = DeleteItemByName(item, ItemHeader, ParentItemName);
                    if (res)
                        return true;
                }
            }
            return false;
        }

        // копировать товар
        public void SpareCopy()
        {
        }

        // РАЗНОЕ
        // поиск по заданной строке (возможен строгий поиск)
        public void SparesSearchByText(int searchFieldIndex, string text, bool StrongSearch)
        {
            _SearchFieldIndex = searchFieldIndex;
            _SearchText = text;
        }

        private void LoadIncomes()
        {
            if (dgSpares.SelectedItem != null)
            {
                LoadIncomes((dgSpares.SelectedItem as SpareView).id);
            }
            else
            {
                dgIncomes.DataContext = null;
            }
        }

        private void LoadIncomes(int SpareID)
        {
            da = new DataAccess();
            List<SpareInSpareIncomeView> items = da.GetIncomes(SpareID);
            foreach (SpareInSpareIncomeView i in items)
            {
                decimal POutBasic = 0;
                decimal PInBasic = 0;
                if (!i.POutBasic.HasValue)
                {
                    string IncomeCurrencyCode = i.CurrencyCode;

                    decimal PIn = i.PIn.Value;
                    POutBasic = CurrencyHelper.GetBasicPrice(IncomeCurrencyCode, PIn);
                }
                else
                {
                    POutBasic = i.POutBasic.Value;
                }
                if (i.PInBasic.HasValue)
                {
                    PInBasic = i.PInBasic.Value;
                }
                else
                {
                    string IncomeCurrencyCode = i.CurrencyCode;
                    decimal PIn = i.PIn.Value;
                    PInBasic = CurrencyHelper.GetBasicPrice(IncomeCurrencyCode, PIn);
                }
                i.DF_PriceInCurrency = CurrencyHelper.GetPrice(currentCurrencyName, POutBasic);
                i.DF_PriceInCurrencyIn = CurrencyHelper.GetPrice(currentCurrencyName, PInBasic);

                string strDate = "";
                if (i.SpareIncomeDate.Value.Day < 10)
                    strDate += "0";
                strDate += i.SpareIncomeDate.Value.Day + ".";
                if (i.SpareIncomeDate.Value.Month < 10)
                    strDate += "0";
                strDate += i.SpareIncomeDate.Value.Month + ".";
                strDate += i.SpareIncomeDate.Value.Year;
                i.DF_Date = strDate;
            }
            dgIncomes.DataContext = items;
        }

        private void LoadHistory(int SpareID)
        {
            lvHistory.Items.Clear();
            da = new DataAccess();
            List<SpareInSpareIncomeView> items = da.GetIncomes(SpareID);
            items = (from i in items orderby i.SpareIncomeDate.Value descending select i).Take(10).ToList();
            foreach (SpareInSpareIncomeView i in items)
            {
                string hi = "";
                string d = i.SpareIncomeDate.Value.ToShortDateString();
                string an = i.AccountName;

                decimal POutBasic = 0;
                if (!i.POutBasic.HasValue)
                {
                    string IncomeCurrencyCode = i.CurrencyCode;
                    decimal PIn = i.PIn.Value;
                    POutBasic = CurrencyHelper.GetBasicPrice(IncomeCurrencyCode, PIn);
                }
                else
                {
                    POutBasic = i.POutBasic.Value;
                }
                i.DF_PriceInCurrency = CurrencyHelper.GetPrice(currentCurrencyName, POutBasic);
                string p = i.DF_PriceInCurrency.ToString();
                string cc = i.CurrencyCode;
                if (an == null)
                    an = "НЕ УКАЗАН";
                hi = d + " от контрагента [" + an + "] поступило " + i.QIn.ToString() + " единиц товара по цене " + p + cc + ".";
                lvHistory.Items.Add(hi);
            }
        }

        public void RevisionMode(bool state)
        {
            if (state)
            {
                // отобразить кнопки плюса-минуса
            }
            else
            {
                // спрятать спец.кнопки
            }
        }

        #endregion CUSTOM FUNCTIONS

        // HANDLERS
        public UCSpares()
        {
            InitializeComponent();

            // marvin init
            Marvin.Instance.MainWindowObj = this;
        }

        public UCSpares(MainWindow parent, string CurrencyName)
        {
            InitializeComponent();

            // marvin init
            Marvin.Instance.MainWindowObj = this;

            ParentWindow = parent;
            CurrentCurrencyName = CurrencyName;
            LoadGroups();

        }

        private void treeSpareGroups_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            GroupsTreeViewSelectionChanged(sender, e);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void tree_cm_Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadGroups(false);
        }

        private void tree_cm_ExpandAll_Click(object sender, RoutedEventArgs e)
        {
            LoadGroups(true);
        }

        private void tree_cm_UnexpandAll_Click(object sender, RoutedEventArgs e)
        {
            LoadGroups(false);
        }

        private void tree_cm_Create_Click(object sender, RoutedEventArgs e)
        {
            GroupCreate();
            LoadGroups(false);
        }

        private void tree_cm_Edit_Click(object sender, RoutedEventArgs e)
        {
            GroupEdit();
            LoadGroups(false);
        }

        private void tree_cm_Delete_Click(object sender, RoutedEventArgs e)
        {
            GroupDelete();
            LoadGroups(false);
        }

        private void dgSpares_cm_Add_Click(object sender, RoutedEventArgs e)
        {
            SpareCreate();
        }

        private void dgSpares_cm_Edit_Click(object sender, RoutedEventArgs e)
        {
            SpareEdit();
        }

        private void dgSpares_cm_Delete_Click(object sender, RoutedEventArgs e)
        {
            SpareDelete();
        }

        public void AddSpareToBasket(SpareView sv)
        {
            if (sv.QRest <= 0)
                return;
            int SpareID = sv.id;

            // если такой товар в корзине есть
            if (BasketItems.Where(x => x.SpareID == SpareID).Count() > 0)
            {
                if ((decimal)sv.QRest.Value >
                                                    BasketItems.FirstOrDefault(x => x.SpareID == SpareID).Q)
                    BasketItems.FirstOrDefault(x => x.SpareID == SpareID).Q++;
            }
            else
            {
                SpareInSpareIncomeView income = da.GetLastIncome(SpareID);
                AddIncomeToBasket(income);
            }

            // обновим грид
            LoadBasket();
        }

        private void dgSpares_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // если режим корзины
            if (grBasket.Visibility == System.Windows.Visibility.Visible)
            {
                if (dgSpares.SelectedItem == null)
                    return;
                else
                    AddSpareToBasket(dgSpares.SelectedItem as SpareView);
            }
            else
            {
                SpareEdit();
            }
        }

        public void LoadBasket()
        {
            dgBasket.DataContext = null;
            dgBasket.DataContext = BasketItems;
            decimal Pbyr = 0;
            decimal Peur = 0;
            decimal Pusd = 0;

            foreach (BasketView i in BasketItems)
            {
                Pbyr += i.Pbyr.Value * i.Q.Value;
                Peur += i.Peur.Value * i.Q.Value;
                Pusd += i.Pusd.Value * i.Q.Value;
            }

            //edtPbyr.Text = string.Format("{0:n0}", Pbyr) + " Br";
            edtPbyr.Text = Pbyr.ToString("### ### ### ###") + " Br";
            edtPeur.Text = Peur.ToString() + " €";
            edtPusd.Text = Pusd.ToString() + " $";
            edtBasketQ.Text = "Количество: " + BasketItems.Count + " объектов";

            //edtBasketNumDate.Text = DateTime.Now.ToShortDateString();
        }

        private void AddIncomeToBasket(SpareInSpareIncomeView income)
        {
            int SpareID = income.SpareID.Value;

            // если такой товар в корзине есть
            if (BasketItems.Where(x => x.SpareID == SpareID).Count() > 0)
            {
                if ((decimal)SpareContainer.Instance.Remains.FirstOrDefault(i => i.id == SpareID).QRest.Value >
                                                    BasketItems.FirstOrDefault(x => x.SpareID == SpareID).Q)
                    BasketItems.FirstOrDefault(x => x.SpareID == SpareID).Q++;
            }
            else
            {
                // если такого товара в корзине нет
                BasketView item = new BasketView();
                item.ID = BasketItems.Count + 1;

                // [CreatedOn]
                item.CreatedOn = DateTime.Now;

                // [SpareID]
                item.SpareID = SpareID;

                // [Q]
                item.Q = 1;

                // [OfferingID] = income
                //item.OfferingInID = (dgIncomes.SelectedItem as SpareInSpareIncomeView).id;
                // [OfferingID] = outgo
                //TODO item.OfferingOutID
                decimal POutBasic = income.POutBasic.Value;
                DataAccess db = new DataAccess();

                spare_outgo outgo = db.SpareOutgoOpened();
                if (outgo == null)
                {
                    MessageBox.Show("Сначала укажите текущую открытую накладную!");
                    return;
                }
                if (outgo.currency == null)
                    outgo.currencyReference.Load();
                if (outgo.currency.code == CurrencyHelper.BasicCurrencyCode)
                    POutBasic = income.POut.Value;

                // [Pusd]
                item.Pusd = CurrencyHelper.GetPrice("USD", POutBasic);

                // [Peur]
                item.Peur = CurrencyHelper.GetPrice("EUR", POutBasic);

                // [Pbyr]
                item.Pbyr = CurrencyHelper.GetPrice("BYR", POutBasic);

                // [Prur]
                item.Prur = CurrencyHelper.GetPrice("RUR", POutBasic);

                // [SpareName]
                item.SpareName = income.SpareName;

                // [UnitID]
                item.UnitID = 1;

                // [SpareCode]
                item.SpareCode = income.SpareCode;

                // [SpareCodeShatem]
                item.SpareCodeShatem = income.SpareCodeShatem;
                BasketItems.Add(item);
            }

            // обновим грид
            LoadBasket();
        }

        private void dgSpares_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowSpareDetailsInBackground(e.AddedItems);
        }

        private void dgIncomes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (grBasket.Visibility == System.Windows.Visibility.Visible)
            {
                if (dgIncomes.SelectedItem == null)
                    return;
                AddIncomeToBasket(dgIncomes.SelectedItem as SpareInSpareIncomeView);
            }
            else
            {
                CreateSpareInSpareOutgo();
                LoadSpares();
                LoadIncomes(SelectedSpareID);
            }
        }

        private void mi_dgIncomes_AddToCurrent_Click(object sender, RoutedEventArgs e)
        {
            CreateSpareInSpareOutgo();
            LoadSpares();
            LoadIncomes(SelectedSpareID);
        }

        private void mi_dgIncomes_AddToNew_Click(object sender, RoutedEventArgs e)
        {
            CreateSpareMovementOut();
            LoadSpares();
            LoadIncomes(SelectedSpareID);
        }

        private void tabSpares_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadIncomes();
        }

        private void mi_dgAnalogues_GoTo_Click(object sender, RoutedEventArgs e)
        {
            GoToSpare();
        }

        private void mi_dgAnalogues_GoTo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            GoToSpare();
        }

        private void dgAnalogues_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            GoToSpare();
        }

        private void btnBasketCancel_Click(object sender, RoutedEventArgs e)
        {
            grBasket.Visibility = System.Windows.Visibility.Collapsed;
            treeSpareGroups.Visibility = System.Windows.Visibility.Visible;
            BasketItems = new List<BasketView>();
        }

        private void btnBasketSell_Click(object sender, RoutedEventArgs e)
        {
            SaveBasket();
        }

        private void dgBasket_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                MessageBoxResult mbr = MessageBox.Show("Удалить из корзины выделенное?", "Корзина", MessageBoxButton.YesNo);
                if (mbr == MessageBoxResult.Yes)
                {
                    foreach (BasketView bv in dgBasket.SelectedItems)
                    {
                        BasketItems.Remove(bv);
                    }
                    LoadBasket();
                }
            }
        }

        private void SaveBasket()
        {
            DataAccess db = new DataAccess();

            // получить ID накладной, в которую будет включена корзина
            spare_outgo outgo = db.SpareOutgoOpened();

            // - если есть открытая накладная
            // - если нет открытой накладной
            if (outgo == null)
            {
                MessageBox.Show("Не указана текущая открытая накладная!");
                return;
            }
            int OutgoID = outgo.id;

            // создадим Sale
            // посчитаем сумму корзины в разных валютах
            decimal Prur = 0;
            decimal Pbyr = 0;
            decimal Peur = 0;
            decimal Pusd = 0;
            foreach (BasketView item in BasketItems)
            {
                Prur += item.Prur.Value * item.Q.Value;
                Pbyr += item.Pbyr.Value * item.Q.Value;
                Pusd += item.Pusd.Value * item.Q.Value;
                Peur += item.Peur.Value * item.Q.Value;
            }

            // сохранить запись о корзине
            Sale sale = new Sale();
            sale.Pbyr = Pbyr;
            sale.Prur = Prur;
            sale.Pusd = Pusd;
            sale.Peur = Peur;
            Sale s = db.SaleCreate(sale);

            // CreateBasketItem
            foreach (BasketView item in BasketItems)
            {
                // здесь же внутри
                // - уменшается остаток по приходу
                // - идёт связывание с отгрузками
                item.SaleID = s.ID;
                db.BasketCreate(item);

                // обновляем остаток товара в кэше
                SpareContainer.Instance.Update(item.SpareID.Value);
            }

            // Закрыть корзину, очистить список
            grBasket.Visibility = System.Windows.Visibility.Collapsed;
            treeSpareGroups.Visibility = System.Windows.Visibility.Visible;
            BasketItems = new List<BasketView>();
        }
        void LoadGroups()
        {
            DataAccess db = new DataAccess();
            treeSpareGroups.ItemsSource = da.GetRoots();
        }       
    }
}