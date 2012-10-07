using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using bycar;
using bycar3.External_Code;

namespace bycar3.Views.Overpricing
{
    /// <summary>
    /// Interaction logic for SpareInOverpricingSelectView.xaml
    /// </summary>
    public partial class SpareInOverpricingSelectView : Window
    {
        #region MEMBERS

        private DataAccess da;
        public int _ParentItemID = -1;
        public OverpricingEditView ParentWindow = null;

        #endregion MEMBERS

        public SpareInOverpricingSelectView()
        {
            InitializeComponent();
            da = new DataAccess();
        }

        #region CUSTOM FUNCTION

        private void LoadSpares()
        {
            dgSpares.DataContext = SpareContainer.Instance.Spares.Where(i => i.QRest > 0).ToList();
            dgSpares.SelectedIndex = 0;
        }

        private void SpareSearch()
        {
            if (edtSearchText != null)
            {
                if (edtSearchText.Text.Length < 1)
                    LoadSpares();
                else
                    LoadSpares(edtSearchField.SelectedIndex, edtSearchText.Text);
            }
        }

        private void LoadSpares(int SearchFieldIndex, string SearchText)
        {
            dgSpares.DataContext = SpareContainer.Instance.GetSpares(SearchFieldIndex, SearchText, true, 0, "");
            if (dgSpares.Items.Count > 0)
                dgSpares.SelectedIndex = 0;
        }

        private void AddSelectedSpareToOverpricing()
        {
            // получаем ID запчасти
            int SpareID = (dgSpares.SelectedItem as SpareView).id;

            // получаем список неизрасходованных поступлений данной запчасти
            List<SpareInSpareIncomeView> incomes = da.GetIncomes(SpareID);

            // каждое поступление прикрепляем к переоценке
            foreach (SpareInSpareIncomeView income in incomes)
            {
                // создаем новую запись в таблице переоценки
                spare_in_overpricing sio = new spare_in_overpricing();

                // наполняем данными
                sio.num = 0;
                sio.overpricing = da.OverpricingGet(_ParentItemID);
                sio.percentOld = (int)income.Markup;
                sio.priceOld = income.POut.Value;
                sio.purchasePrice = income.PIn.Value;
                sio.quantity = (int)income.QIn;
                sio.receiptDate = income.SpareIncomeDate;
                sio.spare = da.GetSpare(SpareID);
                sio.sumOld = income.S;
                sio.spare_in_spare_income = da.InOfferingGet(income.id);

                // сохраняем в БД
                da.OverpricingOfferingCreate(sio);
            }
            if (ParentWindow != null)
            {
                ParentWindow.LoadOfferings();
                ParentWindow.dgSpares.UpdateLayout();
                ParentWindow.dgSpares.ScrollIntoView(dgSpares.Items[dgSpares.Items.Count - 1]);
            }
        }

        #endregion CUSTOM FUNCTION

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSpares();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void dgIncomes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AddSelectedSpareToOverpricing();
        }

        private void edtSearchField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SpareSearch();
        }

        private void edtSearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            SpareSearch();
        }

        private void edtSearchText_TextInput(object sender, TextCompositionEventArgs e)
        {
            SpareSearch();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            edtSearchText.Focus();
        }

        private void SearchTextBox_Search(object sender, RoutedEventArgs e)
        {
            SpareSearch();
        }
    }
}