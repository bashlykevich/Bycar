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
using bycar3.Views.Common;

namespace bycar3.Views
{
    /// <summary>
    /// Interaction logic for CarMarksEditView.xaml
    /// </summary>
    public partial class CarMarksEditView : Window
    {
        public int _id = 0;
        public CarMarksEditView()
        {
            InitializeComponent();
            loadComboBox_CarProducers();
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
            da.CarMarkEdit(getItemFromFields(), edtProducer.SelectedItem.ToString());
        }
        private void CreateItem()
        {
            DataAccess da = new DataAccess();            
            da.CarMarkCreate(getItemFromFields(), edtProducer.SelectedItem.ToString());
        }
        car_mark getItemFromFields()
        {
            car_mark item = new car_mark();
            item.id = this._id;
            item.name = edtName.Text;
            item.description = edtDescr.Text;
            return item;
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        void loadComboBox_CarProducers()
        {
            edtProducer.Items.Clear();
            edtProducer.UpdateLayout();
            DataAccess da = new DataAccess();
            List<car_producer> items = da.GetCarProducers();
            foreach(car_producer i in items)
            {
                edtProducer.Items.Add(i.name);
            }
            edtProducer.SelectedIndex = 0;
        }

        private void btnItemSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectView v = new SelectView();
            v.ClassName = (new car_producer()).ToString();
            v.ShowDialog();
            loadComboBox_CarProducers();
            edtProducer.SelectedItem = v.RES;
        }
    }
}
