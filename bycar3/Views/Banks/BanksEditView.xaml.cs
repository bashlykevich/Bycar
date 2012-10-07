using System.Windows;
using bycar;

namespace bycar3.Views
{
    public partial class BanksEditView : Window
    {
        public int _ID = 0;

        public BanksEditView()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (this._ID > 0)
                EditBank();
            else
                CreateBank();

            this.Close();
        }

        public void LoadItem(int id)
        {
            _ID = id;
            DataAccess da = new DataAccess();
            bank b = da.BankGet(id);
            edtName.Text = b.name;
            edtFax.Text = b.fax;
            edtAddress.Text = b.address;
            edtMFO.Text = b.mfo;
            edtPhone.Text = b.phone;
        }

        private void EditBank()
        {
            DataAccess da = new DataAccess();
            bank item = new bank();
            item.id = this._ID;
            item.mfo = edtMFO.Text;
            item.fax = edtFax.Text;
            item.address = edtAddress.Text;
            item.name = edtName.Text;
            item.phone = edtPhone.Text;
            da.BankEdit(item);
        }

        private void CreateBank()
        {
            DataAccess da = new DataAccess();
            bank item = new bank();
            item.mfo = edtMFO.Text;
            item.fax = edtFax.Text;
            item.address = edtAddress.Text;
            item.name = edtName.Text;
            item.phone = edtPhone.Text;
            da.BankCreate(item);
        }
    }
}