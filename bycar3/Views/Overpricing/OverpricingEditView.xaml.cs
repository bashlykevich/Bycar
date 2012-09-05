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
using bycar3.Views.Spare_Income;
using bycar3.Reporting;
using bycar3.Views.Invoice;
using bycar3.External_Code;

namespace bycar3.Views.Overpricing
{
    /// <summary>
    /// Interaction logic for OverpricingEditView.xaml
    /// </summary>
    public partial class OverpricingEditView : Window
    {
        #region DATA MEMBERS
        DataAccess da = new DataAccess();        
        bool ToDeleteOnClose = false;
        overpricing Item = null;
        
        #endregion

        public OverpricingEditView()
        {
            InitializeComponent();
            Item = new overpricing();
            Item.createdOn = DateTime.Now;
            Item.commited = false;
            Item.num = da.OverpricingGetMaxID();
            Item = da.OverpricingCreate(Item);

            ToDeleteOnClose = true;
        }

        public OverpricingEditView(int id)
        {
            InitializeComponent();
            Item = da.OverpricingGet(id);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {            
            Load();
        }

        public decimal LoadOfferings()
        {
            if (Item != null)
            {
                da = new DataAccess();
                List<SpareInOverpricingView> items = da.OverpricingOfferingGet(Item.id);
                dgSpares.DataContext = items;
            }
            return 0;
        }
        void Load()
        {
            if (Item == null)
            {
                // IS NEW
                //IsNew = true;
                Item = new overpricing();
                edtNumber.Text = da.OverpricingGetMaxID().ToString();
                edtDate.SelectedDate = DateTime.Now;
                edtPercentIncrease.Text = "0";
            }
            else
            {
                edtNumber.Text = Item.num.ToString();
                edtDate.SelectedDate = Item.createdOn.Value;                
                edtPercentIncrease.Text = Item.increasePerc.ToString();
                CheckIfPosted();
            }
            LoadOfferings();
        }        

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ToDeleteOnClose)
                da.OverpricingDelete(Item.id);
        }

        private void btnSpareAdd_Click(object sender, RoutedEventArgs e)
        {
            // сохранить корневой элемент
            Save();
            //добавить новый товар в список
            AddNewOffering();
        }
        void AddNewOffering()
        {
            SpareInOverpricingSelectView v = new SpareInOverpricingSelectView();
            v._ParentItemID = Item.id;
            v.ParentWindow = this;
            v.ShowDialog();
            LoadOfferings();
        }
        
