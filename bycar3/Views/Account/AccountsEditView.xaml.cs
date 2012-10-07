using System.Windows;
using bycar;
using bycar3.Views.Account;

namespace bycar3.Views
{
    /// <summary>
    /// Interaction logic for AccountsEditView.xaml
    /// </summary>
    public partial class AccountsEditView : Window
    {
        public int _id = 0;

        public AccountsEditView()
        {
            InitializeComponent();
            LoadBankAccounts();
        }

        public void LoadItem(int id)
        {
            DataAccess da = new DataAccess();
            account b = da.GetAccount(id);
            this._id = id;
            edtName.Text = b.name;
            edtDescr.Text = b.description;
            edtAddress.Text = b.address;
            edtDiscount.Text = b.discount.ToString();
            edtOKPO.Text = b.okpo.ToString();

            //edtScore.Text = b.bank_score;
            edtShippingBase.Text = b.shipping_base;
            edtShippingDest.Text = b.shipping_point;

            // edtType.SelectedItem = b.account_type.name;
            //edtBank.SelectedItem = b.bank.name;
            edtUNN.Text = b.unn;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (this._id > 0)
                EditItem();
            else
                CreateItem();

            this.Close();
        }

        private void EditItem()
        {
            DataAccess da = new DataAccess();

            //string bankName = edtBank.SelectedItem.ToString();
            //string typeName = edtType.SelectedItem.ToString();
            //da.AccountEdit(getItemFromFields(), bankName, typeName);
            da.AccountEdit(getItemFromFields());
        }

        private void CreateItem()
        {
            DataAccess da = new DataAccess();

            //string bankName = edtBank.SelectedItem.ToString();
            //string typeName = edtType.SelectedItem.ToString();
            //_id = da.AccountCreate(getItemFromFields(), bankName, typeName);\
            _id = da.AccountCreate(getItemFromFields());
        }

        private account getItemFromFields()
        {
            account item = new account();
            item.id = this._id;
            item.name = edtName.Text;
            item.description = edtDescr.Text;
            item.unn = edtUNN.Text;
            item.shipping_base = edtShippingBase.Text;
            item.shipping_point = edtShippingDest.Text;
            item.okpo = edtOKPO.Text;
            item.discount = 0;
            item.address = edtAddress.Text;

            //item.bank_score = edtScore.Text;
            return item;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LoadBankAccounts()
        {
            if (this._id > 0)
            {
                DataAccess da = new DataAccess();
                dgSpares.DataContext = da.BankAccountViewGet(this._id);
            }
        }

        private void BankAccountAdd()
        {
            if (_id < 1)
            {
                CreateItem();
            }
            BankAccountEditView v = new BankAccountEditView(this._id);
            v.ShowDialog();
            LoadBankAccounts();
        }

        private void BankAccountDelete()
        {
            DataAccess da = new DataAccess();
            if ((dgSpares.SelectedItem as BankAccountView) != null)
            {
                da.BankAccountDelete((dgSpares.SelectedItem as BankAccountView).id);
                LoadBankAccounts();
            }
        }

        private void btnBAAdd_Click(object sender, RoutedEventArgs e)
        {
            BankAccountAdd();
        }

        private void btnBADelete_Click(object sender, RoutedEventArgs e)
        {
            BankAccountDelete();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBankAccounts();
        }
    }
}