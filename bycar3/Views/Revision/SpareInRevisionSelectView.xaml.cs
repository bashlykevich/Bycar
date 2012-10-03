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
using bycar3.External_Code;
using bycar3.Core;

namespace bycar3.Views.Revision
{
    /// <summary>
    /// Interaction logic for SpareInRevisionSelectView.xaml
    /// </summary>
    public partial class SpareInRevisionSelectView : Window
    {
        #region MEMBERS

        public RevisionEditView _ParentWindow = null;
        DataAccess da = new DataAccess();
        public int _SpareIncomeID = -1;
        public int _OfferingNumber = 0;
        public string CurrencyRateCode;
        #endregion

        #region FUNCTIONS

        string GetSelectedSpareName()
        {
            string result = "";
            try
            {
                if (dgSpares.SelectedItem != null)
                {
                    object sel = dgSpares.SelectedItem;
                    result = ((SpareView)sel).name;
                }
                else
                {
                    MessageBox.Show("Сначала выберите деталь из списка");
                }
            }
            catch (Exception)
            {
                LoadSpares();
                return "";
            }
            return result;
        }

        int GetSelectedSpareId()
        {
            int result = 0;
            try
            {
                if (dgSpares.SelectedItem != null)
                {
                    object sel = dgSpares.SelectedItem;
                    result = ((SpareView)sel).id;
                }
                else
                {
                    MessageBox.Show("Сначала выберите деталь из списка");
                }
            }
            catch (Exception)
            {
                LoadSpares();
                return -1;
            }
            return result;
        }

        void LoadSpares()
        {
            LoadSpares(null, "");
        }

        void LoadSpares(int? searchFieldIndex, string searchString)
        {
            da = new DataAccess();
            List<SpareView> items = new List<SpareView>();
            if (searchFieldIndex.HasValue && !searchString.Equals(""))
            {
                items = SpareContainer.Instance.GetSpares(searchFieldIndex.Value, searchString);
            }
            else
            {
                items = SpareContainer.Instance.Spares;
            }
            dgSpares.DataContext = items;
        }

        void SpareSearch()
        {
            if (edtSearchText != null)
            {
                string searchString = edtSearchText.Text;
                int searchFieldIndex = edtSearchField.SelectedIndex;
                LoadSpares(searchFieldIndex, searchString);
            }
        }
               
        void SpareAdd()
        {
            Marvin.Instance.SpareCreate();
            LoadSpares();
        }

        void SpareEdit()
        {
            if (dgSpares.SelectedItem == null)
                return;
            int SpareID = (dgSpares.SelectedItem as SpareView).id;
            Marvin.Instance.SpareEdit(SpareID);
            LoadSpares();
        }

        void SpareDelete()
        {
            int id = 0;
            SpareView b = null;
            if (dgSpares.SelectedItem != null)
            {
                object sel = dgSpares.SelectedItem;
                b = (SpareView)sel;
                id = b.id;
            }
            if (id > 0)
            {
                MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выделенную запись?", "Удаление!", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    DataAccess da = new DataAccess();
                    da.SpareDelete(id);
                    LoadSpares();
                }
            }
        }

        void ShowSelectedSpareName()
        {
            edtOfferingName.Content = GetSelectedSpareName();
        }

        void SearchFieldOnMouseEnter()
        {
            if (edtSearchText.Text.Equals("Введите текст..."))
            {
                edtSearchText.Text = "";
                edtSearchText.FontStyle = FontStyles.Normal;
                edtSearchText.FontWeight = FontWeights.Bold;
                edtSearchText.Foreground = Brushes.Black;
            }
        }

        void SearchFieldOnKeyPressed(KeyEventArgs e)
        {
            string searchString = edtSearchText.Text;
            switch (e.Key)
            {
                case Key.Enter:
                    if (searchString.Length == 0)
                    {
                        MessageBox.Show("Вы не задали строку поиска!");
                    }
                    else
                    {
                        SpareSearch();
                    }
                    break;
            }
        }

