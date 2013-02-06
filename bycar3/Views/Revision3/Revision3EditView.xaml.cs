using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using bycar;
using bycar3.External_Code;
using bycar3.Helpers.ValueConversion;
using bycar3.Reporting;

namespace bycar3.Views.Revision3
{
    /// <summary>
    /// Interaction logic for Revision3EditView.xaml
    /// </summary>
    public partial class Revision3EditView : Window
    {
        #region MEMBERS

        //private int SelectedSpareID = 0;
        private IList items;

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

        private bool _mismatchOnly = false;

        public bool _MismatchOnly
        {
            get { return _mismatchOnly; }
            set
            {
                _mismatchOnly = value;
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

        private int _brandID = 0;

        public int _BrandID
        {
            get { return _brandID; }
            set
            {
                _brandID = value;
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

        private DataAccess da = new DataAccess();

        private BackgroundWorker BackgroundLoad;

        private void LoadSparesInBackground()
        {
            edtStatus.Content = "загрузка...";

            Binding RestsBinding = new Binding("QRests");
            RestsBinding.Converter = new WarehouseToColumnConverter();
            RestsBinding.ConverterParameter = edtWarehouse.SelectedIndex;
            dgtcRests.Binding = RestsBinding;

            BackgroundLoad = new BackgroundWorker();
            BackgroundLoad.DoWork += new DoWorkEventHandler(BackgroundLoad_DoWork);
            BackgroundLoad.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundLoad_RunWorkerCompleted);
            if (BackgroundLoad.IsBusy)
                BackgroundLoad.CancelAsync();
            BackgroundLoad.RunWorkerAsync();
        }

        private void BackgroundLoad_DoWork(object sender, DoWorkEventArgs e)
        {
            items = FilterSpares(_SearchFieldIndex, _SearchText, _RemainsOnly, _GroupID, _BrandName, _mismatchOnly);
        }

        private void BackgroundLoad_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            dgSpares.DataContext = items;
            edtStatus.Content = "ok";
        }

        public void LoadSpares()
        {
            if (IsReady)
                LoadSparesInBackground();
        }

        public List<string> getBrandsInSpareGroup(int GroupID)
        {
            List<string> items = new List<string>();

            //List<SpareView> spares = getSparesByGroupName(groupName);
            List<SpareView> spares = SpareContainer.Instance.Spares.Where(x => x.GroupID == GroupID).ToList();
            List<brand> brands = da.GetBrands();
            foreach (brand i in brands)
            {
                if (spares.Where(x => x.BrandID == i.id).Count() > 0)
                {
                    items.Add(i.name);
                }
            }
            items.Sort();
            return items;
        }

        public List<SpareView> FilterSpares(
          int SearchFieldIndex,
          string SearchText,
          bool RemainsOnly,
          int GroupID,
          string BrandName,
          bool MismatchOnly,
            int WarehouseID)
        {
            List<SpareView> ResultList = new List<SpareView>();

            int BrandID = 0;
            if (BrandName.Length > 0)
                BrandID = da.GetBrandIDByName(BrandName);

            //int GroupID,
            if (GroupID > 0 && (!(BrandID > 0)))
            {
                if (GroupID > 0)
                {
                    ResultList = SpareContainer.Instance.Spares.Where(x => x != null).Where(x => (x.GroupID == GroupID)
                        || (x.spare_group1_id == GroupID)
                        || (x.spare_group2_id == GroupID)
                        || (x.spare_group3_id == GroupID)).ToList();
                }
            }
            else

                //int BrandID
                if (BrandID > 0)
                {
                    if (GroupID > 0)
                        ResultList = SpareContainer.Instance.Spares.Where(x => ((x.GroupID == GroupID)
                        || (x.spare_group1_id == GroupID)
                        || (x.spare_group2_id == GroupID)
                        || (x.spare_group3_id == GroupID))
                        && (x.BrandID == BrandID)).ToList();
                }

            //bool RemainsOnly,
            if (RemainsOnly)
            {
                ResultList = ResultList.Where(i => i.QRest > 0).ToList();
            }

            //bool MismatchOnly,
            if (MismatchOnly)
            {
                ResultList = ResultList.Where(i => i.QRest.Value != (int)i.q_rest.Value).ToList();
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

        public List<SpareView> FilterSpares(
           int SearchFieldIndex,
           string SearchText,
           bool RemainsOnly,
           int GroupID,
           string BrandName,
           bool MismatchOnly)
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
                        ResultList = SpareContainer.Instance.Spares.Where(x => x != null).Where(x => (x.GroupID == GroupID)
                            || (x.spare_group1_id == GroupID)
                            || (x.spare_group2_id == GroupID)
                            || (x.spare_group3_id == GroupID)).ToList();
                    }
                }
                else

                    //int BrandID
                    if (BrandID > 0)
                    {
                        if (GroupID > 0)
                            ResultList = SpareContainer.Instance.Spares.Where(x => ((x.GroupID == GroupID)
                            || (x.spare_group1_id == GroupID)
                            || (x.spare_group2_id == GroupID)
                            || (x.spare_group3_id == GroupID))
                            && (x.BrandID == BrandID)).ToList();
                    }

            //bool RemainsOnly,
            if (RemainsOnly)
            {
                ResultList = ResultList.Where(i => i.QRest > 0).ToList();
            }

            //bool MismatchOnly,
            if (MismatchOnly)
            {
                ResultList = ResultList.Where(i => i.QRest.Value != (int)i.q_rest.Value).ToList();
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

        private void LoadWarehouses()
        {
            edtWarehouse.Items.Clear();
            edtWarehouse.Items.Add("Все склады");
            da = new DataAccess();
            List<warehouse> list = da.GetWarehouses();
            foreach (warehouse w in list)
            {
                edtWarehouse.Items.Add(w.name);
            }
            edtWarehouse.SelectedIndex = 0;
        }

        //============================================================
        public Revision3EditView()
        {
            InitializeComponent();
        }

        private bool IsReady = false;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IsReady = true;
            da = new DataAccess();
            LoadGroups();
            LoadSpares();
            LoadWarehouses();
            edtDate.SelectedDate = DateTime.Now;
        }

        private void edtSearchField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _SearchFieldIndex = edtSearchField.SelectedIndex;
        }

        private void edtSearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            ItemSearch();
        }

