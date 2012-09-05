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
using System.Windows.Navigation;
using System.Windows.Shapes;
using bycar;
using bycar3.Views.Common;
using bycar3.Views.Spare_Income;
using bycar3.Views.Spare_Outgo;
using bycar3.Views.Invoice;
using bycar3.Views.Overpricing;
using bycar3.Views.Revision2;
using bycar3.Views.Sales;

namespace bycar3.Views.Main_window
{
    /// <summary>
    /// Interaction logic for UCMovements.xaml
    /// </summary>
    public partial class UCMovements : UserControl
    {
        DataAccess da = new DataAccess();
        public MainWindow ParentWindow = null;

        public UCMovements()
        {
            InitializeComponent();
        }

        #region CUSTOM FUNCTIONS
        // обновить данные в окошке
        public void RefreshWindow()
        {
            // загрузить список запчастей в таблицу
            //LoadSpares();
            da = new DataAccess();
            if (!da.getProfileCurrent().BasicCurrencyCode.Contains("BYR"))
                tabItemOverpricing.Visibility = System.Windows.Visibility.Collapsed;
            else
                tabItemOverpricing.Visibility = System.Windows.Visibility.Visible;            
        }        
        private void LoadSpareIncome()
        {
            da = new DataAccess();
            List<SpareIncomeView> list =  da.SpareIncomeGetAll();
            foreach (SpareIncomeView s in list)
            {
                string strDate = "";
                strDate += s.Date.Year + ".";
                if (s.Date.Month < 10)
                    strDate += "0";
                strDate += s.Date.Month + ".";
                if(s.Date.Day<10)
                    strDate +="0";
                strDate+=s.Date.Day;
                                
                s.DateString = strDate;
            }
            dgSpareMovementIn.DataContext = list.OrderByDescending( s => s.Date);
        }

        private void LoadSpareOutgo()
        {
            da = new DataAccess();
            List<SpareOutgoView> list = da.SpareOutgoGetAll();
            foreach (SpareOutgoView s in list)
            {
                string strDate = "";
                strDate += s.Date.Value.Year + ".";
                if (s.Date.Value.Month < 10)
                    strDate += "0";
                strDate += s.Date.Value.Month + ".";
                if (s.Date.Value.Day < 10)
                    strDate += "0";
                strDate += s.Date.Value.Day;
                s.DateString = strDate;
            }
            dgSpareMovementOut.DataContext = list.OrderByDescending(s => s.Date);
        }
        private void LoadInvoices()
        {
            da = new DataAccess();
            List<invoice> list = da.InvoiceGet();
            foreach(invoice s in list)
            {
                if (s.InvoiceDate != null)
                {
                    string strDate = "";
                    strDate += s.InvoiceDate.Value.Year + ".";
                    if (s.InvoiceDate.Value.Month < 10)
                        strDate += "0";
                    strDate += s.InvoiceDate.Value.Month + ".";
                    if (s.InvoiceDate.Value.Day < 10)
                        strDate += "0";
                    strDate += s.InvoiceDate.Value.Day;
                    s.DateString = strDate;
                }
            }
            dgInvoices.DataContext = list.OrderByDescending(s => s.InvoiceDate);
        }
        public void LoadSales()
        {
            da = new DataAccess();
            List<Sale> list = da.SaleGet();            
            dgSales.DataContext = list.OrderByDescending(s => s.Number);
        }
        private void LoadOverpricings()
        {
            da = new DataAccess();
            List<overpricing> list = da.OverpricingGet();
            foreach (overpricing s in list)
            {
                string strDate = "";
                strDate += s.createdOn.Value.Year + ".";
                if (s.createdOn.Value.Month < 10)
                    strDate += "0";
                strDate += s.createdOn.Value.Month + ".";
                if (s.createdOn.Value.Day < 10)
                    strDate += "0";
                strDate += s.createdOn.Value.Day;
                s.DateString = strDate;
            }
            dgOverpricing.DataContext = list.OrderByDescending(s => s.createdOn);
        }
        
