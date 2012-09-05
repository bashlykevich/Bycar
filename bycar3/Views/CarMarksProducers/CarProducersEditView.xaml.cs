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

namespace bycar3.Views
{
    /// <summary>
    /// Interaction logic for CarProducersEditView.xaml
    /// </summary>
    public partial class CarProducersEditView : Window
    {
        public int _id = 0;
        public CarProducersEditView()
        {
            InitializeComponent();
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
            da.CarProducerEdit(getItemFromFields());
        }
        private void CreateItem()
        {
            DataAccess da = new DataAccess();
            da.CarProducerCreate(getItemFromFields());
        }
        car_producer getItemFromFields()
        {
            car_producer i = new car_producer();
            i.id = this._id;
            i.name = edtName.Text;
            i.descripton = edtDescr.Text;
            return i;
        }
        public void LoadItem(int id)
        {
            _id = id;
            DataAccess da = new DataAccess();
            car_producer b = da.GetCarProducer(id);
            edtName.Text = b.name;
            edtDescr.Text = b.descripton;
        }
    }
}
