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

namespace bycar3.Views.Spare_Income
{
    /// <summary>
    /// Interaction logic for SpareInIncomeSelectView.xaml
    /// </summary>
    public partial class SpareInIncomeSelectView : Window
    {
        #region MEMBERS

        public SpareIncomeEditView _ParentWindow = null;
        DataAccess da = new DataAccess();
        public int _SpareIncomeID = -1;
        public int _OfferingNumber = 0;
        public string CurrenctCurrencyCode;
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
                    //MessageBox.Show("Сначала выберите деталь из списка");
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
                    //MessageBox.Show("Сначала выберите деталь из списка");
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
            if(searchFieldIndex.HasValue && !searchString.Equals(""))
            {
                if (cbSctrictSearch.IsChecked.Value)
                {
                    items = SpareContainer.Instance.GetSparesStrict(searchFieldIndex.Value, searchString);
                }
                else
                {
                    items = SpareContainer.Instance.GetSpares(searchFieldIndex.Value, searchString);
                }
            } else
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
                    SpareContainer.Instance.Remove(id);
                    LoadSpares();
                }
            }
        }
        
        void ShowSelectedSpareName()
        {
            edtOfferingName.Content = GetSelectedSpareName();
        }               

        void AddNewOffering()
        {
            if (cbSimpleIncome.IsChecked.Value)
            {
                SpareInIncomeSimpleEditView v = new SpareInIncomeSimpleEditView();
                v._SpareName = GetSelectedSpareName();
                v._SpareID = GetSelectedSpareId();
                v._SpareIncomeID = _SpareIncomeID;
                v.CurrentCurrencyCode = CurrenctCurrencyCode;
                v._OfferingNumber = _OfferingNumber;
                v.IsSimpleInput = cbSimpleIncome.IsChecked.HasValue ? cbSimpleIncome.IsChecked.Value : false;
                v.ShowDialog();
            }
            else
            {
                SpareInIncomeEditView v = new SpareInIncomeEditView();
                v._SpareName = GetSelectedSpareName();
                v._SpareID = GetSelectedSpareId();
                v._SpareIncomeID = _SpareIncomeID;
                v.CurrentCurrencyCode = CurrenctCurrencyCode;
                v._OfferingNumber = _OfferingNumber;
                v.IsSimpleInput = cbSimpleIncome.IsChecked.HasValue ? cbSimpleIncome.IsChecked.Value : false;
                v.ShowDialog();
            }
        }
        #endregion

        //  HANDLERS

        public SpareInIncomeSelectView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSpares();
            DataAccess db = new DataAccess();
            settings_profile sp = db.getProfileCurrent();
            cbSimpleIncome.IsChecked = sp.SimpleInput.HasValue ? (sp.SimpleInput.Value == 1?true:false): false;
            cbSctrictSearch.IsChecked = sp.StrictSearch.HasValue ? (sp.StrictSearch.Value == 1 ? true : false) : false;
        }        

        private void btnSpareSearch_Click(object sender, RoutedEventArgs e)
        {
            SpareSearch();
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
            _ParentWindow.dgSpares.UpdateLayout();
            _ParentWindow.dgSpares.ScrollIntoView(dgSpares.Items[dgSpares.Items.Count - 1]);            
        }

        private void dgSpares_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowSelectedSpareName();
        }
        
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {            
            Close();
        }

        private void edtSearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            SpareSearch();
        }

        private void edtSearchText_TextInput(object sender, TextCompositionEventArgs e)
        {
            SpareSearch();
        }

        private void edtSearchField_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DataAccess db = new DataAccess();
            settings_profile sp = db.getProfileCurrent();
            //cbSimpleIncome.IsChecked = sp.SimpleInput.HasValue ? (sp.SimpleInput.Value == 1 ? true : false) : false;
            sp.SimpleInput = cbSimpleIncome.IsChecked.HasValue ? (cbSimpleIncome.IsChecked.Value ? 1 : 0) : 0;
            sp.StrictSearch = cbSctrictSearch.IsChecked.HasValue ? (cbSctrictSearch.IsChecked.Value ? 1 : 0) : 0;
            db.ProfileEdit(sp);
        }

        private void cbSctrictSearch_Checked(object sender, RoutedEventArgs e)
        {
            SpareSearch();
        }
    }
}
