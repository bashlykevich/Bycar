using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml;
using bycar;
using bycar3.Views.Common;

namespace bycar3.External_Code
{
    public class Office2003XmlTable
    {
        public static List<brand> getBrands(string FilePath)
        {
            List<brand> brands = new List<brand>();

            // Объявляем и забиваем файл в документ
            XmlDocument xd = new XmlDocument();
            FileStream fs = new FileStream(FilePath, FileMode.Open);
            xd.Load(fs);

            XmlNodeList list = xd.GetElementsByTagName("Row"); // Создаем и заполняем лист по тегу "user"
            for (int i = 0; i < list.Count; i++)
            {
                brand b = new brand();
                b.code1C = list[i].FirstChild.InnerText;
                b.name = list[i].LastChild.InnerText;
                brands.Add(b);
            }

            // Закрываем поток
            fs.Close();

            return brands;
        }

        public static List<spare_group> getGroups(string FilePath)
        {
            List<spare_group> items = new List<spare_group>();

            // Объявляем и забиваем файл в документ
            XmlDocument xd = new XmlDocument();
            FileStream fs = new FileStream(FilePath, FileMode.Open);
            xd.Load(fs);

            XmlNodeList list = xd.GetElementsByTagName("Row"); // Создаем и заполняем лист по тегу "row"
            for (int i = 0; i < list.Count; i++)
            {
                spare_group b = new spare_group();
                b.code1C = list[i].FirstChild.InnerText;
                b.name = list[i].LastChild.InnerText;
                items.Add(b);
            }
            MessageBox.Show("Сформирован список из " + items.Count.ToString() + " элементов.");
            DataAccess da = new DataAccess();
            int xc = 0;
            foreach (spare_group i in items)
            {
                da.SpareGroupCreate(i);
                xc++;
            }
            MessageBox.Show("Добавлено  " + xc.ToString() + " элементов.");
            FixGroupsParents(list);

            // Закрываем поток
            fs.Close();

            return items;
        }

        private static void FixGroupsParents(XmlNodeList list)
        {
            DataAccess da = new DataAccess();
            for (int i = 0; i < list.Count; i++)
            {
                string ParentCode1C = "";
                string Code1C = list[i].ChildNodes[0].InnerText;
                ParentCode1C = list[i].ChildNodes[1].InnerText;
                if (ParentCode1C.Equals(""))
                {
                    ParentCode1C = "ROOT";
                }
                da.SpareGroupUpdateParent(Code1C, ParentCode1C);
            }
        }

        //private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);

        public static int getSpares(string FilePath)
        {
            List<spare> items = new List<spare>();

            // Объявляем и забиваем файл в документ
            XmlDocument xd = new XmlDocument();
            FileStream fs = new FileStream(FilePath, FileMode.Open);
            xd.Load(fs);
            DataAccess da = new DataAccess();

            XmlNodeList list = xd.GetElementsByTagName("Row"); // Создаем и заполняем лист по тегу "user"

            LoadingWindowView v = new LoadingWindowView();
            v._ProgressBar.Minimum = 0;
            v._ProgressBar.Maximum = list.Count;
            v._ProgressBar.Value = list.Count / 2;

            //object value = 0;
            //Сохраняем значение ProgressBar
            //Создаем новый экземпляр делегата для ProgressBar
            // который показывает на метод ProgressBar.SetValue
            //UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(v._ProgressBar.SetValue);
            v.Show();
            string Errors = "";
            for (int i = 1; i < list.Count; i++)
            {
                try
                {
                    spare b = new spare();
                    b.code1C = list[i].ChildNodes[0].InnerText;
                    if (!da.ExistCode1C(b.code1C))
                    {
                        b.codeShatem = list[i].ChildNodes[1].InnerText;
                        b.name = list[i].ChildNodes[2].InnerText;
                        string BrandCode1C = list[i].ChildNodes[3].InnerText;
                        string GroupCode1C = list[i].ChildNodes[4].InnerText;
                        da.SpareCreate(b, BrandCode1C, GroupCode1C);
                    }
                }
                catch (Exception qwe)
                {
                    Errors += qwe.Message;
                }
            }

            // Закрываем поток
            fs.Close();
            v.Close();
            return items.Count;
        }