        private void LoadRevisions()
        {/*
            da = new DataAccess();
            List<revision> list = da.RevisionGet();
            foreach (revision s in list)
            {
                string strDate = "";
                if (s.RevisionDate.Value.Day < 10)
                    strDate += "0";
                strDate += s.RevisionDate.Value.Day + ".";
                if (s.RevisionDate.Value.Month < 10)
                    strDate += "0";
                strDate += s.RevisionDate.Value.Month + ".";
                strDate += s.RevisionDate.Value.Year;
                s.DateString = strDate;
            }
            dgRevision.DataContext = list;*/
        }
        public void ItemCreate()
        {
            // определить, какая из вкладок активна
            int selectedTabIndex = tabControlSpareMovements.SelectedIndex;
            switch (selectedTabIndex)
            {
                //case 2: // вызвать минидиалог выбора типа накладной - отгрузка или приход
                  //  SelectSpareMovementTypeView selView = new SelectSpareMovementTypeView();
                    //selView.ShowDialog();
                    //break;
                case 0: // поступление
                    CreateSpareMovementIn();
                    break;
                case 1: // отгрузка
                    CreateSpareMovementOut();
                    break;
                case 2: // счёт-фактура
                    CreateInvoice();
                    break;
                case 3: // переоценка
                    CreateOverpricing();
                    break;
                case 4: // ревизия
                    CreateRevision();
                    break;
            }
        }
              
        public void ItemEdit()
        {
            // определить, какая из вкладок активна
            int selectedTabIndex = tabControlSpareMovements.SelectedIndex;
            switch (selectedTabIndex)
            {
                //case 2: // вызвать минидиалог выбора типа накладной - отгрузка или приход
                    //SelectSpareMovementTypeView selView = new SelectSpareMovementTypeView();
                    //selView.ShowDialog();
                  //  break;
                case 0: // поступление
                    // вызвать диалог прихода
                    EditSelectedSpareMovementIn();
                    break;
                case 1: // отгрузка
                    // вызвать диалог добавление отгрузки
                    EditSelectedSpareMovementOut();
                    break;
                case 2: // счёт-фактура
                    EditInvoice();
                    break;
                case 3: // переоценка
                    EditOverpicing();
                    break;
                case 4: // ревизия
                    EditRevision();
                    break;
            }
        }

        public void ItemDelete()
        {
            // определить, какая из вкладок активна
            TabItem item = tabControlSpareMovements.SelectedItem as TabItem;
            switch (item.Name)
            {
                //case 2: // вызвать минидиалог выбора типа накладной - отгрузка или приход
                    //SelectSpareMovementTypeView selView = new SelectSpareMovementTypeView();
                    //selView.ShowDialog();
                  //  break;
                case "tabControlSpareMovements": // поступление
                    // вызвать диалог прихода
                    DeleteSelectedSpareMovementIn();
                    break;
                case "tabItemOfferingsOut": // отгрузка
                    // удаление отгрузки            
                    DeleteSelectedSpareMovementOut();
                    break;
                case "tabItemInvoices": // инвойс
                    // удаление отгрузки            
                    DeleteInvoice();
                    break;
                case "tabItemOverpricing": // переоценка                    
                    DeleteOverpicing();
                    break;
                /*case 4: // ревизия
                    DeleteRevision();
                    break;*/
                case "tabItemSales":
                    DeleteSale();
                    break;
            }
        }
        private void CreateSpareMovementIn()
        {
            // вызвать диалог добавление прихода
            SpareIncomeEditView v = new SpareIncomeEditView();
            v._id = -1;
            v.ShowDialog();
            LoadSpareIncome();
        }
        private void CreateInvoice()
        {
            // вызвать диалог добавление прихода
            InvoiceEditView v = new InvoiceEditView();            
            v.ShowDialog();
            LoadInvoices();
        }
        private void CreateOverpricing()
        {
            // вызвать диалог добавление прихода
            OverpricingEditView v = new OverpricingEditView();            
            v.ShowDialog();
            LoadOverpricings();
        }
        private void CreateRevision()
        {
            // вызвать диалог добавление прихода
            Revision2EditView v = new Revision2EditView();            
            v.ShowDialog();
            LoadRevisions();
        }
        private void CreateSpareMovementOut()
        {
            // вызвать диалог добавление прихода
            SpareOutgoEditView v = new SpareOutgoEditView();
            v._id = -1;
            v.ShowDialog();
            LoadSpareOutgo();
        }
        private void EditSelectedSpareMovementIn()
        {
            int id = 0;
            SpareIncomeView b = null;
            DataGrid dataGrid = dgSpareMovementIn;
            if (dataGrid.SelectedItem != null)
            {
                object sel = dataGrid.SelectedItem;
                b = (SpareIncomeView)sel;
                id = b.id;
            }
            if (id > 0)
            {
                SpareIncomeEditView v = new SpareIncomeEditView();
                v._id = b.id;
                v.ShowDialog();
                LoadSpareIncome();
            }
        }
        private void EditInvoice()
        {
            int id = 0;
            invoice b = null;
            DataGrid dataGrid = dgInvoices;
            if (dataGrid.SelectedItem != null)
            {
                object sel = dataGrid.SelectedItem;
                b = (invoice)sel;
                id = b.id;
            }
            if (id > 0)
            {
                InvoiceEditView v = new InvoiceEditView(b.id);                
                v.ShowDialog();
                LoadInvoices();
            }
        }
        private void EditOverpicing()
        {
            int id = 0;
            overpricing b = null;
            DataGrid dataGrid = dgOverpricing;
            if (dataGrid.SelectedItem != null)
            {
                object sel = dataGrid.SelectedItem;
                b = (overpricing)sel;
                id = b.id;
            }
            if (id > 0)
            {
                OverpricingEditView v = new OverpricingEditView(b.id);                
                v.ShowDialog();
                LoadOverpricings();
            }
        }
        private void EditRevision()
        {
            int id = 0;
            revision b = null;
            /*DataGrid dataGrid = dgRevision;
            if (dataGrid.SelectedItem != null)
            {
                object sel = dataGrid.SelectedItem;
                b = (revision)sel;
                id = b.id;
            }
            if (id > 0)
            {
                Revision2EditView v = new Revision2EditView(b.id);                
                v.ShowDialog();
                LoadRevisions();
            }*/
        }

