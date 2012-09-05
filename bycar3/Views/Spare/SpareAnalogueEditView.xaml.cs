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

namespace bycar3.Views
{
    /// <summary>
    /// Interaction logic for SpareAnalogueEditView.xaml
    /// </summary>    
    public partial class SpareAnalogueEditView : Window
    {

        DataAccess da = new DataAccess();
        public int _id = 0;
        public int _spareId1 = 0;
        public int _spareId2 = 0;
        public SpareView result = null;

        public SpareAnalogueEditView()
        {
            InitializeComponent();
            loadComboBox_Spares();
        }


        private void EditItem()
        {
            DataAccess da = new DataAccess();
            da.SpareAnalogueEdit(getItemFromFields(), getSelectedSpareName());
        }
        private void CreateItem()
        {
            DataAccess da = new DataAccess();
            int _spareId2 = getSelectedSpareId();
            da.SpareAnalogueCreate(getItemFromFields(), _spareId1, _spareId2);
        }
        string getSelectedSpareName()
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
                loadSpares();
            }
            return result;
        }

        int getSelectedSpareId()
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
                loadSpares();
            }
            return result;
        }

        spare_analogue getItemFromFields()
        {
            spare_analogue item = new spare_analogue();
            item.id = this._id;
            item.is_equal = edtIsBoth.IsChecked.GetValueOrDefault(false) ? 1 : 0;
            return item;
        }
        void loadComboBox_Spares()
        {
            //DataAccess da = new DataAccess();
            //List<SpareView> items = da.GetSpares();
            //foreach (SpareView i in items)
            //{
            //edtSpare.Items.Add(i.name);
            //}
            //edtSpare.SelectedIndex = 0;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int ind = loadSpares();
            dgSpares.SelectedIndex = ind;
        }
        private int loadSpares()
        {
            int ind = 0;
            List<SpareView> items = SpareContainer.Instance.Spares.ToList();
            int cntr = 0;
            items.Remove(items.FirstOrDefault(x => x.id == _spareId1));
            // убрать уже добавленные аналоги
            List<SpareView> analogues = da.GetAnalogues(_spareId1);
            foreach (SpareView a in analogues)
                items.Remove(items.FirstOrDefault(x => x.id == a.id));
            foreach (SpareView i in items)
            {
                //i.brandReference.Load();
                //i.unitReference.Load();
                //i.spare_groupReference.Load();
                if (result != null)
                    if (i.name == result.name)
                        ind = cntr;
                cntr++;
            }
            dgSpares.DataContext = items;
            return ind;
        }
        private void edtSearchText_KeyDown(object sender, KeyEventArgs e)
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
        private void btnSpareSearch_Click(object sender, RoutedEventArgs e)
        {
            SpareSearch();
        }
        void SpareSearch()
        {
            if (edtSearchText != null)
            {
                string searchString = edtSearchText.Text;
                int searchFieldIndex = edtSearchField.SelectedIndex;
                ReloadSpares(searchFieldIndex, searchString);

            }
        }
        private void ReloadSpares(int searchFieldIndex, string searchString)
        {
            //List<SpareView> items = da.GetSpares(searchFieldIndex, searchString);
            try
            {
                List<SpareView> items = SpareContainer.Instance.GetSpares(searchFieldIndex, searchString);
                if (items.FirstOrDefault(x => x.id == _spareId1) != null)
                    items.Remove(items.FirstOrDefault(x => x.id == _spareId1));
                // убрать уже добавленные аналоги
                List<SpareView> analogues = da.GetAnalogues(_spareId1);
                foreach (SpareView a in analogues)
                    items.Remove(items.FirstOrDefault(x => x.id == a.id));
                dgSpares.DataContext = items;
            }
            catch (Exception e)
            {
                Marvin.Instance.Log(e.Message);
                loadSpares();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void dgSpares_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this._id > 0)
                EditItem();
            else
                CreateItem();

            this.Close();
        }

        private void btnOk_Click_1(object sender, RoutedEventArgs e)
        {
            if (this._id > 0)
                EditItem();
            else
                CreateItem();

            this.Close();
        }

        private void btnSpareAdd_Click(object sender, RoutedEventArgs e)
        {
            Marvin.Instance.SpareCreate();
            loadSpares();
        }

        private void btnSpareEdit_Click(object sender, RoutedEventArgs e)
        {
            EditSelectedItem();
        }
        private void EditSelectedItem()
        {
            if (dgSpares.SelectedItem == null)
                return;
            int SpareID = (dgSpares.SelectedItem as SpareView).id;
            Marvin.Instance.SpareEdit(SpareID);
            loadSpares();
        }

        private void btnSpareDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedSpare();
        }
        void DeleteSelectedSpare()
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
                    loadSpares();
                }
            }
        }
        private void edtSearchField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SpareSearch();
        }

        private void edtSearchText_TextInput(object sender, TextCompositionEventArgs e)
        {
            SpareSearch();
        }

        private void edtSearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            SpareSearch();
        }
        private void SearchTextBox_Search(object sender, RoutedEventArgs e)
        {
            SpareSearch();
        }
    }
}

