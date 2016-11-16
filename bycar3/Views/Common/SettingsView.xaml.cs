using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using bycar;
using bycar3.Core;
using bycar3.Views.Admin_Units;

namespace bycar3.Views.Common
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : Window
    {
        private DataAccess da = new DataAccess();
        private settings_profile profile = null;
        private string CurrencyCodeOld = "";
        private string CurrencyCodeOld_IncomeDef = "";

        // CUSTOM FUNCTIONS
        private void LoadBankAccounts()
        {
            da = new DataAccess();
            dgItems.DataContext = da.ProfileBankAccountViewGet();
        }

        private void LoadUsers()
        {
            DataAccess db = new DataAccess();
            dgUsers.DataContext = db.GetAdminUnits();
        }

        private void LoadSearchFields()
        {
            edtDefaultSearchField.Items.Add("Код магазина");
            edtDefaultSearchField.Items.Add("Наименование");
            edtDefaultSearchField.Items.Add("Код");
        }

        private void AddBankAccount()
        {
            ProfileBankAccountDialog v = new ProfileBankAccountDialog();
            v.ShowDialog();
            LoadBankAccounts();
        }

        private void DeleteBankAccount()
        {
            if (dgItems.SelectedItem != null)
            {
                if (dgItems.SelectedItem as ProfileBankAccountView != null)
                {
                    MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выделенную запись?", "Удаление", MessageBoxButton.YesNo);
                    if (res == MessageBoxResult.Yes)
                    {
                        int ID = (dgItems.SelectedItem as ProfileBankAccountView).id;
                        da.ProfileBankAccountDelete(ID);
                        LoadBankAccounts();
                    }
                }
            }
        }

        private void EditBankAccount()
        {
            try
            {
                if (dgItems.SelectedItem != null)
                {
                    int ID = (dgItems.SelectedItem as ProfileBankAccountView).id;
                    ProfileBankAccountDialog v = new ProfileBankAccountDialog(ID);
                    v.ShowDialog();
                    LoadBankAccounts();
                }
            }
            catch (Exception)
            { }
        }

        private void LoadCurrencies()
        {
            edtBasicCurrency.Items.Clear();
            edtDefaultIncomeCurrency.Items.Clear();
            List<currency> items = da.GetCurrency();
            foreach (currency i in items)
            {
                edtBasicCurrency.Items.Add(i.code);
                edtDefaultIncomeCurrency.Items.Add(i.code);
            }
            edtBasicCurrency.SelectedItem = da.getBasicCurrencyCode();
            edtDefaultIncomeCurrency.SelectedItem = profile.DefaultIncomeCurrencyCode;
        }

        private void SaveBasicCurrency()
        {
            RecalulateBasics();
        }

        private void RecalulateBasics()
        {
            MessageBoxResult res = MessageBox.Show("Вы действительно хотите изменить базовую валюту? (Будут обновлены курсы)", "Изменение базовой валюты!", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                DataAccess da = new DataAccess();
                string OldBasicCode = da.getBasicCurrencyCode();

                // изменить базовую валюту
                da.UpdateBasicCurrencyCode(edtBasicCurrency.SelectedItem.ToString());

                // пересчитать приходы
                da.RecalculateBasics(OldBasicCode);
                MessageBox.Show("Пересчет завершен! Базовая валюта изменена.");
            }
        }

        private void LoadCurrentProfile()
        {
            if (profile == null)
            {
                profile = da.getProfileCurrent();
            }
            profile.DefaultIncomeCurrencyCode = profile.DefaultIncomeCurrencyCode.Replace(" ", String.Empty);
            edtAddress.Text = profile.AddressFact;
            edtName.Text = profile.CompanyName;

            //edtDefaultIsCashless.IsChecked = profile.DefaultIsCashless == 1 ? true : false;
            edtUNN.Text = profile.UNN;
            LoadCurrencies();
            CurrencyCodeOld = edtBasicCurrency.SelectedItem.ToString();
            CurrencyCodeOld_IncomeDef = profile.DefaultIncomeCurrencyCode;
            edtDefaultIncomeCurrency.SelectedItem = profile.DefaultIncomeCurrencyCode;

            edtAccountant.Text = profile.SeniorAccountant;
            edtCompanyHead.Text = profile.CompanyHead;
            edtJAddress.Text = profile.AddressJur;
            edtLoadPoint.Text = profile.LoadPoint;
            edtOKPO.Text = profile.OKPO;

            edtDefaultSearchField.SelectedIndex = profile.DefSearchFieldIndex.HasValue ? profile.DefSearchFieldIndex.Value : 0;
            edtSearchTypeInMainWindow.SelectedIndex = profile.UseScanner.HasValue ? profile.UseScanner.Value : 0;

            //edtBankAccount.Text = profile.BankAccount;
            // edtBankAddress.Text = profile.BankAddress;
            // edtBankMFO.Text = profile.BankMFO;
            // edtBankName.Text = profile.BankName;
        }

        private void SaveProfile()
        {
            profile.CompanyName = edtName.Text;
            profile.UNN = edtUNN.Text;

            //profile.DefaultIsCashless = edtDefaultIsCashless.IsChecked.Value?1:0;
            profile.SeniorAccountant = edtAccountant.Text;
            profile.CompanyHead = edtCompanyHead.Text;
            profile.AddressJur = edtJAddress.Text;
            profile.LoadPoint = edtLoadPoint.Text;
            profile.OKPO = edtOKPO.Text;

            //  profile.BankAccount = edtBankAccount.Text;
            //  profile.BankAddress = edtBankAddress.Text;
            //  profile.BankMFO = edtBankMFO.Text;
            //  profile.BankName = edtBankName.Text;

            profile.DefSearchFieldIndex = edtDefaultSearchField.SelectedIndex;
            profile.UseScanner = edtSearchTypeInMainWindow.SelectedIndex;

            profile.AddressFact = edtAddress.Text;
            profile.BasicCurrencyCode = edtBasicCurrency.SelectedItem.ToString();
            profile.DefaultIncomeCurrencyCode = edtDefaultIncomeCurrency.SelectedItem.ToString();
            if (CurrencyCodeOld != edtBasicCurrency.SelectedItem.ToString())
                SaveBasicCurrency();
            da.ProfileEdit(profile);
        }

        private string FixAnalogues()
        {
            int cntTotal = 0;
            int cntEqual = 0;
            int cntFixed = 0;
            da = new DataAccess();
            da.FixAnalogues();

            string Mess = "В таблице АНАЛОГИ ";
            Mess += cntTotal.ToString() + " записей;\nисправлено - " + cntFixed + " записей;\nвсего взаимных - " + cntEqual;
            return Mess;
        }

        // HANDLERS
        public SettingsView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCurrentProfile();
            LoadBankAccounts();
            LoadSearchFields();
            LoadUsers();
            if (Marvin.Instance.CurrentUser.is_admin != 1)
            {
                spButtons.Visibility = System.Windows.Visibility.Collapsed;
                dgUsers.IsEnabled = false;
            }
            /*
            {
                btnUserAdd.IsEnabled = true;
                btnUserEdit.IsEnabled = true;
                btnUserDelete.IsEnabled = true;
                dgUsers.IsEnabled = true;
            }*/
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveProfile();
            Close();
        }

        private void btnFixAnalogues_Click(object sender, RoutedEventArgs e)
        {
            string Mess = FixAnalogues();
            MessageBox.Show(Mess);
            Close();
        }

        private void btnBAAdd_Click(object sender, RoutedEventArgs e)
        {
            AddBankAccount();
        }

        private void btnBADelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteBankAccount();
        }

        private void btnBAEdit_Click(object sender, RoutedEventArgs e)
        {
            EditBankAccount();
        }

        private void dgItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditBankAccount();
        }

        private void btnUserDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteUsers();
        }

        private void btnUserEdit_Click(object sender, RoutedEventArgs e)
        {
            EditUser();
        }

        private void CreateUser()
        {
            UserEditView v = new UserEditView();
            v.ShowDialog();
            LoadUsers();
        }

        private void EditUser()
        {
            if (dgUsers.SelectedItem == null)
                return;
            UserEditView v = new UserEditView((dgUsers.SelectedItem as admin_unit).id);
            v.ShowDialog();
            LoadUsers();
        }

        private void DeleteUsers()
        {
            if (dgUsers.SelectedItems.Count > 0)
            {
                MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выделенные записи?", "Удаление", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    DataAccess db = new DataAccess();
                    if (db.AdminUnitsCount() == dgUsers.SelectedItems.Count)
                    {
                        MessageBox.Show("Нельзя удалить всех пользователей! Должен остаться хотя бы один.");
                        return;
                    }
                    foreach (admin_unit user in dgUsers.SelectedItems)
                        db.AdminUnitDelete(user.id);
                    LoadUsers();
                }
            }
        }

        private void btnUserAdd_Click(object sender, RoutedEventArgs e)
        {
            CreateUser();
        }

        private void dgUsers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditUser();
        }
    }
}