        void AddNewOffering()
        {
            string _SpareName = GetSelectedSpareName();
            int _SpareID = GetSelectedSpareId();
            // проверить, есть ли списке введенных данный товар
            bool InRevisionExist = da.GetRemains(_SpareIncomeID).Where(x => x.SpareID == _SpareID).Count()>0;
            // если есть, спросить, добавить ли его новой позицией или приплюсовать 1 шт. к уже внесенному
            if (InRevisionExist)
            {                

                string QM = "Добавить отдельной позицией?";
                // (ДА - появится новая строчка с данным товаром; НЕТ - добавится 1 шт. к уже существущей строчке с товаром)?";
                if (MessageBox.Show(QM, "Есть такая деталь в списке!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    // добавить новой позицией
                    spare_in_spare_income offering = new spare_in_spare_income();
                    offering.id = -1;
                    offering.Markup = 1;
                    offering.num = ++_OfferingNumber;
                    offering.PIn = 1;
                    offering.QIn = 1;
                    offering.POut = 1;
                    offering.S = 1;
                    
                    offering.PInBasic = 0;
                    offering.POutBasic = 0;
                    offering.SBasic = 0;

                    if (CurrencyRateCode == "BYR")
                        offering.CurrencyID = 1;
                    else
                        if (CurrencyRateCode == "USD")
                            offering.CurrencyID = 2;
                        else
                            if (CurrencyRateCode == "EUR")
                                offering.CurrencyID = 3;
                            else
                                if (CurrencyRateCode == "RUR")
                                    offering.CurrencyID = 4;

                    string VATName = "Без НДС";
                    da.InOfferingCreate(offering, _SpareID, _SpareIncomeID, VATName);
                    SpareContainer.Instance.Update(_SpareID);
                }
                else
                {
                    // приплюсовать штуку к уже внесенному
                    //#TODO
                    // пока не реализовано
                }
            }
            else
            {
                spare_in_spare_income offering = new spare_in_spare_income();
                offering.id = -1;
                offering.Markup = 1;
                offering.num = ++_OfferingNumber;
                offering.PIn = 1;
                offering.QIn = 1;
                offering.POut = 1;
                offering.S = 1;

                offering.PInBasic = 0;
                offering.POutBasic = 0;
                offering.SBasic = 0;

                string VATName = "Без НДС";

                if(CurrencyRateCode == "BYR")
                    offering.CurrencyID = 1; else
                    if (CurrencyRateCode == "USD")
                    offering.CurrencyID = 2;
                else
                        if (CurrencyRateCode == "EUR")
                    offering.CurrencyID = 3;
                else
                            if (CurrencyRateCode == "RUR")
                    offering.CurrencyID = 4;

                da.InOfferingCreate(offering, _SpareID, _SpareIncomeID, VATName);
                SpareContainer.Instance.Update(_SpareID);
            }            
        }
        #endregion

        // HANDLERS
        public SpareInRevisionSelectView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSpares();
        }

        private void edtSearchText_MouseEnter(object sender, MouseEventArgs e)
        {
            SearchFieldOnMouseEnter();
        }

        private void edtSearchText_KeyDown(object sender, KeyEventArgs e)
        {
            SearchFieldOnKeyPressed(e);
        }        
       
        private void btnSpareAdd_Click(object sender, RoutedEventArgs e)
        {
            SpareAdd();
        }

        private void btnSpareEdit_Click(object sender, RoutedEventArgs e)
        {
            SpareEdit();
        }

        private void btnSpareDelete_Click(object sender, RoutedEventArgs e)
        {
            SpareDelete();
        }

        private void dgSpares_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AddNewOffering();
            _ParentWindow.LoadOfferings();
        }

        private void dgSpares_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowSelectedSpareName();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void edtSearchText_TextInput(object sender, TextCompositionEventArgs e)
        {
            SpareSearch();
        }

        private void edtSearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            SpareSearch();
        }

        private void edtSearchField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SpareSearch();
        }
        private void SearchTextBox_Search(object sender, RoutedEventArgs e)
        {
            SpareSearch();
        }
    }
}
