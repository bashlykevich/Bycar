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
using bycar3.Reporting;

namespace bycar3.Views.Sales
{
    /// <summary>
    /// Interaction logic for SaleEditView.xaml
    /// </summary>
    public partial class SaleEditView : Window
    {
        int SaleID = 0;
        public SaleEditView(int saleID)
        {
            InitializeComponent();
            SaleID = saleID;
            LoadSales();
        }
        public void LoadSales()
        {
            DataAccess db = new DataAccess();
            List<BasketView> BasketItems = db.BasketViewGet(SaleID);
            dgBasket.DataContext = null;
            dgBasket.DataContext = BasketItems;

            Sale sale = db.SaleGet(SaleID);
            decimal Pbyr = sale.Pbyr;
            decimal Peur = sale.Peur;
            decimal Pusd = sale.Pusd;

            //edtPbyr.Text = string.Format("{0:n0}", Pbyr) + " Br";
            string edtPbyr = Pbyr.ToString("### ### ### ###") + " Br";
            string edtPeur = Peur.ToString() + " €";
            string edtPusd = Pusd.ToString() + " $";
            string edtBasketQ = "Количество: " + BasketItems.Count + " объектов";
            this.Title = "Продажа №" + sale.Number + ", " + sale.SaleDate.ToShortDateString() + ": "
                            + edtPbyr + " | " + edtPusd + " | " + edtPeur;
        }

        private void dgBasket_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                if (dgBasket.SelectedItems.Count == 0)
                    return;
                DataAccess db = new DataAccess();
                MessageBoxResult mbr = MessageBox.Show("Удалить из корзины выделенное?", "Корзина", MessageBoxButton.YesNo);
                if (mbr == MessageBoxResult.Yes)
                {
                    List<BasketView> toDelete = new List<BasketView>();
                    foreach (var i in dgBasket.SelectedItems)
                        toDelete.Add(i as BasketView);
                    db.BasketDelete(toDelete);
                }
                LoadSales();
            }
        }
        void Print()
        {
            if (dgBasket.Items.Count > 0)
            {
                DataAccess db = new DataAccess();
                Reporter.GenerateSalesCheck(db.SaleGet(SaleID));
            }
            else
            {
                MessageBox.Show("Корзина пуста!");
            }
        }
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            Print();
        }
    }
}
