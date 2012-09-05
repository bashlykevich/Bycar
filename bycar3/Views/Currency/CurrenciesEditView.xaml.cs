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
    /// Interaction logic for CurrenciesEditView.xaml
    /// </summary>
    public partial class CurrenciesEditView : Window
    {
        public int _id = 0;
        bool wasBasic = false;
        public CurrenciesEditView()
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
            if (wasBasic != edtBasic.IsChecked.Value && edtBasic.IsChecked.Value == true)
                RecalulateBasics();
            else
            {
                DataAccess da = new DataAccess();
                da.CurrencyEdit(getItemFromFields());
            }
        }
        private void CreateItem()
        {
            if (wasBasic != edtBasic.IsChecked.Value && edtBasic.IsChecked.Value == true)
                RecalulateBasics();
            else
            {
                DataAccess da = new DataAccess();
                da.CurrencyCreate(getItemFromFields());
            }
        }
        void RecalulateBasics()
        {
            MessageBoxResult res = MessageBox.Show("Вы действительно хотите изменить базовую валюту? (Будут обновлены курсы)", "Изменение базовой валюты!", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                DataAccess da= new DataAccess();
                string OldBasicCode = da.getBasicCurrencyCode();
                // изменить базовую валюту
                da.CurrencyEdit(getItemFromFields());
                // загрузить курсы валют относительно новой валюты
                CurrenciesInput v = new CurrenciesInput();
                v.ShowDialog();
                // пересчитать приходы                
                da.RecalculateBasics(OldBasicCode);
                MessageBox.Show("Пересчет завершен! Базовая валюта изменена.");
            }
            else
                edtBasic.IsChecked = false;
        }
        currency getItemFromFields()
        {
            currency item = new currency();
            item.id = _id;
            item.name = edtName.Text;
            item.code = edtCode.Text;
            item.short_name = edtShortName.Text;
            item.is_basic = edtBasic.IsChecked.HasValue?(edtBasic.IsChecked.Value?1:0):0;
            return item;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            wasBasic = edtBasic.IsChecked.Value;
        }        
    }
}