        private void EditSelectedSpareMovementOut()
        {
            int id = 0;
            SpareOutgoView b = null;
            DataGrid dataGrid = dgSpareMovementOut;
            if (dataGrid.SelectedItem != null)
            {
                object sel = dataGrid.SelectedItem;
                b = (SpareOutgoView)sel;
                id = b.id;
            }
            if (id > 0)
            {                
                SpareOutgoEditView v = new SpareOutgoEditView();
                v._id = b.id;
                v.ShowDialog();                
                LoadSpareOutgo();
            }
        }

        private void DeleteSelectedSpareMovementIn()
        {            
            DataGrid dg = dgSpareMovementIn;
            if (dg.SelectedItems.Count > 0)
            {
                MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выделенные записи?", "Удаление", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {                   
                    foreach (SpareIncomeView sov in dg.SelectedItems)
                    {                        
                        da.SpareIncomeDelete(sov.id);
                    }
                    LoadSpareIncome();
                }
            }            
        }

        private void DeleteSelectedSpareMovementOut()
        {
            bool DeleteFlag = false;
            DataGrid dg = dgSpareMovementOut;
            if (dg.SelectedItems.Count > 0)
            {
                MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выделенные записи?", "Удаление", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)                
                    DeleteFlag = true;                
            }
            if(DeleteFlag)
            {
                foreach(SpareOutgoView sov in dg.SelectedItems)
                {
                    da.SpareOutgoDelete(sov.id);                    
                }
                LoadSpareOutgo();
            }            
        }
        private void DeleteInvoice()
        {
            DataGrid dg = dgInvoices;
            if (dg.SelectedItems.Count > 0)
            {
                MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выделенные записи?", "Удаление", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    foreach (invoice i in dg.SelectedItems)
                    {
                        da.InvoiceDelete(i.id);
                    }
                    LoadInvoices();
                }
            }
        }
        private void DeleteSale()
        {
            DataGrid dg = dgSales;
            if (dg.SelectedItems.Count > 0)
            {
                MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выделенные записи?", "Удаление", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    foreach (Sale i in dg.SelectedItems)
                    {
                        da.SaleDelete(i.ID);
                    }
                    LoadSales();
                }
            }
        }
        private void DeleteOverpicing()
        {
            DataGrid dg = dgOverpricing;
            if (dg.SelectedItems.Count > 0)
            {
                MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выделенные записи?", "Удаление", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    foreach (overpricing i in dg.SelectedItems)
                    {
                        da.OverpricingDelete(i.id);
                    }
                    LoadOverpricings();
                }
            }
        }
        private void DeleteRevision()
        {
            int id = 0;
            revision b = null;
            /*DataGrid dataGrid = dgRevision;
            if (dataGrid.SelectedItem != null)
            {
                object sel = dataGrid.SelectedItem;
                b = (revision)sel;
                id = b.id;
            }
            if (id > 0)
            {
                MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выделенную запись?", "Удаление", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    da.RevisionDelete(id);
                    LoadRevisions();
                }
            }*/
        }
        #endregion
        // HANDLERS
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshWindow();
        }

        private void tabItemOfferingsOut_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSpareOutgo();
        }
        private void tabControlSpareIncome_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSpareIncome();
        }

        private void dgSpareMovementIn_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditSelectedSpareMovementIn();

            /* DEBUG CODE
             * string s = 
                 "DataGrid:   " + dgSpareMovementIn.ActualHeight + "\n";
            s += "TabItem:    " + tabControlSpareIncome.ActualHeight + "\n";
            s += "TabControl: " + tabControlSpareMovements.ActualHeight + "\n";            
            s += "Grid:       " + grMain.ActualHeight + "\n";
            s += "UserCtrl:   " + this.ActualHeight + "\n";
            MessageBox.Show(s);*/
        }

        private void dgSpareMovementOut_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditSelectedSpareMovementOut();
        }

        private void tabItemInvoices_Loaded(object sender, RoutedEventArgs e)
        {
            LoadInvoices();
        }

        private void dgInvoices_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditInvoice();
        }

        private void tabItemOverpricing_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOverpricings();
        }

        private void dgOverpricing_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditOverpicing();
        }      

        private void tabItemRevision_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRevisions();
        }

        private void dgRevision_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditRevision();
        }

        private void tabControlSpareMovements_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ParentWindow != null)
            {
                switch (tabControlSpareMovements.SelectedIndex)
                {
                    case 0: // поступление товаров                
                        ParentWindow.btnItemAdd.IsEnabled = true;                        
                        ParentWindow.btnItemEdit.IsEnabled = true;
                        ParentWindow.btnItemAdd.ToolTip = "Создать новую приходную накладную";
                        ParentWindow.btnItemDelete.ToolTip = "Удалить выделенную приходную накладную";
                        ParentWindow.btnItemEdit.ToolTip = "Редактировать выделенную приходную накладную";
                        break;
                    case 1: // отгрузка товаров
                        ParentWindow.btnItemAdd.IsEnabled = true;                        
                        ParentWindow.btnItemEdit.IsEnabled = true;
                        ParentWindow.btnItemAdd.ToolTip = "Создать новую расходную накладную";
                        ParentWindow.btnItemDelete.ToolTip = "Удалить выделенную расходную накладную";
                        ParentWindow.btnItemEdit.ToolTip = "Редактировать выделенную расходную накладную";
                        break;
                    case 2: // счет-фактура
                        ParentWindow.btnItemAdd.IsEnabled = true;                        
                        ParentWindow.btnItemEdit.IsEnabled = true;
                        ParentWindow.btnItemAdd.ToolTip = "Создать новую счёт-фактуру";
                        ParentWindow.btnItemDelete.ToolTip = "Удалить выделенную счёт-фактуру";
                        ParentWindow.btnItemEdit.ToolTip = "Редактировать выделенную счёт-фактуру";
                        LoadSpareOutgo();
                        break;
                    case 3: // переоценка
                        ParentWindow.btnItemAdd.IsEnabled = true;                        
                        ParentWindow.btnItemEdit.IsEnabled = true;
                        ParentWindow.btnItemAdd.ToolTip = "Создать новый бланк переоценки";
                        ParentWindow.btnItemDelete.ToolTip = "Удалить выделенный бланк переоценки";
                        ParentWindow.btnItemEdit.ToolTip = "Редактировать выделенный бланк переоценки";
                        break;
                    case 4: // корзины (быстрые покупки)
                        ParentWindow.btnItemAdd.IsEnabled = false;                        
                        ParentWindow.btnItemEdit.IsEnabled = false;
                        break;  
                }
            }   
            if (!da.getProfileCurrent().BasicCurrencyCode.Contains("BYR"))
                tabItemOverpricing.Visibility = System.Windows.Visibility.Collapsed;
            else
                tabItemOverpricing.Visibility = System.Windows.Visibility.Visible;            
        }

        private void tabItemSales_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSales();
        }

        private void dgSales_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgSales.SelectedItem != null)
            {
                int SaleID = (dgSales.SelectedItem as Sale).ID;
                SaleEditView v = new SaleEditView(SaleID);
                v.ShowDialog();
            }
        }                
    }
}
