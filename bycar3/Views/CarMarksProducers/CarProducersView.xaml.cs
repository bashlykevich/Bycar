﻿using System;
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
    /// Interaction logic for CarProducersView.xaml
    /// </summary>
    public partial class CarProducersView : Window
    {
        public CarProducersView()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            CreateItem();
            ReloadList();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            EditSelectedItem();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteItem();
        }
        void DeleteItem()
        {
            int id = 0;
            car_producer b = null;
            if (dgList.SelectedItem != null)
            {
                object sel = dgList.SelectedItem;
                b = (car_producer)sel;
                id = b.id;
            }
            if (id > 0)
            {
                MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выделенную запись?", "Удаление!", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    DataAccess da = new DataAccess();
                    da.CarProducerDelete(id);
                    ReloadList();
                }
            }
        }
        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                EditSelectedItem();
            }
            catch (Exception)
            {
                ReloadList();
            };
        }
        private void CreateItem()
        {
            CarProducersEditView v = new CarProducersEditView();
            v._id = -1;
            v.ShowDialog();
        }
        private void EditSelectedItem()
        {
            int id = 0;
            car_producer b = null;
            if (dgList.SelectedItem != null)
            {
                object sel = dgList.SelectedItem;
                b = (car_producer)sel;
                id = b.id;
            }
            if (id > 0)
            {
                CarProducersEditView v = new CarProducersEditView();
                v._id = b.id;
                v.edtName.Text = b.name;
                v.edtDescr.Text = b.descripton;
                v.ShowDialog();
                ReloadList();
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadList();
        }
        private void ReloadList()
        {
            DataAccess da = new DataAccess();
            this.DataContext = da.GetCarProducers();
        }
    }
}
