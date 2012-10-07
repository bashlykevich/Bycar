using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using bycar;

namespace bycar3.Views
{
    /// <summary>
    /// Interaction logic for ClientsEditView.xaml
    /// </summary>
    public partial class ClientsEditView : Window
    {
        public int _id = 0;

        public ClientsEditView()
        {
            InitializeComponent();
            LoadComboBox_CarMarks();
            LoadComboBox_CarProducers();
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
            client item = new client();
            item.id = _id;
            item.name = edtName.Text;
            item.description = edtDescr.Text;
            da.ClientEdit(item, cbCar.SelectedItem.ToString());
        }

        private void CreateItem()
        {
            DataAccess da = new DataAccess();
            client item = new client();
            item.name = edtName.Text;
            item.description = edtDescr.Text;
            da.ClientCreate(item, cbCar.SelectedItem.ToString());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void LoadComboBox_CarMarks()
        {
            DataAccess da = new DataAccess();
            List<car_mark> marks = da.GetCarMarks();
            foreach (car_mark m in marks)
            {
                cbCar.Items.Add(m.name);
            }
            cbCar.SelectedIndex = 0;
        }

        private void LoadComboBox_CarProducers()
        {
            DataAccess da = new DataAccess();
            List<car_producer> cars = da.GetCarProducers();
            foreach (car_producer m in cars)
            {
                cbProds.Items.Add(m.name);
            }
            cbProds.SelectedIndex = 0;
        }
    }
}