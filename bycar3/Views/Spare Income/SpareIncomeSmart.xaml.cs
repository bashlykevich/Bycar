using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using bycar;
using bycar3.Core;
using bycar3.External_Code;

namespace bycar3.Views.Spare_Income
{
    /// <summary>
    /// Interaction logic for SpareIncomeSmart.xaml
    /// </summary>
    public partial class SpareIncomeSmart : Window
    {
        private List<SpareInSpareIncomeView> offerings = new List<SpareInSpareIncomeView>();
        private string code = "";
        private spare_income Income = null;
        private int Stage = 0;

        public SpareIncomeSmart(int IncID, string filename)
        {
            //try
            {
                InitializeComponent();
                DataAccess db = new DataAccess();
                Income = db.SpareIncomeGet(IncID);

                // readinf csv
                string[] source = File.ReadAllLines(filename, System.Text.Encoding.Default);
                int i = 0;
                foreach (string s in source)
                {
                    if (i != 0)
                    {
                        string[] fields = s.Split(';');

                        // 0 код_брэнд
                        string[] f1 = fields[0].Split('_');
                        string CodeShatem = f1[0];
                        string BrandName = f1[1];

                        // 1 количество
                        decimal Q = 0;
                        decimal.TryParse(fields[1], out Q);

                        // 2 код_брэнд_
                        string[] f2 = fields[2].Split('_');

                        // string CodeShatem = f1[0];
                        // string Brand = f1[1];
                        // string Code = f2[2];
                        // 3 Цена, доллар
                        decimal Pusd = 0;
                        string p = fields[3].Replace(',', '.');
                        decimal.TryParse(p, out Pusd);

                        // 4 Цена, евро
                        decimal Peuro = 0;
                        decimal.TryParse(fields[4], out Peuro);

                        // 5 группа
                        string ParentGroup = fields[5];

                        // 6 группа
                        string Group = fields[6];

                        // 7 группа
                        string name = fields[7];

                        // 8 единица измерения
                        string UnitName = fields[8];

                        // поиск запчасти по коду и брэнду
                        SpareView FoundSpare = null;
                        int searchFieldIndex = 2;   // код шате-м
                        List<SpareView> FoundList = SpareContainer.Instance.GetSparesStrict(searchFieldIndex, CodeShatem);
                        if (FoundList.Count == 0)
                        {
                            // если запчасть не найдена, предложить создать новую
                            string mess = "Товар с кодом [" + CodeShatem + "] не найден в базе.\n";
                            mess += "Название:  " + name + "\n";
                            mess += "Подгруппа: " + Group + "\n";
                            mess += "Группа:    " + ParentGroup + "\n";
                            mess += "Брэнд:     " + BrandName + "\n";
                            mess += "Создать новый?";
                            MessageBoxResult answer = MessageBox.Show(mess, "Импорт новой детали", MessageBoxButton.YesNo);
                            if (answer == MessageBoxResult.Yes)
                            {
                                // если создать новую, создать новую деталь
                                FoundSpare = Marvin.Instance.SpareCreateSilent(name, CodeShatem, Group, ParentGroup, BrandName, UnitName, "Импортировано");
                            }
                        }
                        else
                            if (FoundList.Count == 1)
                            {
                                FoundSpare = FoundList[0];
                            }
                            else
                            {
                                FoundList = FoundList.Where(x => x.BrandName == BrandName).ToList();

                                // если есть детали с одинаковым кодом, выбрать по брэнду (группе)
                                if (FoundList.Count == 1)
                                    FoundSpare = FoundList[0];
                            }
                        if (FoundSpare != null)
                        {
                            // добавить в список, подставиви количество и цену
                            OfferingAdd(FoundSpare.id, Q, Pusd, Group, ParentGroup);
                        }
                    }
                    i++;
                }
                LoadOfferings();
                Stage++;
                dgSpares.IsEnabled = true;
            }

            //catch(Exception e)
            //{
            //    MessageBox.Show("Ошибка импорта: " + e.Message + "\n" + e.InnerException);
            //}
        }

        public SpareIncomeSmart(int IncID)
        {
            InitializeComponent();
            DataAccess db = new DataAccess();
            Income = db.SpareIncomeGet(IncID);
            dgSpares.IsEnabled = false;
        }

