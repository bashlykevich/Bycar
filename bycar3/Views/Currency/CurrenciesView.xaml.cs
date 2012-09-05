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
using bycar3.Views.Currency;

namespace bycar3.Views
{
    /// <summary>
    /// Interaction logic for CurrenciesView.xaml
    /// </summary>
    public partial class CurrenciesView : Window
    {
        int SelectedCurrencyID = 0;
        string SelectedCurrencyCode = "";

        // CUSTOM FUNCTIONS
        private void LoadRates()
        {
            if (dgList.SelectedItem != null)
            {
                SelectedCurrencyID = (dgList.SelectedItem as currency).id;
                SelectedCurrencyCode = (dgList.SelectedItem as currency).code;

                DataAccess da = new DataAccess();
                //dgRates.DataContext = da.GetCurrencyRates(SelectedCurrencyID);
            }
        }
        void RateAdd()
        {
            if (dgList.SelectedItem != null)
            {
                CurrencyRateEditView v = new CurrencyRateEditView();
                v.CurrencyID = SelectedCurrencyID;
                v.CurrencyName = SelectedCurrencyCode;
                v.BasicCurrencyName = BasicCurencyCode();
                v.ShowDialog();
                LoadRates();
            } else
            {
                MessageBox.Show("Выберите валютиу из списка!");
            }
        }
        void RateEdit()
        {            
            if (dgRates.SelectedItem != null)
            {
                int RateID = (dgRates.SelectedItem as currency_rate).id;                
                CurrencyRateEditView v = new CurrencyRateEditView();
                v.RateID = RateID;
                v.CurrencyID = SelectedCurrencyID;
                v.CurrencyName = SelectedCurrencyCode;
                v.BasicCurrencyName = BasicCurencyCode();
                v.Rate = (float)(dgRates.SelectedItem as currency_rate).rate;
                v.CurrencyID = SelectedCurrencyID;
                v.ShowDialog();
                LoadRates();
            }
            else
            {
                MessageBox.Show("Выберите запись для редактирования!");
            }
        }
        void RateDelete()
        {
            DataAccess da = new DataAccess();
            if(dgRates.SelectedItem != null)
            {
                int RateID = (dgRates.SelectedItem as currency_rate).id;
                da.CurrencyRateDelete(RateID);
                LoadRates();
            } else
            {
                MessageBox.Show("Выберите запись для удаления!");
            }
        }

        string BasicCurencyCode()
        {            
            DataAccess da = new DataAccess();
            return da.getBasicCurrencyCode();            
        }
        // HANDLERS
        public CurrenciesView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadList();
        }
        private void ReloadList()
        {
            DataAccess da = new DataAccess();
            this.DataContext = da.GetCurrency();
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
        void DeleteItem()
        {
            int id = 0;
            currency b = null;
            if (dgList.SelectedItem != null)
            {
                object sel = dgList.SelectedItem;
                b = (currency)sel;
                id = b.id;
            }
            if (id > 0)
            {
                MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выделенную запись?", "Удаление!", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    DataAccess da = new DataAccess();
                    da.CurrencyDelete(id);
                    ReloadList();
                }
            }
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteItem();
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
            CurrenciesEditView v = new CurrenciesEditView();
            v._id = -1;
            v.ShowDialog();
        }
        private void EditSelectedItem()
        {
            int id = 0;
            currency b = null;
            if (dgList.SelectedItem != null)
            {
                object sel = dgList.SelectedItem;
                b = (currency)sel;
                id = b.id;
            }
            if (id > 0)
            {
                CurrenciesEditView v = new CurrenciesEditView();
                v._id = b.id;
                v.edtName.Text = b.name;
                v.edtCode.Text = b.code;
                v.edtShortName.Text = b.short_name;
                v.edtBasic.IsChecked = b.is_basic != 1 ? false : true;
                v.ShowDialog();
                ReloadList();
            }
        }

        private void dgRates_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RateEdit();
        }

        private void btnRateAdd_Click(object sender, RoutedEventArgs e)
        {
            RateAdd();
        }

        private void btnRateEdit_Click(object sender, RoutedEventArgs e)
        {
            RateEdit();
        }

        private void btnRateDelete_Click(object sender, RoutedEventArgs e)
        {
            RateDelete();
        }

        private void dgList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadRates();
        }
    }
}
