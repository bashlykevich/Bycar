using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using bycar;
using bycar3.External_Code;
using bycar3.Reporting;
using bycar3.Views.Common;

namespace bycar3.Views.Spare_Outgo
{
    /// <summary>
    /// Interaction logic for SpareOutgoEditView.xaml
    /// </summary>
    public partial class SpareOutgoEditView : Window
    {
        public int _id = -1;
        private bool _Commited = true;
        private DataAccess da = new DataAccess();

        //double OldRowHeight = 0;
        //bool isNew = true;

        private invoice Invoice = null;
        private bool isSaved = false;

        public SpareOutgoEditView()
        {
            InitializeComponent();
            edtNumber.Text = da.SpareOutgoGetMaxId().ToString();
        }

        public SpareOutgoEditView(int SpareOutgoId)
        {
            InitializeComponent();
            _id = SpareOutgoId;
        }

        public SpareOutgoEditView(invoice _inv)
        {
            da = new DataAccess();
            InitializeComponent();
            edtNumber.Text = da.SpareOutgoGetMaxId().ToString();
            Invoice = da.InvoiceGet(_inv.id);
        }

        public decimal LoadOfferings()
        {
            decimal sum = 0;
            int n = 1;
            da = new DataAccess();
            List<SpareInSpareOutgoView> items = da.SpareInSpareOutgoViewGet(this._id);
            foreach (SpareInSpareOutgoView i in items)
            {
                i.num = n;
                sum += i.total_sum;
                n++;
            }
            dgSpares.DataContext = items;
            edtSum.Content = "Сумма: " + sum.ToString();
            return sum;
        }

        private void AddNewOffering()
        {
            SpareInOutgoSelectView v = new SpareInOutgoSelectView();
            v._SpareOutgoID = this._id;
            v.ParentWindow = this;
            v.CurrentCurrencyCode = edtCurrency.SelectedItem.ToString();
            v.ShowDialog();

            /*
            SpareInMovementEditView v = new SpareInMovementEditView();
            v._md = MovementDirection.Out;
            v._spareId = -1;
            v._offeringNumber = dgSpares.Items.Count + 1;
            v._spareIncomeId = this._id;
            v.ShowDialog();
            LoadOfferings();*/
        }

        private void btnAccountCreateNew_Click(object sender, RoutedEventArgs e)
        {
            CreateNewAccount();
        }

        private void btnAccountSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectView v = new SelectView();
            v.ClassName = (new account()).ToString();
            v.ShowDialog();
            LoadComboboxAccount();
            try
            {
                edtCustomer.SelectedValue = v.Selected._Id;
            }
            catch (Exception)
            {
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            GenerateReport();
        }

