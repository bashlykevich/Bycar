using System.Windows;
using bycar;

namespace bycar3.Views
{
    /// <summary>
    /// Interaction logic for WarehousesEditView.xaml
    /// </summary>
    public partial class WarehousesEditView : Window
    {
        public int _id = 0;

        public WarehousesEditView()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void LoadItem(int id)
        {
            _id = id;
            DataAccess da = new DataAccess();
            warehouse b = da.GetWarehouse(id);
            edtName.Text = b.name;
            edtDescr.Text = b.description;
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
            da.WarehouseEdit(getItemFromFields());
        }

        private void CreateItem()
        {
            DataAccess da = new DataAccess();
            da.WarehouseCreate(getItemFromFields());
        }

        private warehouse getItemFromFields()
        {
            warehouse item = new warehouse();
            item.id = this._id;
            item.name = edtName.Text;
            item.description = edtDescr.Text;
            return item;
        }
    }
}