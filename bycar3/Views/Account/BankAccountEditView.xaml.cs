using System;
using System.Windows;
using bycar;
using bycar3.Views.Common;

namespace bycar3.Views.Account
{
    /// <summary>
    /// Interaction logic for BankAccountEditView.xaml
    /// </summary>
    public partial class BankAccountEditView : Window
    {
        public int AccountID = 0;
        private BankAccountView Item = null;

        public BankAccountEditView(int AccID)
        {
            InitializeComponent();
            AccountID = AccID;
        }

        public BankAccountEditView()
        {
            InitializeComponent();
        }

        private void LoadBanks()
        {
            DataAccess da = new DataAccess();
            edtBank.DataContext = da.GetAllBanks();
            edtBank.SelectedIndex = 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBanks();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Item == null)
                {
                    DataAccess da = new DataAccess();
                    bank_account ba = new bank_account();
                    ba.account = da.GetAccount(AccountID);
                    int BankID = (edtBank.SelectedItem as bank).id;
                    ba.bank = da.BankGet(BankID);
                    ba.BankAccount = edtAccountNumber.Text;
                    ba.Description = edtDescription.Text;
                    AccountID = da.BankAccountCreate(ba).id;
                }
                else
                {
                    DataAccess da = new DataAccess();
                    bank_account ba = da.bank_account_get(Item.id);
                    int BankID = (edtBank.SelectedItem as bank).id;
                    ba.bank = da.BankGet(BankID);
                    ba.BankAccount = edtAccountNumber.Text;
                    ba.Description = edtDescription.Text;
                    da.BankAccountEdit(ba);
                }
            }
            catch (Exception)
            {
            }
            Close();
        }

        private void btnBankSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectView v = new SelectView();
            v.ClassName = (new bank()).ToString();
            v.ShowDialog();
            LoadBanks();
            if (v.Selected != null)
                edtBank.SelectedValue = v.Selected._Id;
        }

        public void LoadItem(int id)
        {
            DataAccess da = new DataAccess();
            Item = da.BankAccountView(id);
            edtAccountNumber.Text = Item.BankAccount;
            edtBank.SelectedValue = Item.BankID;
            edtDescription.Text = Item.Description;
        }
    }
}