        private void btnExporTtnNWithAppendix_Click(object sender, RoutedEventArgs e)
        {
            if (SaveItem())
            {
                Reporter.GenerateTTNWithAppendixFromSpareIncomeId(this._id);
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (SaveItem())
            {
                _Commited = true;
                this.Close();
            }
            else
                MessageBox.Show("Запись невозможно сохранить!");
        }

        private void btnSalesCheck_Click(object sender, RoutedEventArgs e)
        {
            if (SaveItem())
            {
                Reporter.GenerateSalesCheck(this._id);
            }
        }

        private void btnSpareAdd_Click(object sender, RoutedEventArgs e)
        {
            // сохранить накладную
            SaveItem();

            //добавить новый товар в список
            AddNewOffering();
            LoadOfferings();
        }

        private void btnSpareDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgSpares.SelectedItem == null)
            {
                MessageBox.Show("Сначала выберите деталь из списка");
                return;
            }
            MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выделенную запись?", "Удаление", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                // сохранить накладную
                //SaveItem();
                // удалить выделенный товар
                DeleteSelectedOffering();
            }
        }

        private void btnSpareEdit_Click(object sender, RoutedEventArgs e)
        {
            // сохранить накладную
            SaveItem();

            //редактировать товар в списке
            //Feb15 EditSelectedOffering();
        }

        private int CreateItem()
        {
            da = new DataAccess();
            string _CurrencyCode = edtCurrency.SelectedItem.ToString();
            return da.SpareOutgoCreate(getItemFromFields(), _CurrencyCode);
        }

        private void CreateNewAccount()
        {
            int NewID = 0;
            AccountsEditView v = new AccountsEditView();
            v.ShowDialog();
            NewID = v._id;
            LoadComboboxAccount();
            edtCustomer.SelectedValue = NewID;
        }

        private void DeleteNewCreated()
        {
            da = new DataAccess();
            da.SpareOutgoDelete(_id);
        }

        private void DeleteSelectedOffering()
        {
            int offeringId = getSelectedOfferingId();
            da.OutOfferingDelete(offeringId);
            int SpareID = (dgSpares.SelectedItem as SpareInSpareOutgoView).spare_id;
            SpareContainer.Instance.Update(da.GetSpareView(SpareID));
            LoadOfferings();
        }

        private void EditItem()
        {
            da = new DataAccess();
            string _CurrencyCode = edtCurrency.SelectedItem.ToString();
            da.SpareOutgoEdit(getItemFromFields(), _CurrencyCode);
        }

        private void edtCustomer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadAccountReqs();
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            //OldRowHeight = rdSpareTableRow.ActualHeight;
            rdAppendixRow.Height = new GridLength(260);
            //double s = this.ActualHeight - rdHeader.ActualHeight - 260 - rdButtonBar.ActualHeight;
            //rdSpareTableRow.Height = new GridLength(s);
        }

        private void expAppendix_Collapsed(object sender, RoutedEventArgs e)
        {
            rdAppendixRow.Height = new GridLength(30);
            //double s = this.ActualHeight - rdHeader.ActualHeight - 260 - rdButtonBar.ActualHeight;
            //rdSpareTableRow.Height = new GridLength(s);
            //rdSpareTableRow.Height = new GridLength(OldRowHeight, GridUnitType.Pixel);
        }

        private void FillOutgoWithInvoiceOfferings()
        {
            List<SpareInInvoiceView> offerings = da.GetSparesByInvoiceID(Invoice.id);
            foreach (SpareInInvoiceView siiv in offerings)
            {
                int SpareInSpareIncomeID = siiv.IncomeID.HasValue ? siiv.IncomeID.Value : 0;
                if (SpareInSpareIncomeID > 0)
                {
                    int quantity = siiv.quantity.Value;
                    int SpareOutgoID = this._id;
                    decimal Price = siiv.price.Value;
                    decimal BasicPrice = siiv.IncomePOutBasic.HasValue ? siiv.IncomePOutBasic.Value : 0;
                    decimal discount = 0;
                    da.SpareInSpareOutgoCreate(SpareInSpareIncomeID, quantity, SpareOutgoID, Price, BasicPrice, discount, 0);

                    SpareContainer.Instance.Update(siiv.SpareID.Value);
                }
            }
        }

        private void GenerateReport()
        {
            if (SaveItem())
            {
                Reporter.GenerateTNFromSpareIncomeId(this._id);
            }
        }

        private spare_outgo getItemFromFields()
        {
            spare_outgo item = new spare_outgo();
            item.id = _id;
            item.IDN = Int32.Parse(edtNumber.Text);
            item.created_on = edtDate.SelectedDate.HasValue ? edtDate.SelectedDate.Value : DateTime.Now;
            item.description = edtDescription.Text;

            item.accepter = edtAccepter.Text;
            item.address = edtAddress.Text;
            item.basement = edtBasement.Text;
            if (Invoice != null)
            {
                if (Invoice.account == null)
                    if (Invoice.accountReference != null)
                        Invoice.accountReference.Load();
                if (Invoice.account != null)
                    item.AccountID = Invoice.account.id;
            }
            else
            {
                if (edtCustomer.SelectedItem != null)
                    item.AccountID = (edtCustomer.SelectedItem as AccountView).id;
            }
            item.deliverer = edtDeliverer.Text;
            item.driver = edtDriver.Text;
            item.warrant = edtProcuration.Text;
            item.trailer = edtTrailer.Text;
            item.tripsheet = edtTripSheet.Text;
            item.truck = edtTruck.Text;
            item.truckowner = edtTruckOwner.Text;
            item.unloading = edtUnloading.Text;
            item.unn = edtUNN.Text;
            if (cbEmptySpareOutgo.IsChecked.Value)
                item.isGhost = 1;
            else
                item.isGhost = 0;
            return item;
        }

        private int getSelectedOfferingId()
        {
            int result = 0;
            try
            {
                if (dgSpares.SelectedItem != null)
                {
                    object sel = dgSpares.SelectedItem;
                    result = ((SpareInSpareOutgoView)sel).id;
                }
                else
                {
                    MessageBox.Show("Сначала выберите деталь из списка");
                }
            }
            catch (Exception)
            {
                LoadOfferings();
            }
            return result;
        }

        private void LoadAccountReqs()
        {
            AccountView a = edtCustomer.SelectedItem as AccountView;
            if (a != null)
            {
                edtAddress.Text = a.address;
                edtUNN.Text = a.unn;
            }
        }

        private void LoadComboboxAccount()
        {
            List<AccountView> items = da.GetAllAccountViews();
            edtCustomer.DataContext = items;

            //edtCustomer.SelectedIndex = 0;
        }

        private void LoadComboboxCurrency()
        {
            List<currency> items = da.GetCurrency();
            foreach (currency i in items)
            {
                edtCurrency.Items.Add(i.code);
            }
            edtCurrency.SelectedIndex = 0;
        }

        private void LoadItem()
        {
            if (Invoice != null)
            {
                _Commited = false;
                cbEmptySpareOutgo.IsEnabled = true;
                edtDate.SelectedDate = Invoice.InvoiceDate;

                //edtNumber.Text = item.IDN.ToString();
                //edtOpened.IsChecked = item.opened == 1 ? true : false;
                //edtAccepter.Text = item.accepter;
                //edtAddress.Text = item.address;
                //edtBasement.Text = item.basement;
                //edtCustomer.Text = item.customer;
                if (Invoice.account == null)
                    Invoice.accountReference.Load();
                if (Invoice.account != null)
                    edtCustomer.SelectedValue = Invoice.account.id;

                //edtDeliverer.Text = item.deliverer;
                //edtDriver.Text = item.driver;
                //edtProcuration.Text = item.warrant;
                //edtTrailer.Text = item.trailer;
                //edtTripSheet.Text = item.tripsheet;
                //edtTruck.Text = item.truck;
                //edtTruckOwner.Text = item.truckowner;
                //edtUnloading.Text = item.unloading;
                //edtUNN.Text = item.unn;
                SaveItem();
                FillOutgoWithInvoiceOfferings();
                LoadOfferings();
                return;
            }
            if (_id > 0)
            {
                spare_outgo item = da.SpareOutgoGet(_id);
                edtCurrency.SelectedItem = item.currency.code;
                edtDescription.Text = item.description;
                edtDate.SelectedDate = item.created_on;
                edtNumber.Text = item.IDN.ToString();
                edtOpened.IsChecked = item.opened == 1 ? true : false;

                edtAccepter.Text = item.accepter;
                edtAddress.Text = item.address;
                edtBasement.Text = item.basement;

                //edtCustomer.Text = item.customer;
                edtCustomer.SelectedValue = item.AccountID;
                edtDeliverer.Text = item.deliverer;
                edtDriver.Text = item.driver;
                edtProcuration.Text = item.warrant;
                edtTrailer.Text = item.trailer;
                edtTripSheet.Text = item.tripsheet;
                edtTruck.Text = item.truck;
                edtTruckOwner.Text = item.truckowner;
                edtUnloading.Text = item.unloading;
                edtUNN.Text = item.unn;

                // загрузить товары в накладной
                LoadOfferings();
                _Commited = true;
                if (item.isGhost == 1)
                    cbEmptySpareOutgo.IsChecked = true;

                //dgSpares.IsEnabled = false;
                //TMP btnSpareDelete.IsEnabled = false;
                //btnSpare.IsEnabled = false;
            }
            else
            {
                _Commited = false;
                cbEmptySpareOutgo.IsEnabled = true;

                //dgSpares.IsEnabled = true;
                //TMP btnSpareDelete.IsEnabled = true;
                //btnSpareEdit.IsEnabled = true;
            }
        }

        private bool SaveItem()
        {
            bool res = true;

            //try
            //{
            if (_id > 0)
            {
                EditItem();
            }
            else
            {
                _id = CreateItem();
            }
            if (edtOpened.IsChecked.HasValue)
            {
                if (edtOpened.IsChecked.Value)
                {
                    da = new DataAccess();
                    da.SpareOutgoSetOpened(this._id);
                }
            }
            isSaved = true;
            cbEmptySpareOutgo.IsEnabled = false;

            //}
            //catch (Exception e)
            //{
            //    res = false;
            //}
            return res;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_Commited && _id > 0)
            {
                DeleteNewCreated();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            edtDate.SelectedDate = DateTime.Now;
            LoadComboboxCurrency();
            LoadComboboxAccount();
            LoadItem();
        }

        /*Feb15
        void EditSelectedOffering()
        {
            SpareInMovementEditView v = new SpareInMovementEditView();
            v._md = MovementDirection.Out;
            v._offeringId = getSelectedOfferingId();
            v._spareIncomeId = this._id;
            v.ShowDialog();
            LoadOfferings();
        }*/
    }
}