        public static int getAnalogues(string FilePath)
        {
            List<spare_analogue> items = new List<spare_analogue>();

            // Объявляем и забиваем файл в документ
            XmlDocument xd = new XmlDocument();
            FileStream fs = new FileStream(FilePath, FileMode.Open);
            xd.Load(fs);
            DataAccess da = new DataAccess();

            XmlNodeList list = xd.GetElementsByTagName("Row"); // Создаем и заполняем лист по тегу "user"
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].HasChildNodes)
                {
                    string Spare1Code1C = list[i].ChildNodes[0].InnerText;
                    string Spare2Code1C = list[i].ChildNodes[1].InnerText;
                    int is_equal = list[i].ChildNodes[2].InnerText.Contains("стина") ? 1 : 0;
                    da.SpareAnalogueCreate(is_equal, Spare1Code1C, Spare2Code1C);
                }
            }

            // Закрываем поток
            fs.Close();

            return items.Count;
        }

        public static string getRemains(string FilePath)
        {
            List<spare_analogue> items = new List<spare_analogue>();
            string Message = "";

            // Объявляем и забиваем файл в документ
            XmlDocument xd = new XmlDocument();
            FileStream fs = new FileStream(FilePath, FileMode.Open);
            xd.Load(fs);
            DataAccess da = new DataAccess();

            XmlNodeList list = xd.GetElementsByTagName("Row");

            //int RemainsInputID = da.getRemainsInputID();
            MessageBox.Show("Будет импортировано " + list.Count + " записей.");
            string m1 = "";
            System.Globalization.CultureInfo ci =
          System.Globalization.CultureInfo.InstalledUICulture;
            System.Globalization.NumberFormatInfo ni = (System.Globalization.NumberFormatInfo)
              ci.NumberFormat.Clone();
            ni.NumberDecimalSeparator = ".";
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].HasChildNodes)
                {
                    decimal P = 0;
                    if (decimal.TryParse(list[i].ChildNodes[4].InnerText, out P))
                    {
                        P = decimal.Parse(list[i].ChildNodes[4].InnerText, ni);
                        da = new DataAccess();
                        spare_in_spare_income offering = new spare_in_spare_income();

                        //[id] - генериурется автоматически
                        //[num]
                        offering.num = 0;

                        //[description]
                        offering.description = "";

                        //[QIn]
                        decimal Q = 0;
                        Q = decimal.Parse(list[i].ChildNodes[3].InnerText, ni);
                        offering.QIn = Q;

                        //[PIn]
                        offering.PIn = P;

                        //[PInBasic]
                        offering.PInBasic = P;

                        //[VatRateID]
                        offering.vat_rate = da.getZeroVatRate();

                        //[Markup]
                        offering.Markup = 0;

                        //[SpareIncomeID] - в зависимости от поля #5
                        string WarehouseMarker = list[i].ChildNodes[5].InnerText;
                        int SpareIncomeID = 61;
                        if (WarehouseMarker.Contains("истина"))
                            SpareIncomeID = 61;
                        else
                            SpareIncomeID = 62;
                        offering.spare_income = da.SpareIncomeGet(SpareIncomeID);

                        //[CurrencyID]
                        offering.CurrencyID = 1;

                        //[S]
                        offering.S = Q * P;

                        //[SBasic]
                        offering.SBasic = Q * P;

                        //[POut]
                        offering.POut = P;

                        //[POutBasic]
                        offering.POutBasic = P;

                        //[QRest]
                        offering.QRest = Q;
                        string mess = "";

                        //===================================================== [SpareID]
                        string SpareCode1C = list[i].ChildNodes[0].InnerText;
                        mess += "1C:[" + SpareCode1C + "] - ";

                        //  search by code1C
                        spare sp = null;
                        sp = da.GetSpare(SpareCode1C);
                        string code = "X";
                        if (sp == null)
                        {
                            mess += "not found! ";

                            // search by spare code
                            code = list[i].ChildNodes[1].InnerText;
                            mess += "CODE:[" + code + "] - ";
                            sp = da.GetSpareByCode(code);
                        }
                        string name = "X";
                        if (sp == null)
                        {
                            mess += "not found! ";

                            // search by spare name
                            name = list[i].ChildNodes[2].InnerText;
                            mess += "NAME:[" + name + "] - ";
                            sp = da.GetSpare(name);
                        }
                        if (sp == null)
                            m1 += SpareCode1C + ", " + code + ", " + name + "\n";
                        else
                            da.InOfferingCreate(offering, sp.id);
                    }
                }
            }
            MessageBox.Show(m1);

            // Закрываем поток
            fs.Close();
            return Message;
        }
    }
}