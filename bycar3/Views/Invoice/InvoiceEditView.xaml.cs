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
using bycar3.Views.Spare_Income;
using bycar3.Reporting;
using bycar3.Views.Account;
using bycar3.Views.Spare_Outgo;
using bycar3.Views.Common;

namespace bycar3.Views.Invoice
{
    /// <summary>
    /// Interaction logic for InvoiceEditView.xaml
    /// </summary>
    public partial class InvoiceEditView : Window
    {
        #region DATA MEMBERS
        DataAccess da = new DataAccess();
        invoice Item = null;        
        decimal InvoiceSum = 0;
        bool DeleteNew = false;
        #endregion

        #region LOGIC METHODS
        // загрузка Item из БД, заполнение полей =====================
        void ItemToForm(int ItemID)
        {
            //.1
            Item = da.InvoiceGet(ItemID);
            if (Item.account == null)
                Item.accountReference.Load();
            if (Item.account != null)
                edtAccount.SelectedValue = Item.account.id;
            edtAccountAddress.Text = Item.AccountAddress;
            edtAccountBankMFO.Text = Item.AccountBankMFO;
            edtAccountBankName.Text = Item.AccountBankName;
            edtBankAccount.SelectedValue = Item.BankAccountID;
            edtAccountUNN.Text = Item.AccountUNN;
            if (Item.InvoiceDate.HasValue)
                edtDate.SelectedDate = Item.InvoiceDate.Value;
            edtNumber.Text = Item.InvoiceNumber.ToString();
            edtSum.Content = Item.InvoiceSum.ToString();
        }
        // заполнение Item из полей, сохранение в БД =================
        invoice FormToItem()
        {
            da = new DataAccess();
            if (Item == null)
                Item = new invoice();            
            Item.InvoiceDate = edtDate.SelectedDate.HasValue ? edtDate.SelectedDate.Value : DateTime.Now;
            int AccountID = 0;
            if (edtAccount.SelectedValue != null)
            {
                AccountID = (int)edtAccount.SelectedValue;
            }
            Item.AccountUNN = edtAccountUNN.Text;
            Item.AccountBankName = edtAccountBankName.Text;
            BankAccountView bav = edtBankAccount.SelectedItem as BankAccountView;
            if (bav != null)
            {
                Item.BankAccountID = bav.id;
            }            
            Item.AccountAddress = edtAccountAddress.Text;
            Item.AccountBankMFO = edtAccountBankMFO.Text;
            Item.InvoiceSum = (double)InvoiceSum;
            int INum = 0; 
            //if(Item.InvoiceNumber.HasValue)
                //INum = Item.InvoiceNumber.Value;
            if(Int32.TryParse(edtNumber.Text, out INum))
                Item.InvoiceNumber = INum;
            da.InvoiceEdit(Item, AccountID);
            return Item;
        }
        // загрузка товаров ==========================================
        public decimal LoadOfferings()
        {
            InvoiceSum = 0;
            da = new DataAccess();
            List<SpareInInvoiceView> items = da.GetSparesByInvoiceID(Item.id);
            foreach (SpareInInvoiceView i in items)
            {
                InvoiceSum += i.TotalWithVat.Value;
            }
            dgSpares.DataContext = items;
            edtSum.Content = "Сумма: " + InvoiceSum.ToString();
            return InvoiceSum;
        }
        // загрузка контрагентов =====================================
        private void LoadAccounts()
        {
            //edtAccount.Items.Clear();
            List<account> items = da.GetAllAccounts();
            edtAccount.DataContext = items;            
        }
        // загрузка данных выбранного контаргента
        void LoadAccountData()
        {
            account a = edtAccount.SelectedItem as account;
            if (a != null)
            {
                edtAccountAddress.Text = a.address;                
                edtAccountUNN.Text = a.unn;
            }
        }
        // загрузка счетов ====================
        void LoadBankAccounts()
        {
            if ((edtAccount.SelectedItem as account) != null)
            {
                edtBankAccount.DataContext = da.BankAccountViewGet((edtAccount.SelectedItem as account).id);
                edtBankAccount.SelectedIndex = 0;
                if (Item != null && Item.BankAccountID.HasValue)
                {                                                                     
                    if(Item.BankAccountID > 0)
                        edtBankAccount.SelectedValue = Item.BankAccountID;
                }
            }
        }
        // загрузка данных выбранного расчетного счета, вызывается из обработчика SelectionChanged комбобокса Р/С
        void LoadBankAccountData()
        {
            BankAccountView a = edtBankAccount.SelectedItem as BankAccountView;
            if (a != null)
            {
                edtAccountBankMFO.Text = a.BankMFO;
                edtAccountBankName.Text = a.BankName;
            }
        }
        // добавление запчасти в инвойс ==============================
        void AddNewOffering()
        {
            SpareInInvoiceSelectView2 v = new SpareInInvoiceSelectView2();            
            v._InvoiceID = Item.id;            
            v.CurrentCurrencyCode = "BYR";
            v.ShowDialog();
            LoadOfferings();
        }
        // удаление запчасти из инвойса =============================
        void DeleteSelectedOffering()
        {
            MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выделенную запись?", "Удаление", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {                
                // удалить выделенный товар
                int offeringId = getSelectedOfferingId();
                da.InvoiceOfferingDelete(offeringId);
                LoadOfferings();
            }
        }
        // удаление новосозданного инвойса по закрытию окна =========
        void Delete()
        {
            if (Item != null)
                da.InvoiceDelete(Item.id);
        }
        // генерация отчета для печати инвойса
        void Print()
        {
            FormToItem();
            Reporter.GenerateInvoiceReport(Item.id);
        }
        // вызов окна создания нового расчетного счета и выбор его созданного
        void CreateNewBankAccount()
        {
            account a = edtAccount.SelectedItem as account;
            if (a != null)
            {
                BankAccountEditView v = new BankAccountEditView(a.id);
                v.ShowDialog();
                LoadBankAccounts();
                edtBankAccount.SelectedValue = v.AccountID;
            }
            else
                MessageBox.Show("Сначала выберите контрагента!");            
        }
        // вызов окна создания нового контаргента
        void CreateNewAccount()
        {
            int NewID = 0;
            AccountsEditView v = new AccountsEditView();
            v.ShowDialog();
            NewID = v._id;
            LoadAccounts();
            //edtAccount.SelectedValue = NewID;
            //if (v.Selected != null)
                edtAccount.SelectedValue = NewID;
        }
        // Создать инвойс
        void CreateInvoice()
        {
            DeleteNew = true;
            Item = new invoice();              
            Item = da.InvoiceCreate(Item);
            edtNumber.Text = Item.InvoiceNumber.ToString();
        }
        // получить ID ыфбранной детали
        int getSelectedOfferingId()
        {
            int result = 0;
            try
            {
                if (dgSpares.SelectedItem != null)
                {
                    object sel = dgSpares.SelectedItem;
                    result = ((SpareInInvoiceView)sel).id;
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
        #endregion

        public InvoiceEditView()
        {
            InitializeComponent();
            CreateInvoice();
        }
        public InvoiceEditView(int ID)
        {
            InitializeComponent();
            ItemToForm(ID);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAccounts();
            LoadBankAccounts();
            LoadOfferings();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DeleteNew)
                Delete();
        }
        private void btnSpareAdd_Click(object sender, RoutedEventArgs e)
        {                        
            AddNewOffering();
        }        
                
        private void btnSpareDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgSpares.SelectedItem == null)
            {
                MessageBox.Show("Сначала выберите деталь из списка");
                return;
            }
            DeleteSelectedOffering();
        }             
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            Print();                            
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            DeleteNew = false;
            FormToItem();
            Close();
        }      

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void edtAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadAccountData();
            LoadBankAccounts();
        }

        private void btnAccountCreateNew_Click(object sender, RoutedEventArgs e)
        {
            CreateNewAccount();
        }
                        
        private void btnBankAccountCreateNew_Click(object sender, RoutedEventArgs e)
        {
            CreateNewBankAccount();
        }        

        private void edtAccountBankNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadBankAccountData();
        }

        private void dgSpares_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
        }
        void CreateOutgoByInvoice()
        {
            da = new DataAccess();
            this.Item = FormToItem();
            bool WarningFlag = false;
            List<SpareInInvoiceView> items = da.GetSparesByInvoiceID(Item.id);
            foreach (SpareInInvoiceView siiv in items)
            {
                if (!siiv.IncomeID.HasValue)
                {
                    WarningFlag = true;
                }
            }
            if (WarningFlag)
            {
                MessageBox.Show("Вероятно, данный счет-фактура создан в предыдущей версии программы, поэтому не все товары могут быть перенесены в новую накладную.");
            }
            SpareOutgoEditView v = new SpareOutgoEditView(this.Item);            
            v.ShowDialog();
            //Close();
        }

        private void btnCreateOutgo_Click(object sender, RoutedEventArgs e)
        {
            CreateOutgoByInvoice();
        }

        private void btnAccountSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectView v = new SelectView();
            v.ClassName = (new account()).ToString();
            v.ShowDialog();
            LoadAccounts();
            if (v.Selected != null)
                edtAccount.SelectedValue = v.Selected._Id;            
        }

        private void btnBankSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectView v = new SelectView();
            v.ClassName = (new bank_account()).ToString();
            if(edtBankAccount.SelectedItem != null)
                v.ParentItemID = (int)edtAccount.SelectedValue;
            v.ShowDialog();
            LoadBankAccounts();
            if (v.Selected != null)
                edtBankAccount.SelectedValue = v.Selected._Id;
            else
                edtBankAccount.SelectedIndex = 0;
        }
    }
}
