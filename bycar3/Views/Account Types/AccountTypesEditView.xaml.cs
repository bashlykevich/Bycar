using System.Windows;
using bycar;

namespace bycar3.Views
{
    /// <summary>
    /// Interaction logic for AccountTypesEditView.xaml
    /// </summary>
    public partial class AccountTypesEditView : Window
    {
        public int _id = 0;

        public AccountTypesEditView()
        {
            InitializeComponent();
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
            account_type item = new account_type();
            item.id = _id;
            item.name = edtName.Text;
            item.description = edtDescr.Text;
            da.AccountTypeEdit(item);
        }

        private void CreateItem()
        {
            DataAccess da = new DataAccess();
            account_type item = new account_type();
            item.name = edtName.Text;
            item.description = edtDescr.Text;
            da.AccountTypeCreate(item);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}