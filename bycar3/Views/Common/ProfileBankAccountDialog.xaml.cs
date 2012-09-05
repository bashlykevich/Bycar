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

namespace bycar3.Views.Common
{
    /// <summary>
    /// Interaction logic for ProfileBankAccountDialog.xaml
    /// </summary>
    public partial class ProfileBankAccountDialog : Window
    {
        DataAccess da = new DataAccess();
        ProfileBankAccount Item = null;
        public ProfileBankAccountDialog()
        {
            InitializeComponent();
        }
        public ProfileBankAccountDialog(int BankAccountID)
        {
            InitializeComponent();          
            Item = da.ProfileBankAccountGet(BankAccountID);
        }
        void LoadBanks()
        {            
            edtBank.DataContext = da.GetAllBanks();
            edtBank.SelectedIndex = 0;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBanks();
            if (Item != null)
            {
                try
                {
                    if (Item.bank == null)
                        Item.bankReference.Load();
                    edtBank.SelectedValue = Item.bank.id;
                }
                catch (Exception)
                {
                }
                edtBankAccount.Text = Item.BankAccount;
                edtDescription.Text = Item.Description;
                edtIsMain.IsChecked = Item.IsMain == 1;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //try
            {                
                if (Item == null)
                {                    
                    ProfileBankAccount ba = new ProfileBankAccount();
                    //ba.account = da.GetAccount(AccountID);
                    int BankID = (edtBank.SelectedItem as bank).id;
                    ba.bank = da.BankGet(BankID);
                    ba.BankAccount = edtBankAccount.Text;
                    ba.Description = edtDescription.Text;
                    ba.IsMain = edtIsMain.IsChecked.Value ? 1 : 0;
                    //AccountID = da.BankAccountCreate(ba).id;
                    da.ProfileBankAccountCreate(ba);
                }
                else
                {                                                            
                    int BankID = (edtBank.SelectedItem as bank).id;
                    Item.bank = da.BankGet(BankID);
                    Item.BankAccount = edtBankAccount.Text;
                    Item.Description = edtDescription.Text;
                    Item.IsMain = 0;
                    if(edtIsMain.IsChecked.HasValue)
                        if(edtIsMain.IsChecked.Value)
                            Item.IsMain = 1;

                    //AccountID = da.BankAccountCreate(ba).id;
                    da.ProfileBankAccountEdit(Item);
                }
            }
            //catch (Exception)
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
            if(edtBank.SelectedValue != null)
                edtBank.SelectedValue = v.Selected._Id;
        }
    }
}
