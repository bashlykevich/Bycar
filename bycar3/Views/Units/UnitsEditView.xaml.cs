using System.Windows;
using bycar;

namespace bycar3.Views
{
    /// <summary>
    /// Interaction logic for UnitsEditView.xaml
    /// </summary>
    public partial class UnitsEditView : Window
    {
        public int _id = 0;

        public UnitsEditView()
        {
            InitializeComponent();
        }

        public void LoadItem(int id)
        {
            _id = id;
            DataAccess da = new DataAccess();
            unit b = da.GetUnit(id);
            edtName.Text = b.name;
            edtDescr.Text = b.description;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
            da.UnitEdit(getItem());
        }

        private void CreateItem()
        {
            DataAccess da = new DataAccess();
            da.UnitCreate(getItem());
        }

        private unit getItem()
        {
            unit item = new unit();
            item.id = this._id;
            item.name = edtName.Text;
            item.description = edtDescr.Text;
            return item;
        }
    }
}