        private void btnSpareDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgSpares.SelectedItem == null)
                return;
            MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выделенную запись?", "Удаление", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                // сохранить накладную
                Save();
                // удалить выделенный товар
                DeleteSelectedOffering();
            }
        }
        void DeleteSelectedOffering()
        {
            try
            {
                int offeringId = (dgSpares.SelectedItem as SpareInOverpricingView).id;
                da.OverpricingOfferingDelete(offeringId);
                LoadOfferings();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {

            Save();            
            Reporter.GenerateOverpricingReport(Item.id);
        }

     
        overpricing getItemFromFields()
        {
            overpricing i = new overpricing();
            i.createdOn = edtDate.SelectedDate.Value;
            //i.description = edt
            int pers = 0;
            Int32.TryParse(edtPercentIncrease.Text, out pers);
            i.increasePerc = pers;
            return i;
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ToDeleteOnClose = false;
            Save();
            Close();            
        }

        private void btnPost_Click(object sender, RoutedEventArgs e)
        {
            OverpricingPost();
        }

        private void btnCancelPost_Click(object sender, RoutedEventArgs e)
        {
            OverpricingUndoPost();
        }
        void FillWithRemains()
        {
            int num = 1;
            // получить список приходов, по которым есть остатки 
            List<SpareInSpareIncomeView> incomes = da.GetActualIncomes();
            foreach (SpareInSpareIncomeView income in incomes)
            {
                int SpareID = income.SpareID.Value;
                // создаем новую запись в таблице переоценки
                spare_in_overpricing sio = new spare_in_overpricing();
                // наполняем данными
                sio.num = num++;
                
                sio.percentOld = (int)income.Markup;
                sio.priceOld = income.POut.Value;
                sio.purchasePrice = income.PIn.Value;
                sio.quantity = (int)income.QIn;
                sio.receiptDate = income.SpareIncomeDate;
                sio.spare = da.GetSpare(SpareID);
                sio.sumOld = income.S;

                sio.overpricing = da.OverpricingGet(Item.id);
                sio.spare_in_spare_income = da.InOfferingGet(income.id);
                // сохраняем в БД
                da.OverpricingOfferingCreate(sio);
            }
        }
        private void btnFillWithRemains_Click(object sender, RoutedEventArgs e)
        {
            Save();
            FillWithRemains();
            LoadOfferings();
        }
        private bool isManualEditCommit;
        private void HandleMainDataGridCellEditEnding(
          object sender, DataGridCellEditEndingEventArgs e)
        {
            if (!isManualEditCommit)
            {
                isManualEditCommit = true;
                DataGrid grid = (DataGrid)sender;
                grid.CommitEdit(DataGridEditingUnit.Row, true);
                isManualEditCommit = false;
            }
        }
        private void dgSpares_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            HandleMainDataGridCellEditEnding(sender, e);
            SpareInOverpricingView siov = (SpareInOverpricingView)e.Row.DataContext;
            if (siov.id > 0)
                if (siov.percentNew.HasValue)
                {
                    siov.priceNew = siov.purchasePrice + siov.purchasePrice * (siov.percentNew + siov.VatRate) / 100;
                    siov.sumNew = siov.priceNew * siov.quantity;
                    da.OverpricingOfferingEdit(siov);
                }
        }
        void Recalc()
        {
            int perc = 0;
            int.TryParse(edtPercentIncrease.Text, out perc);
            edtPercentIncrease.Text = perc.ToString();
            List<SpareInOverpricingView> items = da.OverpricingOfferingGet(Item.id);
            foreach (SpareInOverpricingView siov in items)
            {
                siov.percentNew = siov.percentOld + perc;
                siov.priceNew = siov.purchasePrice + siov.purchasePrice * (siov.percentNew + siov.VatRate) / 100;
                siov.sumNew = siov.priceNew * siov.quantity;
                da.OverpricingOfferingEdit(siov);
            }
            dgSpares.DataContext = items;
        }
        private void btnRecalc_Click(object sender, RoutedEventArgs e)
        {
          Recalc();
        }

        void OverpricingPost()
        {
            Item.commited = true;
            Save();
            Recalc();
            RecalcIncomesPrices();
            CheckIfPosted();
        }
        void OverpricingUndoPost()
        {
            Item.commited = false;
            Save();
            RecalcIncomesPricesBack();
            CheckIfPosted();
        }
        void CheckIfPosted()
        {   
            bool flag = false;
            if(Item != null)
                if(Item.commited.HasValue)
                    flag = Item.commited.Value;

            edtDate.IsEnabled = !flag;
            edtPercentIncrease.IsEnabled = !flag;
            btnFillWithRemains.IsEnabled = !flag;
            btnPost.IsEnabled = !flag;
            btnRecalc.IsEnabled = !flag;
            btnSpareAdd.IsEnabled = !flag;
            btnSpareDelete.IsEnabled = !flag;
            dgSpares.IsEnabled = !flag;

            btnCancelPost.IsEnabled = flag;
        }
        
        /*
        int Save_OLD()
        {
            if (Item == null)
            {
                Item = new overpricing();
                Item.commited = 0;
                Item.description = "";
                Item.createdOn = edtDate.SelectedDate;
                int ip = 0;
                int.TryParse(edtPercentIncrease.Text, out ip);
                Item.increasePerc = ip;
                Item = da.OverpricingCreate(Item);
            }
            else
            {
                Item.createdOn = edtDate.SelectedDate;
                int ip = 0;
                int.TryParse(edtPercentIncrease.Text, out ip);
                Item.increasePerc = ip;
                Item = da.OverpricingEdit(Item);
            }
            return Item.id;
        }*/
        //void Fill()
        //{
        //    edtDate.SelectedDate = Item.createdOn.Value;
        //    edtNumber.Text = Item.id.ToString();
        //    edtPercentIncrease.Text = Item.increasePerc.ToString();
        //    CheckIfPosted();
        //}
        void Fill(int ID)
        {
            Item = da.OverpricingGet(ID);
            edtDate.SelectedDate = Item.createdOn.Value;
            edtNumber.Text = Item.num.ToString();
            edtPercentIncrease.Text = Item.increasePerc.ToString();
            CheckIfPosted();
        }
        void RecalcIncomesPrices()
        {
            List<SpareInOverpricingView> items = da.OverpricingOfferingGet(Item.id);
            foreach (SpareInOverpricingView siov in items)
            {
                da.SetNewIncomePrice(siov.IncomeID, siov.percentNew.Value);
            }
        }
        void RecalcIncomesPricesBack()
        {
            List<SpareInOverpricingView> items = da.OverpricingOfferingGet(Item.id);
            foreach (SpareInOverpricingView siov in items)
            {
                da.SetNewIncomePrice(siov.IncomeID, siov.percentOld.Value);
            }
        }
        void Save()
        {
           
           
               // переписываем данные с формы в объект                    
                    Item.createdOn = edtDate.SelectedDate;
                    int ip = 0;
                    int.TryParse(edtPercentIncrease.Text, out ip);
                    Item.increasePerc = ip;
                    da.OverpricingEdit(Item);               
        }
        void Delete()
        {
            da.OverpricingDelete(Item.id);
        }
    }
}