        private void ItemSearch()
        {
            // получение параметров
            string SearchText = edtSearchText.Text;
            int SearcFieldIndex = edtSearchField.SelectedIndex;
            _SearchTextAndIndex(SearchText, SearcFieldIndex);
        }

        private void edtShowRests_Click(object sender, RoutedEventArgs e)
        {
            _RemainsOnly = edtShowRests.IsChecked.Value;
        }

        private void treeSpareGroups_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            GroupsTreeViewSelectionChanged(sender, e);
        }

        private bool isManualEditCommit;

        private void HandleMainDataGridCellEditEnding(
          object sender, DataGridCellEditEndingEventArgs e)
        {
            if (!isManualEditCommit)
            {
                isManualEditCommit = true;
                DataGrid grid = (DataGrid)sender;
                grid.CommitEdit(DataGridEditingUnit.Row, true);
                isManualEditCommit = false;
            }
        }

        private void dgSpares_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            HandleMainDataGridCellEditEnding(sender, e);
            SpareView i = (SpareView)e.Row.DataContext;
            if (i.id > 0)
                if (i.q_rest.HasValue)
                    if (i.q_rest.Value >= 0)
                        da.SpareUpdateQReal(i.id, i.q_rest.Value);
                    else
                        i.q_rest = 0;
        }

        private void btnNullRealQ_Click(object sender, RoutedEventArgs e)
        {
            SetQToZero();
        }

        private void SetQToZero()
        {
            MessageBoxResult mbr = MessageBox.Show("Операция может занять некоторое время! Продолжить?", "Уведомление", MessageBoxButton.YesNo);
            if (mbr == MessageBoxResult.Yes)
            {
                da.SpareUpdateQRealToZero();
                LoadSpares();
            }
        }

        private void ShowMismatch()
        {
            _MismatchOnly = btnShowMismatch.IsChecked.Value;
        }

        private void btnShowMismatch_Click(object sender, RoutedEventArgs e)
        {
            ShowMismatch();
        }

        private void SparePlus()
        {
            if (dgSpares.SelectedItem != null)
            {
                if ((dgSpares.SelectedItem as SpareView) != null)
                {
                    SpareView i = (dgSpares.SelectedItem as SpareView);
                    if (!(dgSpares.SelectedItem as SpareView).q_rest.HasValue)
                        (dgSpares.SelectedItem as SpareView).q_rest = 0;
                    (dgSpares.SelectedItem as SpareView).q_rest++;
                    da.SpareUpdateQReal(i.id, i.q_rest.Value);
                }
            }
        }

        private void SpareMinus()
        {
            if (dgSpares.SelectedItem != null)
            {
                if ((dgSpares.SelectedItem as SpareView) != null)
                {
                    SpareView i = (dgSpares.SelectedItem as SpareView);
                    if (!(dgSpares.SelectedItem as SpareView).q_rest.HasValue)
                        (dgSpares.SelectedItem as SpareView).q_rest = 0;
                    else
                    {
                        if ((dgSpares.SelectedItem as SpareView).q_rest.Value > 0)
                            (dgSpares.SelectedItem as SpareView).q_rest--;
                        da.SpareUpdateQReal(i.id, i.q_rest.Value);
                    }
                }
            }
        }

        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            SparePlus();
        }

        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            SpareMinus();
        }

        private void Print()
        {
            MessageBoxResult mbr = MessageBox.Show("Операция может занять некоторое время! Продолжить?", "Уведомление", MessageBoxButton.YesNo);
            if (mbr == MessageBoxResult.Yes)
            {
                Reporter.GenerateRevisionReport(FilterSpares(_SearchFieldIndex, _SearchText, _RemainsOnly, _GroupID, _BrandName, _mismatchOnly), edtDate.SelectedDate.Value);
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            Print();
        }

        private void SearchTextBox_Search(object sender, RoutedEventArgs e)
        {
            ItemSearch();
        }

        private void edtWarehouse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WarehouseChanged();
        }

        private void WarehouseChanged()
        {
            LoadSpares();
        }

        private void LoadGroups()
        {
            treeSpareGroups.ItemsSource = da.GetRoots();
            _groupID = 0;
        }

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

        private void btnFixQ_Click(object sender, RoutedEventArgs e)
        {
            FixCurrentSpareQuantity();
        }

        private void FixCurrentSpareQuantity()
        {
            int SelectedSpareID = 0;
            SpareView spare = null;
            if (dgSpares.SelectedItem != null)
                spare = dgSpares.SelectedItem as SpareView;
            else
                return;
            SelectedSpareID = spare.id;
            decimal NewQuantity = spare.q_rest.HasValue ? spare.q_rest.Value : 0;
            try
            {
                SpareContainer.Instance.FixQuantity(SelectedSpareID, NewQuantity);
                LoadSpares();
                MessageBox.Show("Изменения сохранены.");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}