        private void SpareSearch()
        {
            string SearchCode = code;
            SpareView sv = null;
            code = "";
            List<SpareView> tmp = SpareContainer.Instance.GetSparesStrict(0, SearchCode);
            if (tmp.Count > 0)
            {
                sv = tmp[0];
            }
            else
            {
                edtSearchString.Content = "НЕ НАЙДЕНО!";
                if (MessageBox.Show("Товар не найден. Добавить сейчас новый?", "Добавление товара", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    sv = Marvin.Instance.SpareCreate();
                else
                    return;
            }
            if (sv != null)
            {
                OfferingAdd(sv.id);
                LoadOfferings();
            }
            else
            {
                edtSearchString.Content = "error";
            }
        }

        private void LoadOfferings()
        {
            dgSpares.DataContext = null;
            dgSpares.DataContext = offerings;
            dgSpares.UpdateLayout();
            if (dgSpares.Items.Count > 1)
                dgSpares.ScrollIntoView(dgSpares.Items[dgSpares.Items.Count - 1]);
        }

        private void OfferingAdd(int SpareID)
        {
            SpareInSpareIncomeView sisi = new SpareInSpareIncomeView();
            if (Income.currency == null)
                Income.currencyReference.Load();
            sisi.num = offerings.Count + 1;
            sisi.CurrencyID = Income.currency.id;
            sisi.description = "";
            sisi.Markup = 0;
            sisi.PIn = 1;
            sisi.PInBasic = 1;
            sisi.POut = 1;
            sisi.POutBasic = 1;
            sisi.QIn = 1;
            sisi.QRest = 1;
            sisi.S = 1;
            sisi.SBasic = 1;
            DataAccess db = new DataAccess();
            sisi.SpareID = SpareID;
            SpareView sv = db.GetSpareView(SpareID);
            sisi.SpareName = sv.name;
            sisi.SpareCode = sv.code;
            sisi.SpareCodeShatem = sv.codeShatem;
            sisi.BrandName = sv.BrandName;
            sisi.SpareIncomeID = Income.id;
            sisi.VatRateName = "0%";
            sisi.GroupName = db.GetSpareGroup(sv.GroupID).name;
            sisi.ParentGroupName = db.GetSpareGroup(sv.spare_group1_id.Value).name;
            offerings.Add(sisi);
        }

        private void OfferingAdd(int SpareID, decimal Q, decimal Pusd, string GroupName, string ParentGroupName)
        {
            SpareInSpareIncomeView sisi = new SpareInSpareIncomeView();
            if (Income.currency == null)
                Income.currencyReference.Load();
            sisi.num = offerings.Count + 1;
            sisi.CurrencyID = Income.currency.id;
            sisi.description = "";
            sisi.Markup = 0;
            sisi.PIn = Pusd;
            sisi.PInBasic = Pusd;
            sisi.POut = Pusd;
            sisi.POutBasic = Pusd;
            sisi.QIn = Q;
            sisi.QRest = Q;
            sisi.S = Q * Pusd;
            sisi.SBasic = sisi.S;
            sisi.GroupName = GroupName;
            sisi.ParentGroupName = ParentGroupName;
            DataAccess db = new DataAccess();
            sisi.SpareID = SpareID;
            SpareView sv = db.GetSpareView(SpareID);
            sisi.SpareName = sv.name;
            sisi.SpareCode = sv.code;
            sisi.SpareCodeShatem = sv.codeShatem;
            sisi.BrandName = sv.BrandName;
            sisi.SpareIncomeID = Income.id;
            sisi.VatRateName = "0%";
            offerings.Add(sisi);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Stage == 0)
            {
                Key k = e.Key;
                if (k == Key.Enter)
                    SpareSearch();
                else
                {
                    // digits
                    if (k >= Key.D0 && k <= Key.D9)
                    {
                        char c = k.ToString()[1];
                        code += c;
                    }
                    else
                        code += k;
                }
                edtSearchString.Content = code;
            }
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
            if (Stage == 1)
            {
                HandleMainDataGridCellEditEnding(sender, e);
                SpareInSpareIncomeView sisi = (SpareInSpareIncomeView)e.Row.DataContext;
                if (sisi.SpareID.HasValue)
                {
                    if (e.Column.DisplayIndex == 8)
                    {
                        //sisi.PIn = sisi.POut * (1 - sisi.Markup / 100);
                        sisi.Markup = (int)(100 * (sisi.POut - sisi.PIn) / sisi.PIn);
                    }
                    else
                    {
                        sisi.POut = sisi.PIn * (1 + sisi.Markup / 100);
                    }
                    sisi.S = sisi.POut * sisi.QIn;
                }
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            Stage++;
            if (Stage == 1)
            {
                dgSpares.IsEnabled = true;
                btnNext.Content = "Сохранить";
            }
            else
                if (Stage == 2)
                {
                    foreach (SpareInSpareIncomeView sisi in offerings)
                    {
                        //sisi.SpareName
                        //sisi.BrandName
                        //sisi.SpareCode
                        //sisi.SpareCodeShatem
                        DataAccess db = new DataAccess();
                        int SpareID = sisi.SpareID.Value;
                        db.InOfferingCreate(sisi.SpareID.Value, sisi.QIn, sisi.PIn.Value, sisi.Markup, sisi.POut.Value, sisi.S.Value, sisi.SpareIncomeID.Value);
                        SpareContainer.Instance.Update(SpareID);
                    }
                    Close();
                }
        }
    }
}