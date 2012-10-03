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
using bycar3.External_Code;
using bycar3.Reporting;

namespace bycar3.Views.Revision3
{
    /// <summary>
    /// Interaction logic for Revision3EditView.xaml
    /// </summary>
    public partial class Revision3EditView : Window
    {
        #region MEMBERS
        int SelectedSpareID = 0;
        
        public bool RevisionModeOn = false;
        int _searchFieldIndex = 0;
        public int _SearchFieldIndex
        {
            get { return _searchFieldIndex; }
            set
            {
                _searchFieldIndex = value;
                //XXXXXXLoadSpares(_SearchFieldIndex, _SearchText, _RemainsOnly, _GroupName, _GroupID, _BrandName, _BrandID);
                LoadSpares2();
            }
        }
        string _searchText = "";
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
                //XXXXXXLoadSpares(_SearchFieldIndex, _SearchText, _RemainsOnly, _GroupName, _GroupID, _BrandName, _BrandID);
                LoadSpares2();
            }
        }
        bool _remainsOnly = false;
        public bool _RemainsOnly
        {
            get { return _remainsOnly; }
            set
            {
                _remainsOnly = value;
                //XXXXXXLoadSpares(_SearchFieldIndex, _SearchText, _RemainsOnly, _GroupName, _GroupID, _BrandName, _BrandID);
                LoadSpares2();
            }
        }
        bool _mismatchOnly = false;
        public bool _MismatchOnly
        {
            get { return _mismatchOnly; }
            set
            {
                _mismatchOnly = value;                
                LoadSpares2();
            }
        }
        int _groupID = 39;

        public int _GroupID
        {
            get { return _groupID; }
            set
            {
                _groupID = value;
                //XXXXXXLoadSpares(_SearchFieldIndex, _SearchText, _RemainsOnly, _GroupName, _GroupID, _BrandName, _BrandID);
                LoadSpares2();
            }
        }
        string _brandName = "";

        public string _BrandName
        {
            get { return _brandName; }
            set
            {
                _brandName = value;
                //XXXXXXLoadSpares(_SearchFieldIndex, _SearchText, _RemainsOnly, _GroupName, _GroupID, _BrandName, _BrandID);
                LoadSpares2();
            }
        }
        int _brandID = 0;

        public int _BrandID
        {
            get { return _brandID; }
            set
            {
                _brandID = value;
                //XXXXXXLoadSpares(_SearchFieldIndex, _SearchText, _RemainsOnly, _GroupName, _GroupID, _BrandName, _BrandID);
                LoadSpares2();
            }
        }
        void _GroupIDBrandName(int GroupID, string BrandName)
        {
            _groupID = GroupID;
            _brandName = BrandName;
            //XXXXXXLoadSpares(_SearchFieldIndex, _SearchText, _RemainsOnly, _GroupName, _GroupID, _BrandName, _BrandID);
            LoadSpares2();
        }
         
        #endregion

        DataAccess da = new DataAccess();
        public void LoadSpares()
        {
            LoadSpares2();
        }
        public void LoadSpares2()
        {
            try
            {
                int SelectedSpareID = 0;
                if (dgSpares.SelectedItem != null)
                    SelectedSpareID = (dgSpares.SelectedItem as SpareView).id;
                dgSpares.DataContext = FilterSpares(_SearchFieldIndex, _SearchText, _RemainsOnly, _GroupID, _BrandName, _mismatchOnly);
            }
            catch (Exception)
            {
            }
        }
        // загрузить список групп в дерево
        public void LoadGroups(bool expand)
        {            
            da = new DataAccess();
            treeSpareGroups.Items.Clear();
            var list = da.GetSpareGroups().Where(g => g.ParentGroup == null).OrderBy(g => g.name);
            foreach (spare_group g in list)
            {
                TreeViewItem root = new TreeViewItem();
                root.Header = g.name;
                root.Name = "TVIId" + g.id;
                BuildTreeBrunch(root, g.id, expand);
                root.IsExpanded = true;
                root.IsSelected = true;
                treeSpareGroups.Items.Add(root);
            }
            treeSpareGroups.UpdateLayout();
        }
        // построение дерева
        private int BuildTreeBrunch(TreeViewItem root, int parent_id, bool expand)
        {
            int count = 0;
            var list = da.GetSpareGroups().Where(g => g.ParentGroup != null);
            list = list.Where(g => g.ParentGroup.id == parent_id).OrderBy(g => g.name);
            foreach (spare_group g in list)
            {
                TreeViewItem tvi = new TreeViewItem();
                tvi.Header = g.name;
                tvi.Name = "TVIId" + g.id;
                BuildTreeBrunch(tvi, g.id, expand);
                if (!tvi.HasItems)
                {
                    // ВСТАВЛЯЕМ БРЭНДЫ
                    List<string> brands = getBrandsInSpareGroup(g.id);
                    foreach (string s in brands)
                    {
                        TreeViewItem tvi1 = new TreeViewItem();
                        tvi1.Header = s;
                        //tvi1.Name = "TVIBrand" + s;
                        tvi1.IsExpanded = expand;
                        tvi.Items.Add(tvi1);
                    }
                }
                tvi.IsExpanded = expand;
                count++;
                root.Items.Add(tvi);
            }
            return count;
        }
        /*Feb15
        public List<string> getBrandsInSpareGroup(string groupName)
        {
            List<string> items = new List<string>();
            //List<SpareView> spares = getSparesByGroupName(groupName);
            List<SpareView> spares = SpareContainer.Instance.Spares.Where(x => x.GroupName == groupName).ToList();
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
        }*/
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
        //========================================================        
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
        //============================================================
        public Revision3EditView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            da = new DataAccess();            
            LoadGroups(false);
            LoadSpares();
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
        void ItemSearch()
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
            GroupsTreeViewSelectionChanged();
        }
        void GroupsTreeViewSelectionChanged()
        {
            TreeViewItem selectedItem = treeSpareGroups.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                string selectedItemHeader = selectedItem.Header.ToString();
                DataAccess da = new DataAccess();
                // Если выбрана не группа, а брэнд, отобразить список групп с двумя наложенными фильтрами:
                //  1. Выбранный брэнд
                //  2. Родительская для выранного брэнда группа

                if (!da.isSpareGroup(selectedItemHeader))
                {
                    //tree_cm_Create.IsEnabled = false;
                   // tree_cm_Delete.IsEnabled = false;
                    //tree_cm_Edit.IsEnabled = false;
                    string brandName = selectedItemHeader;
                    if (selectedItem != null)
                    {
                        TreeViewItem parent = ItemsControl.ItemsControlFromItemContainer(selectedItem) as TreeViewItem;
                        if (parent != null)
                        {
                            int GroupID = 0;
                            Int32.TryParse((parent.Name.Replace("TVIId", " ")), out GroupID);
                            _GroupIDBrandName(GroupID, brandName);
                        }
                        else
                        {
                            LoadSpares();
                        }
                    }
                }
                else
                {
                    //tree_cm_Create.IsEnabled = true;
                    //tree_cm_Edit.IsEnabled = true;
                    /// Если выбрана группа, отобразить элементы всех дочерних подгрупп                    
                    int GroupID = 0;
                    Int32.TryParse((selectedItem.Name.Replace("TVIId", " ")), out GroupID);
                    //if (selectedItem == treeSpareGroups.Items[0])
                    //    tree_cm_Delete.IsEnabled = false;
                    //else
                    //    tree_cm_Delete.IsEnabled = true;
                    _GroupIDBrandName(GroupID, "");
                }
            }
            dgSpares.SelectedIndex = 0;
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
        void SetQToZero()
        {
            MessageBoxResult mbr = MessageBox.Show("Операция может занять некоторое время! Продолжить?", "Уведомление", MessageBoxButton.YesNo);
            if (mbr == MessageBoxResult.Yes)
            {
                da.SpareUpdateQRealToZero();
                LoadSpares();
            }
        }
        void ShowMismatch()
        {
            _MismatchOnly = btnShowMismatch.IsChecked.Value;
        }

        private void btnShowMismatch_Click(object sender, RoutedEventArgs e)
        {
            ShowMismatch();
        }

        void SparePlus()
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
        void SpareMinus()
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
                        if ((dgSpares.SelectedItem as SpareView).q_rest.Value>0)
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
        void Print()
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
    }
}
