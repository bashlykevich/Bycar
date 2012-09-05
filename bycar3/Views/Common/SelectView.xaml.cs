using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using bycar.Utils;
using System.Collections;
using bycar;
using bycar3.Views.Account;

namespace bycar3.Views.Common
{
    /// <summary>
    /// Interaction logic for SelectView.xaml
    /// </summary>
    public partial class SelectView : Window
    {
        #region DATA MEMBERS
        private List<CommonSelectItem> items = null;
        public string ClassName = "";
        public string RES = "";
        public int SelectedID = 0;
        public CommonSelectItem Selected = null;
        DataAccess da = new DataAccess();
        public int ParentItemID = -1;

        #endregion

        #region CUSTOM FUNCTIONS
        public IList Items
        {
            get
            {
                return items;
            }
            set
            {
                items = new System.Collections.Generic.List<CommonSelectItem>();
                foreach (CommonSelectItem i in value)
                {
                    items.Add(i);
                }
            }
        }
        
        public SelectView()
        {
            InitializeComponent();
        }
        public void ItemAdd()
        {
            Window v = null;
            if (ClassName.Equals((new account()).ToString()))
            {
                v = new AccountsEditView();
                (v as AccountsEditView)._id = -1;
            }
            if (ClassName.Equals((new bank()).ToString()))
            {
                v = new BanksEditView();
                (v as BanksEditView)._ID = -1;
            }
            if (ClassName.Equals((new warehouse()).ToString()))
            {
                v = new WarehousesEditView();
                (v as WarehousesEditView)._id = -1;
            }
            if (ClassName.Equals((new brand()).ToString()))
            {
                v = new BrandsEditView();
                (v as BrandsEditView)._id = -1;
            }
            if (ClassName.Equals((new car_producer()).ToString()))
            {
                v = new CarProducersEditView();
                (v as CarProducersEditView)._id = -1;
            } 
            if (ClassName.Equals((new unit()).ToString()))
            {
                v = new UnitsEditView();
                (v as UnitsEditView)._id = -1;
            }            
            if (ClassName.Equals((new spare_group()).ToString()))
            {
                MessageBox.Show("Группы можно создавать только из дерева групп!");
                return;
                //v = new SpareGroupEditView();
                //(v as SpareGroupEditView).p = -1;
            }
            if (ClassName.Equals((new bank_account()).ToString()))
            {
                v = new BankAccountEditView(ParentItemID);
                //(v as BankAccountEditView). = -1;
            }
            if (ClassName.Equals((new BankAccountView()).ToString()))
            {
                v = new BankAccountEditView(ParentItemID);
                //(v as BankAccountEditView). = -1;
            }
            //else
                v.ShowDialog();
            LoadItems();
        }
        /*
        public void ItemAdd(int ParentID)
        {
            Window v = null;            
            if (ClassName.Equals((new bank_account()).ToString()))
            {
                v = new BankAccountEditView(ParentID);
                //(v as BankAccountEditView). = -1;
            }            
            else
                v.ShowDialog();
            LoadItems();
        }*/       
        public void ItemEdit()
        {
            if (dgItems.SelectedItem == null)
            {
                MessageBox.Show("Не выбрана запись для редактирования!");
                return;
            }
            Window v = null;
            if (ClassName.Equals((new account()).ToString()))
            {
                v = new AccountsEditView();
                (v as AccountsEditView).LoadItem((dgItems.SelectedItem as account).id);
            }
            if (ClassName.Equals((new warehouse()).ToString()))
            {
                v = new WarehousesEditView();
                (v as WarehousesEditView).LoadItem((dgItems.SelectedItem as warehouse).id);
            }
            if (ClassName.Equals((new bank()).ToString()))
            {
                v = new BanksEditView();
                (v as BanksEditView).LoadItem((dgItems.SelectedItem as bank).id);
            }
            if (ClassName.Equals((new brand()).ToString()))
            {
                v = new BrandsEditView();
                (v as BrandsEditView).LoadItem((dgItems.SelectedItem as brand).id);
            }
            if (ClassName.Equals((new car_producer()).ToString()))
            {
                v = new CarProducersEditView();
                (v as CarProducersEditView).LoadItem((dgItems.SelectedItem as car_producer).id);
            }
            if (ClassName.Equals((new unit()).ToString()))
            {
                v = new UnitsEditView();
                (v as UnitsEditView).LoadItem((dgItems.SelectedItem as unit).id);
            }
            if (ClassName.Equals((new bank_account()).ToString()))
            {
                v = new BankAccountEditView();
                (v as BankAccountEditView).LoadItem((dgItems.SelectedItem as BankAccountView).id);
            }
            if (ClassName.Equals((new BankAccountView()).ToString()))
            {
                v = new BankAccountEditView();
                (v as BankAccountEditView).LoadItem((dgItems.SelectedItem as BankAccountView).id);
            }
            if (ClassName.Equals((new spare_group()).ToString()))
            {
                MessageBox.Show("Группы можно редактировать только из дерева групп!");
                return;
                //v = new SpareGroupEditView();
                //(v as SpareGroupEditView).LoadItem((dgItems.SelectedItem as spare_group).name);
            }            
            v.ShowDialog();
            LoadItems();
        }
        public void ItemDelete()
        {
            if (dgItems.SelectedItem == null)
            {
                MessageBox.Show("Не выбрана запись для удаления!");
                return;
            }
            else
            {
                MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выделенную запись?", "Удаление", MessageBoxButton.YesNo);
                if (res != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            //Window v = null;
            if (ClassName.Equals((new account()).ToString()))
            {
                da.AccountDelete((dgItems.SelectedItem as account).id);
            }
            if (ClassName.Equals((new warehouse()).ToString()))
            {
                da.WarehouseDelete((dgItems.SelectedItem as warehouse).id);
            } 
            if (ClassName.Equals((new bank()).ToString()))
            {
                da.BankDelete((dgItems.SelectedItem as bank).id);
            }
            if (ClassName.Equals((new brand()).ToString()))
            {                
                da.BrandDelete((dgItems.SelectedItem as brand).id);
            }
            if (ClassName.Equals((new car_producer()).ToString()))
            {
                da.CarProducerDelete((dgItems.SelectedItem as car_producer).id);
            }
            if (ClassName.Equals((new unit()).ToString()))
            {                
                da.UnitDelete((dgItems.SelectedItem as unit).id);
            }
            if (ClassName.Equals((new bank_account()).ToString()))
            {
                da.BankAccountDelete((dgItems.SelectedItem as BankAccountView).id);
            }
            if (ClassName.Equals((new spare_group()).ToString()))
            {
                MessageBox.Show("Группы можно удалять только из дерева групп!");
                return;
            } else             
                LoadItems();
        }
        #endregion

        // HANDLERS

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int ind = LoadItems();
            dgItems.SelectedIndex = ind;
        }

        private void edtSearchText_KeyDown(object sender, KeyEventArgs e)
        {
            string searchString = edtSearchText.Text;
            switch (e.Key)
            {
                case Key.Enter:
                    if (searchString.Length == 0)
                    {
                        MessageBox.Show("Вы не задали строку поиска!");
                    }
                    else
                    {
                        ItemSearch();
                    }
                    break;
            }
        }

        private void btnSpareSearch_Click(object sender, RoutedEventArgs e)
        {
            ItemSearch();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            getSelectedItemName();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
                
        private int LoadItems()
        {
            int ind = 0;
            da = new DataAccess();
            if(ParentItemID > 0)
                dgItems.DataContext = da.getItems(ClassName, ParentItemID);
            else
                dgItems.DataContext = da.getItems(ClassName);
            return ind;
        }
        void ItemSearch()
        {
            if (edtSearchText != null)
            {
                string searchString = edtSearchText.Text;
                int searchFieldIndex = edtSearchField.SelectedIndex;               
                LoadItems(searchFieldIndex, searchString, ParentItemID);            
            }

        }        
        private void LoadItems(int searchFieldIndex, string searchString)
        {
            LoadItems(searchFieldIndex, searchString, 0);
        }
        private void LoadItems(int searchFieldIndex, string searchString, int ParentItemID)
        {                        
            if (searchString.Length == 0)
                Items = da.getItems(ClassName, ParentItemID);
            else
                Items = da.getItems(searchFieldIndex, searchString, ClassName, ParentItemID);
            dgItems.DataContext = items;                            
        }
        
        void getSelectedItemName()
       {            
            try
            {
                if (dgItems.SelectedItem != null)
                {
                    Selected = dgItems.SelectedItem as CommonSelectItem;                    
                    RES = Selected._Name;                    
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Сначала выберите деталь из списка");
                }
            }
            catch (Exception)
            {
                LoadItems();
            }            
        }

        private void dgItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            getSelectedItemName();
        }

        private void btnItemAdd_Click(object sender, RoutedEventArgs e)
        {
            ItemAdd();
        }

        private void btnItemEdit_Click(object sender, RoutedEventArgs e)
        {
            ItemEdit();
        }

        private void btnItemDelete_Click(object sender, RoutedEventArgs e)
        {
            ItemDelete();
        }

        private void edtSearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            ItemSearch();
        }

        private void edtSearchText_TextInput(object sender, TextCompositionEventArgs e)
        {
            ItemSearch();
        }

        private void edtSearchField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemSearch();
        }
        private void SearchTextBox_Search(object sender, RoutedEventArgs e)
        {
            ItemSearch();
        }
    }
}

