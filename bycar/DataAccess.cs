using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using bycar.Utils;
using bycar3.External_Code;

namespace bycar
{
    public class DataAccess
    {
        private DriveEntities objDataContext;
        private spare_group root = null;
        private unit u = null;
        private int spid = 0;

        public int SPID
        {
            get
            {
                var x = objDataContext.GetSPID();
                short? y = x.FirstOrDefault();
                spid = y.HasValue ? y.Value : 0;
                return spid;
            }
        }

        public DataAccess()
        {
            objDataContext = new DriveEntities();
            objDataContext.ContextOptions.LazyLoadingEnabled = true;
            root = objDataContext.spare_group.FirstOrDefault(i => i.id == 1);
            u = objDataContext.units.FirstOrDefault(i => i.id == 1);
        }

        public void FixIncomeQuantity(int SpareID, decimal NewQuantity)
        {
            objDataContext = new DriveEntities();
            SpareView spare = SpareContainer.Instance.Spares.FirstOrDefault(x => x.id == SpareID);
            decimal difference = NewQuantity - (decimal)spare.QRest;

            // обновить записи о поступлениях
            var incomes = from income in objDataContext.spare_in_spare_income where income.spare.id == SpareID orderby income.spare_income.si_date select income;
            if (incomes.Count() == 0)
                throw new Exception("Не найдено ни одного прихода по данному товару!");
            List<spare_in_spare_income> sisiList = incomes.ToList();

            // если было 0
            if (spare.QRest == 0)
            {
                spare_in_spare_income income = sisiList.First();
                income.QRest = NewQuantity;
                objDataContext.SaveChanges();
            }
            else
            {
                // если было >0
                List<spare_in_spare_income>.Enumerator incomeEnumerator = sisiList.GetEnumerator();

                // если надо увеличить количество
                if (difference > 0)
                {
                    incomeEnumerator.MoveNext();
                    incomeEnumerator.Current.QRest += difference;
                }
                else

                // если надо уменьшить количество
                {
                    // если новое количество = 0
                    if (NewQuantity <= 0)
                    {
                        while (incomeEnumerator.MoveNext())
                            incomeEnumerator.Current.QRest = 0;
                    }
                    else
                        while (difference != 0)
                        {
                            incomeEnumerator.MoveNext();

                            // если в последнем приходе больше, чем разница
                            if (incomeEnumerator.Current.QRest > (-1) * difference)
                            {
                                incomeEnumerator.Current.QRest += difference;
                                difference = 0;
                            }
                            else

                            // если в последнем приходе меньше, чем разница
                            {
                                difference += (decimal)incomeEnumerator.Current.QRest;
                                incomeEnumerator.Current.QRest = 0;
                            }
                        }
                }
                objDataContext.SaveChanges();
            }

            // обновить запись в кэше (в БД)
            //spare.QRest = (double)NewQuantity;
            SpareContainer.Instance.Update(SpareID, true);
        }

        public void SaleDelete(int SaleID)
        {
            objDataContext = new DriveEntities();

            // delete baskets
            List<Basket> baskets = objDataContext.Baskets.Where(x => x.SaleID == SaleID).ToList();
            List<int> spares = new List<int>();
            foreach (Basket basket in baskets)
            {
                // delete offerings in basket
                List<OfferingInBasketItem> links = objDataContext.OfferingInBasketItems.Where(x => x.BasketItemID == basket.ID).ToList();
                foreach (OfferingInBasketItem link in links)
                {
                    // delete spare in spare outgo
                    spare_in_spare_outgo siso = objDataContext.spare_in_spare_outgo.FirstOrDefault(x => x.id == link.OfferingOutgoID);
                    if (siso != null)
                    {
                        objDataContext.DeleteObject(siso);
                    }

                    // edit spare in spare income
                    spare_in_spare_income sisi = objDataContext.spare_in_spare_income.FirstOrDefault(x => x.id == link.OfferingIncomeID);
                    if (sisi != null)
                    {
                        sisi.QRest += link.Q;
                    }
                    objDataContext.DeleteObject(link);
                }
                spares.Add(basket.SpareID.Value);
                objDataContext.DeleteObject(basket);
            }

            //  delete sale
            Sale s = objDataContext.Sales.FirstOrDefault(x => x.ID == SaleID);
            if (s == null)
                return;
            objDataContext.DeleteObject(s);
            objDataContext.SaveChanges();
            foreach (int SpareID in spares)
                SpareContainer.Instance.Update(SpareID);
        }

        public Sale SaleCreate(Sale i)
        {
            if (objDataContext.Sales.Count() > 0)
                i.Number = objDataContext.Sales.Max(x => x.Number) + 1;
            else
                i.Number = 1;
            i.SaleDate = DateTime.Now;
            i.Comments = "";
            objDataContext.AddToSales(i);
            objDataContext.SaveChanges();
            return i;
        }

        public void BasketDelete(List<BasketView> baskets)
        {
            List<int> spares = new List<int>();
            foreach (BasketView bv in baskets)
            {
                Basket basket = objDataContext.Baskets.FirstOrDefault(x => x.ID == bv.ID);
                if (basket == null)
                    continue;

                // delete offerings in basket
                List<OfferingInBasketItem> links = objDataContext.OfferingInBasketItems.Where(x => x.BasketItemID == basket.ID).ToList();
                foreach (OfferingInBasketItem link in links)
                {
                    // delete spare in spare outgo
                    spare_in_spare_outgo siso = objDataContext.spare_in_spare_outgo.FirstOrDefault(x => x.id == link.OfferingOutgoID);
                    if (siso != null)
                    {
                        objDataContext.DeleteObject(siso);
                    }

                    // edit spare in spare income
                    spare_in_spare_income sisi = objDataContext.spare_in_spare_income.FirstOrDefault(x => x.id == link.OfferingIncomeID);
                    if (sisi != null)
                    {
                        sisi.QRest += link.Q;
                    }
                    objDataContext.DeleteObject(link);
                }
                spares.Add(basket.SpareID.Value);
                objDataContext.DeleteObject(basket);
            }
            objDataContext.SaveChanges();
            foreach (int SpareID in spares)
                SpareContainer.Instance.Update(SpareID);
            return;
        }

        public Basket BasketCreate(BasketView i)
        {
            // здесь
            // Create spare_in_spare_outgo - для каждого прихода, из которых берется товар в отгрузку
            // Edit spare_in_spare_income
            // Create OfferingInBasketItem
            int SpareID = i.SpareID.Value;

            // Create Basket
            Basket b = new Basket();
            b.CreatedOn = DateTime.Now;
            b.Pbyr = i.Pbyr;
            b.Peur = i.Peur;
            b.Prur = i.Prur;
            b.Pusd = i.Pusd;
            b.Q = i.Q;
            b.SaleID = i.SaleID;
            b.SpareID = SpareID;
            objDataContext.AddToBaskets(b);
            objDataContext.SaveChanges();

            decimal unallocatedQ = b.Q.Value;

            // Edit spare_in_spare_income
            List<SpareInSpareIncomeView> incomes = objDataContext.SpareInSpareIncomeViews.Where(x => (x.SpareID == b.SpareID && x.QRest > 0)).OrderBy(x => x.SpareIncomeDate).ToList();
            List<SpareInSpareIncomeView> LinkedIncomes = new List<SpareInSpareIncomeView>();
            List<decimal> Qs = new List<decimal>();
            foreach (SpareInSpareIncomeView income in incomes)
            {
                spare_in_spare_income inc = objDataContext.spare_in_spare_income.FirstOrDefault(x => x.id == income.id);
                if (income.QRest >= unallocatedQ)
                {
                    decimal q = unallocatedQ;
                    inc.QRest -= q;
                    objDataContext.SaveChanges();
                    Qs.Add(q);
                    LinkedIncomes.Add(income);
                    break;
                }
                else
                {
                    decimal q = inc.QRest.Value;
                    unallocatedQ -= q;
                    inc.QRest = 0;
                    objDataContext.SaveChanges();
                    Qs.Add(q);
                    LinkedIncomes.Add(income);
                }
            }
            SpareInSpareIncomeView ActualIncome = LinkedIncomes.OrderByDescending(x => x.SpareIncomeDate).FirstOrDefault();
            spare_outgo outgo = SpareOutgoOpened();
            foreach (SpareInSpareIncomeView income in LinkedIncomes)
            {
                int ind = LinkedIncomes.IndexOf(income);

                // Create spare_in_spare_outgo - для каждого прихода, из которых берется товар в отгрузку
                // при этом подставляем цены и проценты из первого прихода

                spare_in_spare_outgo siso = new spare_in_spare_outgo();

                // [basic_price]    - цена в базовой валюте
                // получим из профиля код базовой валюты
                settings_profile profile = getProfileCurrent();
                string BasicCurrencyCode = profile.BasicCurrencyCode;
                siso.basic_price = ActualIncome.POutBasic;
                /*decimal basic_price = 0;
                switch (BasicCurrencyCode)
                {
                    case "RUR":
                        basic_price = b.Prur.Value;
                        break;

                    case "BYR":
                        basic_price = b.Pbyr.Value;
                        break;

                    case "EUR":
                        basic_price = b.Peur.Value;
                        break;

                    case "USD":
                        basic_price = b.Pusd.Value;
                        break;
                }
                outgo.basic_price = basic_price;                */
                siso.description = "";
                siso.discount = 0;
                siso.markup_percentage = ActualIncome.Markup;
                siso.num = ind;

                // [purchase_price]     - указать цену отпускную в валюте накладной отгрузки
                decimal price = 0;
                if (outgo.currency == null)
                    outgo.currencyReference.Load();
                switch (outgo.currency.code)
                {
                    case "RUR":
                        price = b.Prur.Value;
                        break;

                    case "BYR":
                        price = b.Pbyr.Value;
                        break;

                    case "EUR":
                        price = b.Peur.Value;
                        break;

                    case "USD":
                        price = b.Pusd.Value;
                        break;
                }
                siso.purchase_price = price;
                siso.quantity = Qs[ind];
                siso.spare = objDataContext.spares.FirstOrDefault(x => x.id == SpareID);
                siso.spare_in_spare_income = objDataContext.spare_in_spare_income.FirstOrDefault(x => x.id == income.id);
                siso.spare_outgo = outgo;
                siso.total_sum = siso.quantity * siso.purchase_price.Value;
                siso.vat_rate = getZeroVatRate();
                objDataContext.AddTospare_in_spare_outgo(siso);
                objDataContext.SaveChanges();

                // Create OfferingInBasketItem
                OfferingInBasketItem o = new OfferingInBasketItem();
                o.BasketItemID = b.ID;
                o.OfferingIncomeID = income.id;
                o.OfferingOutgoID = siso.id;
                o.Q = Qs[ind];
                objDataContext.AddToOfferingInBasketItems(o);
                objDataContext.SaveChanges();
            }
            objDataContext.SaveChanges();
            return b;
        }

        #region ACCOUNT TYPES

        public void AccountTypeCreate(account_type _obj)
        {
            _obj.created_on = DateTime.Now;
            objDataContext.AddToaccount_type(_obj);
            objDataContext.SaveChanges();
        }

        public void AccountTypeEdit(account_type item)
        {
            account_type original = objDataContext.account_type.FirstOrDefault(b => b.id == item.id);
            if (original != null)
            {
                original.name = item.name;
                original.description = item.description;
                if (original.created_on == null)
                    original.created_on = DateTime.Now;
                objDataContext.SaveChanges();
            }
        }

        public void AccountTypeDelete(int id)
        {
            account_type original = objDataContext.account_type.FirstOrDefault(b => b.id == id);
            objDataContext.DeleteObject(original);
            objDataContext.SaveChanges();
        }

        public List<account_type> GetAllAccountTypes()
        {
            return objDataContext.account_type.ToList<account_type>();
        }

        #endregion ACCOUNT TYPES

        #region ACCOUNTS

        public int AccountCreate(account _obj)
        {
            _obj.created_on = DateTime.Now;
            objDataContext.AddToaccounts(_obj);
            objDataContext.SaveChanges();
            return _obj.id;
        }

        public int AccountCreate(account _obj, string bankName, string typeName)
        {
            _obj.created_on = DateTime.Now;

            //_obj.bank = objDataContext.banks.FirstOrDefault(c => c.name == bankName);
            //_obj.account_type = objDataContext.account_type.FirstOrDefault(c => c.name == typeName);
            objDataContext.AddToaccounts(_obj);
            objDataContext.SaveChanges();
            return _obj.id;
        }

        public void AccountEdit(account item)
        {
            account original = objDataContext.accounts.FirstOrDefault(b => b.id == item.id);
            original.name = item.name;
            original.address = item.address;

            //original.account_type = obj
            //original.admin_unit = item.mfo;
            //item.bank
            //original.bankReference.EntityKey = objDataContext.banks.FirstOrDefault(b => b.id == item.bank.id).EntityKey;
            //original.bank_score = item.bank_score;
            original.description = item.description;
            original.discount = item.discount;
            original.okpo = item.okpo;
            original.shipping_base = item.shipping_base;
            original.shipping_point = item.shipping_point;
            original.unn = item.unn;

            //if (original.created_on == null)
            //    original.created_on = DateTime.Now;
            objDataContext.SaveChanges();
        }

        public spare SpareCreateSilent(string Name, string CodeShatem, string GroupName, string ParentGroupName, string BrandName, string UnitName, string Description)
        {
            // получим группу
            //spare_group ParentGroup = objDataContext.spare_group.FirstOrDefault(i => i.id == 1).spare_group1.FirstOrDefault(x => x.name.ToUpper() == ParentGroupName.ToUpper());
            List<spare_group> groups = objDataContext.spare_group.Where(i => i.ParentGroup.id == 1).ToList();   // все группы, дочерние группе #1
            groups = groups.Where(i => i.name.ToUpper().Equals(ParentGroupName.ToUpper())).ToList();
            spare_group ParentGroup = null;
            if (groups.Count > 0)
                ParentGroup = groups[0];
            if (ParentGroup == null)
            {
                // создать новую группу
                spare_group sgNew = new spare_group();
                sgNew.name = ParentGroupName;
                spare_group root = objDataContext.spare_group.FirstOrDefault(x => x.id == 1);
                sgNew.ParentGroup = root;
                sgNew.IsBrand = false;
                sgNew.ChildCount = 0;
                sgNew.code1C = "";
                sgNew.description = "";
                sgNew.ParentGroupName = root.name;
                objDataContext.AddTospare_group(sgNew);
                objDataContext.SaveChanges();
                ParentGroup = sgNew;
            }

            // получим подгруппу
            //spare_group group = objDataContext.spare_group.FirstOrDefault(i => i.id == ParentGroup.id).spare_group1.FirstOrDefault(x => x.name.ToUpper() == GroupName.ToUpper());
            groups = objDataContext.spare_group.Where(i => i.ParentGroup.id == ParentGroup.id).ToList();
            groups = groups.Where(i => i.name.ToUpper().Equals(GroupName.ToUpper())).ToList();
            spare_group group = null;
            if (groups.Count > 0)
                group = groups[0];
            if (group == null)
            {
                // создать новую группу
                spare_group sgNew = new spare_group();
                sgNew.name = GroupName;
                spare_group parent = objDataContext.spare_group.FirstOrDefault(x => x.id == ParentGroup.id);
                sgNew.ParentGroup = parent;
                sgNew.IsBrand = false;
                sgNew.ChildCount = 0;
                sgNew.code1C = "";
                sgNew.description = "";
                sgNew.ParentGroupName = parent.name;
                parent.ChildCount++;
                objDataContext.AddTospare_group(sgNew);
                objDataContext.SaveChanges();
                group = sgNew;
            }

            // получим брэнд
            brand b = objDataContext.brands.FirstOrDefault(i => i.name == BrandName);
            if (b == null)
            {
                brand bNew = new brand();
                bNew.name = BrandName;
                objDataContext.AddTobrands(bNew);
                objDataContext.SaveChanges();
                b = bNew;
            }

            // получим unit
            unit u = objDataContext.units.FirstOrDefault(x => x.name == UnitName);
            if (u == null)
            {
                unit bNew = new unit();
                bNew.name = UnitName;
                objDataContext.AddTounits(bNew);
                objDataContext.SaveChanges();
                u = bNew;
            }
            spare sp = new spare();
            sp.name = Name;
            sp.code = "";
            sp.codeShatem = CodeShatem;
            sp.q_demand = 0;
            sp.q_demand_clear = 0;
            sp.q_rest = 0;
            sp.description = Description;
            sp.spare_group1 = ParentGroup;
            sp.spare_group = group;
            int GroupID = group.id;
            sp.brand = b;
            sp.unit = u;

            // [spare_group2_id]
            if (sp.spare_group1.ParentGroup != null)
            {
                spare_group group2 = objDataContext.spare_group.FirstOrDefault(x => x.id == sp.spare_group1.ParentGroup.id);
                if (group2 != null)
                {
                    sp.spare_group2 = group2;

                    // [spare_group3_id]
                    if (group2.ParentGroup != null)
                    {
                        spare_group group3 = objDataContext.spare_group.FirstOrDefault(x => x.id == group2.ParentGroup.id);
                        if (group3 != null)
                            sp.spare_group3 = group3;
                    }
                }
            }

            // [BrandName]
            sp.BrandName = sp.brand.name;

            // [GroupName]
            sp.GroupName = sp.spare_group.name;

            // [ParentGroupName]
            sp.ParentGroupName = sp.spare_group1.name;
            objDataContext.AddTospares(sp);
            objDataContext.SaveChanges();

            //spare s = SpareCreate(sp, sp.brand.id, sp.spare_group.id, b.id);
            return sp;
        }

        public void SpareEdit(DataTable dt)
        {
            //account original = objDataContext.accounts.FirstOrDefault(b => b.id == item.id);
            int cnt = dt.Rows.Count - 1;
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 1000; j++)
                {
                    string CodeShatem = dt.Rows[cnt][0].ToString();
                    string Code = dt.Rows[cnt][1].ToString();
                    spare original = objDataContext.spares.FirstOrDefault(b => b.code == CodeShatem);
                    if (original != null)
                    {
                        original.code = Code;
                        original.codeShatem = CodeShatem;
                        Console.WriteLine(cnt.ToString() + ": " + CodeShatem + " ok!");
                    }
                    else
                        Console.WriteLine(cnt.ToString() + ": " + CodeShatem + " not found.");
                    cnt--;
                }
                objDataContext.SaveChanges();
                Console.WriteLine("Commit.");
            }
        }

        /*=
         * counter++;
                            GlobalCnt++;
                            string[] RawRecord = csvData.RawRecord.Split(';');

                            // delete chars after _
                            CodeShatem = CodeShatem.Split('_')[0];

                            if (da.SpareEdit(CodeShatem, Code))
                                Console.WriteLine(GlobalCnt.ToString() + ": " + CodeShatem + " ok!");
                            else
                                Console.WriteLine(GlobalCnt.ToString() + ": " + CodeShatem + " not found.");

                            if (counter > 1000)
                            {
                                counter = 0;
                                da = new DataAccess();
                                Console.WriteLine("Data access object refresh");
                            }*/

        public void AccountEdit(account item, string bankName, string typeName)
        {
            account original = objDataContext.accounts.FirstOrDefault(b => b.id == item.id);

            //original.bank = objDataContext.banks.FirstOrDefault(c => c.name == bankName);
            //original.account_type = objDataContext.account_type.FirstOrDefault(c => c.name == typeName);
            original.name = item.name;
            original.address = item.address;

            //original.bank_score = item.bank_score;
            original.description = item.description;
            original.discount = item.discount;
            original.okpo = item.okpo;
            original.shipping_base = item.shipping_base;
            original.shipping_point = item.shipping_point;
            original.unn = item.unn;

            //if (original.created_on == null)
            //    original.created_on = DateTime.Now;
            objDataContext.SaveChanges();
        }

        public List<account> GetAllAccounts()
        {
            return objDataContext.accounts.ToList<account>();
        }

        public List<AccountView> GetAllAccountViews()
        {
            return objDataContext.AccountViews.ToList();
        }

        public void AccountDelete(int id)
        {
            account a = objDataContext.accounts.FirstOrDefault(o => o.id == id);
            if (a != null)
            {
                // удалить прикрепленные счета
                DeleteRelatedBankAccount(a.id);

                // обнулить прикрепелённые приходы
                NullizeRelatedIncomes(a.id);

                // обнулить прикрепелённые отгрузки
                NullizeRelatedOutgoes(a.id);

                // обнулить прикрепелённые инвойсы
                NullizeRelatedInvoices(a.id);

                // сохраняем
                objDataContext.DeleteObject(a);
                objDataContext.SaveChanges();
            }
        }

        private void DeleteRelatedBankAccount(int AccountID)
        {
            bank_account item = objDataContext.bank_account.FirstOrDefault(i => i.account.id == AccountID);
            while (item != null)
            {
                objDataContext.DeleteObject(item);
                objDataContext.SaveChanges();
                item = objDataContext.bank_account.FirstOrDefault(i => i.account.id == AccountID);
            }
        }

        private void NullizeRelatedIncomes(int AccountID)
        {
            List<spare_income> items = objDataContext.spare_income.Where(i => i.account.id == AccountID).ToList();
            foreach (spare_income i in items)
            {
                i.account = null;
            }
            objDataContext.SaveChanges();
        }

        private void NullizeRelatedOutgoes(int AccountID)
        {
            List<spare_outgo> items = objDataContext.spare_outgo.Where(i => i.AccountID == AccountID).ToList();
            foreach (spare_outgo i in items)
            {
                i.AccountID = null;
            }
            objDataContext.SaveChanges();
        }

        private void NullizeRelatedInvoices(int AccountID)
        {
            List<invoice> items = objDataContext.invoices.Where(i => i.account.id == AccountID).ToList();
            foreach (invoice i in items)
            {
                i.account = null;
            }
            objDataContext.SaveChanges();
        }

        public account GetAccount(int id)
        {
            return objDataContext.accounts.FirstOrDefault(i => i.id == id);
        }

        public List<account> GetAccounts(int searchFieldIndex, string searchString)
        {
            List<account> items = new List<account>();
            switch (searchFieldIndex)
            {
                case 0:// ПОИСК ПО НАИМЕНОВАНИЮ
                    items = objDataContext.accounts.Where(s => s.name.ToLower().Contains(searchString.ToLower())).ToList();
                    break;

                case 1:// ПОИСК ПО ОПИСАНИЮ(ПРИМЕЧАНИЮ)
                    items = objDataContext.accounts.Where(s => s.description.ToLower().Contains(searchString.ToLower())).ToList();
                    break;
            }
            var sorted = from s in items orderby s.name select s;
            items = sorted.ToList();
            return items;
        }

        #endregion ACCOUNTS

        #region Admin Units

        public List<admin_unit> GetAdminUnits()
        {
            return objDataContext.admin_unit.ToList<admin_unit>();
        }

        public bool UserNameExist(string username)
        {
            if (objDataContext.admin_unit.FirstOrDefault(x => x.name.ToUpper().Equals(username.ToUpper())) != null)
                return true;
            else
                return false;
        }

        public void AdminUnitCreate(admin_unit _obj)
        {
            objDataContext.AddToadmin_unit(_obj);
            objDataContext.SaveChanges();
        }

        public admin_unit AdminUnitGet(int ID)
        {
            return objDataContext.admin_unit.FirstOrDefault(x => x.id == ID);
        }

        public int AdminUnitsCount()
        {
            return objDataContext.admin_unit.Count();
        }

        public void AdminUnitEdit(admin_unit item)
        {
            admin_unit original = objDataContext.admin_unit.FirstOrDefault(b => b.id == item.id);
            if (original != null)
            {
                original.name = item.name;
                original.password = item.password;
                original.is_admin = item.is_admin;
                objDataContext.SaveChanges();
            }
        }

        public void AdminUnitDelete(int id)
        {
            admin_unit b = objDataContext.admin_unit.FirstOrDefault(o => o.id == id);
            objDataContext.DeleteObject(b);
            objDataContext.SaveChanges();
        }

        public void AdminUnitDelete(string name)
        {
            admin_unit b = objDataContext.admin_unit.FirstOrDefault(o => o.name == name);
            objDataContext.DeleteObject(b);
            objDataContext.SaveChanges();
        }

        #endregion Admin Units

        #region BANKS

        public void BankCreate(bank _obj)
        {
            _obj.created_on = DateTime.Now;
            objDataContext.AddTobanks(_obj);
            objDataContext.SaveChanges();
        }

        public bank BankGet(int ID)
        {
            return objDataContext.banks.FirstOrDefault(i => i.id == ID);
        }

        public void BankEdit(bank item)
        {
            bank original = objDataContext.banks.FirstOrDefault(b => b.id == item.id);
            original.name = item.name;
            original.address = item.address;
            original.fax = item.fax;
            original.mfo = item.mfo;
            original.phone = item.phone;
            if (original.created_on == null)
                original.created_on = DateTime.Now;
            objDataContext.SaveChanges();
        }

        public List<bank> GetAllBanks()
        {
            return objDataContext.banks.ToList<bank>();
        }

        public void BankDelete(int id)
        {
            bank bnk = objDataContext.banks.FirstOrDefault(b => b.id == id);
            objDataContext.DeleteObject(bnk);
            objDataContext.SaveChanges();
        }

        public List<bank> GetBanks(int searchFieldIndex, string searchString)
        {
            List<bank> items = new List<bank>();
            switch (searchFieldIndex)
            {
                case 0:// ПОИСК ПО НАИМЕНОВАНИЮ
                    items = objDataContext.banks.Where(s => s.name.ToLower().Contains(searchString.ToLower())).ToList();
                    break;

                case 1:// ПОИСК ПО ОПИСАНИЮ(ПРИМЕЧАНИЮ)
                    items = objDataContext.banks.Where(s => s.mfo.ToLower().Contains(searchString.ToLower())).ToList();
                    items.AddRange(objDataContext.banks.Where(s => s.address.ToLower().Contains(searchString.ToLower())).ToList());
                    break;
            }
            var sorted = from s in items orderby s.name select s;
            items = sorted.ToList();
            return items;
        }

        #endregion BANKS

        public List<BankAccountView> BankAccountViewGet(int AccountID)
        {
            List<BankAccountView> items = objDataContext.BankAccountViews.Where(i => i.AccountID == AccountID).ToList();
            return items;
        }

        public List<BankAccountView> BankAccountViewGet(int searchFieldIndex, string searchString, int AccountID)
        {
            List<BankAccountView> items = new List<BankAccountView>();
            if (AccountID > 0)
            {
                items = objDataContext.BankAccountViews.Where(s => s.AccountID == AccountID).ToList();
            }
            switch (searchFieldIndex)
            {
                case 0:// ПОИСК ПО НАИМЕНОВАНИЮ
                    items = objDataContext.BankAccountViews.Where(s => s.BankAccount.ToLower().Contains(searchString.ToLower())).ToList();
                    break;

                case 1:// ПОИСК ПО ОПИСАНИЮ(ПРИМЕЧАНИЮ)
                    items = objDataContext.BankAccountViews.Where(s => s.BankName.ToLower().Contains(searchString.ToLower())).ToList();
                    break;
            }
            var sorted = from s in items orderby s.BankAccount select s;
            items = sorted.ToList();
            return items;
        }

        public BankAccountView BankAccountGet(int BankAccountID)
        {
            return objDataContext.BankAccountViews.FirstOrDefault(i => i.id == BankAccountID);
        }

        public bank_account bank_account_get(int ID)
        {
            return objDataContext.bank_account.FirstOrDefault(i => i.id == ID);
        }

        public BankAccountView BankAccountView(int ID)
        {
            return objDataContext.BankAccountViews.FirstOrDefault(i => i.id == ID);
        }

        public void BankAccountDelete(int BAID)
        {
            bank_account ba = objDataContext.bank_account.FirstOrDefault(i => i.id == BAID);
            objDataContext.DeleteObject(ba);
            objDataContext.SaveChanges();
        }

        public bank_account BankAccountCreate(bank_account i)
        {
            objDataContext.AddTobank_account(i);
            objDataContext.SaveChanges();
            return i;
        }

        public bank_account BankAccountEdit(bank_account x)
        {
            bank_account o = objDataContext.bank_account.FirstOrDefault(i => i.id == x.id);
            if (o != null)
            {
                //o.account = x.account;
                o.bank = x.bank;
                o.BankAccount = x.BankAccount;
                o.Description = x.Description;
                objDataContext.SaveChanges();
            }
            return o;
        }

        public bank_account BankAccountEdit(BankAccountView x)
        {
            bank_account o = objDataContext.bank_account.FirstOrDefault(i => i.id == x.id);
            if (o != null)
            {
                //o.account = x.account;
                o.bank = BankGet(x.BankID);
                o.BankAccount = x.BankAccount;
                o.Description = x.Description;
                objDataContext.SaveChanges();
            }
            return o;
        }

        public void ProfileBankAccountCreate(ProfileBankAccount a)
        {
            if (a.IsMain == 1) // если это новый основной, остальные обнулим
                UndoMainBankAccounts();
            objDataContext.AddToProfileBankAccounts(a);
            objDataContext.SaveChanges();
        }

        public void ProfileBankAccountEdit(ProfileBankAccount a)
        {
            ProfileBankAccount o = objDataContext.ProfileBankAccounts.FirstOrDefault(i => i.id == a.id);
            o.bank = a.bank;
            o.Description = a.Description;
            if (a.IsMain == 1) // если это новый основной, остальные обнулим
            {
                UndoMainBankAccounts();
                o.IsMain = 1;
            }
            else
                o.IsMain = a.IsMain;

            //o.ProfileID
            objDataContext.SaveChanges();
        }

        private void UndoMainBankAccounts()
        {
            foreach (ProfileBankAccount a in objDataContext.ProfileBankAccounts)
                a.IsMain = 0;

            //       objDataContext.SaveChanges();
        }

        public List<ProfileBankAccountView> ProfileBankAccountViewGet()
        {
            return objDataContext.ProfileBankAccountViews.ToList();
        }

        public ProfileBankAccount ProfileBankAccountGet(int ID)
        {
            return objDataContext.ProfileBankAccounts.FirstOrDefault(i => i.id == ID);
        }

        public void ProfileBankAccountDelete(int ID)
        {
            ProfileBankAccount ba = objDataContext.ProfileBankAccounts.FirstOrDefault(i => i.id == ID);
            objDataContext.DeleteObject(ba);
            objDataContext.SaveChanges();
        }

        #region BRANDS

        public List<brand> GetBrands()
        {
            return objDataContext.brands.ToList<brand>();
        }

        public List<brand> GetBrands(int SearchFieldIndex, string SearchText)
        {
            if (SearchText.Length < 1)
                return objDataContext.brands.ToList<brand>();
            switch (SearchFieldIndex)
            {
                case 0:
                    return objDataContext.brands.Where(x => x.name.Contains(SearchText)).ToList<brand>();
                case 1:
                    return objDataContext.brands.Where(x => x.description.Contains(SearchText)).ToList<brand>();
                default:
                    return objDataContext.brands.ToList<brand>();
            }
        }

        public brand GetBrand(int id)
        {
            return objDataContext.brands.FirstOrDefault(i => i.id == id);
        }

        public brand GetBrand(string name)
        {
            return objDataContext.brands.FirstOrDefault(i => i.name == name);
        }

        public int GetBrandIDByName(string Name)
        {
            return objDataContext.brands.FirstOrDefault(i => i.name == Name).id;
        }

        public brand BrandCreate(brand _obj)
        {
            //_obj.created_on = DateTime.Now;
            objDataContext.AddTobrands(_obj);
            objDataContext.SaveChanges();
            return _obj;
        }

        public void BrandEdit(brand item)
        {
            brand original = objDataContext.brands.FirstOrDefault(b => b.id == item.id);
            if (original != null)
            {
                original.name = item.name;
                original.description = item.description;

                //if (original.created_on == null)
                //    original.created_on = DateTime.Now;
                objDataContext.SaveChanges();
            }
        }

        public void BrandDelete(int id)
        {
            brand b = objDataContext.brands.FirstOrDefault(o => o.id == id);
            objDataContext.DeleteObject(b);
            objDataContext.SaveChanges();
        }

        public void BrandDelete(string name)
        {
            brand b = objDataContext.brands.FirstOrDefault(o => o.name == name);
            objDataContext.DeleteObject(b);
            objDataContext.SaveChanges();
        }

        #endregion BRANDS

        #region CAR Producers

        public List<car_producer> GetCarProducers()
        {
            return objDataContext.car_producer.ToList<car_producer>();
        }

        public car_producer GetCarProducer(int ID)
        {
            return objDataContext.car_producer.FirstOrDefault(i => i.id == ID);
        }

        public List<car_producer> GetCarProducers(int searchFieldIndex, string searchString)
        {
            List<car_producer> items = new List<car_producer>();
            switch (searchFieldIndex)
            {
                case 0:// ПОИСК ПО НАИМЕНОВАНИЮ
                    items = objDataContext.car_producer.Where(s => s.name.ToLower().Contains(searchString.ToLower())).ToList();
                    break;

                case 1:// ПОИСК ПО ОПИСАНИЮ(ПРИМЕЧАНИЮ)
                    items = objDataContext.car_producer.Where(s => s.descripton.ToLower().Contains(searchString.ToLower())).ToList();
                    break;
            }
            var sorted = from s in items orderby s.name select s;
            items = sorted.ToList();
            return items;
        }

        public void CarProducerCreate(car_producer _obj)
        {
            //_obj.created_on = DateTime.Now;
            objDataContext.AddTocar_producer(_obj);
            objDataContext.SaveChanges();
        }

        public void CarProducerEdit(car_producer item)
        {
            car_producer original = objDataContext.car_producer.FirstOrDefault(b => b.id == item.id);
            if (original != null)
            {
                original.name = item.name;
                original.descripton = item.descripton;

                //if (original.created_on == null)
                //    original.created_on = DateTime.Now;
                objDataContext.SaveChanges();
            }
        }

        public void CarProducerDelete(int id)
        {
            car_producer b = objDataContext.car_producer.FirstOrDefault(o => o.id == id);
            objDataContext.DeleteObject(b);
            objDataContext.SaveChanges();
        }

        public void CarProducerDelete(string name)
        {
            car_producer b = objDataContext.car_producer.FirstOrDefault(o => o.name == name);
            objDataContext.DeleteObject(b);
            objDataContext.SaveChanges();
        }

        #endregion CAR Producers

        #region CAR Marks

        public List<car_mark> GetCarMarks()
        {
            return objDataContext.car_mark.ToList<car_mark>();
        }

        public void CarMarkCreate(car_mark _obj)
        {
            //_obj.created_on = DateTime.Now;
            objDataContext.AddTocar_mark(_obj);
            objDataContext.SaveChanges();
        }

        public void CarMarkCreate(car_mark _obj, string prodName)
        {
            _obj.car_producer = objDataContext.car_producer.FirstOrDefault(i => i.name == prodName);
            objDataContext.AddTocar_mark(_obj);
            objDataContext.SaveChanges();
        }

        public void CarMarkEdit(car_mark item)
        {
            car_mark original = objDataContext.car_mark.FirstOrDefault(b => b.id == item.id);
            if (original != null)
            {
                original.name = item.name;

                //original.description = item.description;
                //if (original.created_on == null)
                //    original.created_on = DateTime.Now;
                objDataContext.SaveChanges();
            }
        }

        public void CarMarkEdit(car_mark item, string prodName)
        {
            car_mark original = objDataContext.car_mark.FirstOrDefault(b => b.id == item.id);
            if (original != null)
            {
                original.name = item.name;
                original.description = item.description;
                original.car_producer = objDataContext.car_producer.FirstOrDefault(i => i.name == prodName);
                objDataContext.SaveChanges();
            }
        }

        public void CarMarkDelete(int id)
        {
            car_mark b = objDataContext.car_mark.FirstOrDefault(o => o.id == id);
            objDataContext.DeleteObject(b);
            objDataContext.SaveChanges();
        }

        public void CarMarkDelete(string name)
        {
            car_mark b = objDataContext.car_mark.FirstOrDefault(o => o.name == name);
            objDataContext.DeleteObject(b);
            objDataContext.SaveChanges();
        }

        #endregion CAR Marks

        #region CLIENTS

        public List<client> GetClients()
        {
            return objDataContext.clients.ToList<client>();
        }

        public void ClientCreate(client _obj)
        {
            //_obj.created_on = DateTime.Now;
            objDataContext.AddToclients(_obj);
            objDataContext.SaveChanges();
        }

        public void ClientCreate(client _obj, string carMarkName)
        {
            _obj.car_mark = objDataContext.car_mark.FirstOrDefault(c => c.name == carMarkName);
            objDataContext.AddToclients(_obj);
            objDataContext.SaveChanges();
        }

        public void ClientEdit(client item)
        {
            client original = objDataContext.clients.FirstOrDefault(b => b.id == item.id);
            original.name = item.name;
            original.description = item.description;
            objDataContext.SaveChanges();
        }

        public void ClientEdit(client item, string carMarkName)
        {
            client original = objDataContext.clients.FirstOrDefault(b => b.id == item.id);
            original.car_mark = objDataContext.car_mark.FirstOrDefault(c => c.name == carMarkName);
            original.name = item.name;
            original.description = item.description;
            objDataContext.SaveChanges();
        }

        public void ClientDelete(int id)
        {
            client c = objDataContext.clients.FirstOrDefault(o => o.id == id);
            objDataContext.DeleteObject(c);
            objDataContext.SaveChanges();
        }

        public void ClientDelete(string name)
        {
            client c = objDataContext.clients.FirstOrDefault(o => o.name == name);
            objDataContext.DeleteObject(c);
            objDataContext.SaveChanges();
        }

        #endregion CLIENTS

        /*

        #region CONTACTS

        public void ContactCreate(contact _obj)
        {
            //_obj.created_on = DateTime.Now;
            objDataContext.AddTocontacts(_obj);
            objDataContext.SaveChanges();
        }
        public void ContactEdit(contact item)
        {
            contact original = objDataContext.contacts.FirstOrDefault(b => b.id == item.id);
            original.name = item.name;
            original.passort_date = item.passort_date;
            original.passport_by = item.passport_by;
            original.passport_number = item.passport_number;
            original.passport_series = item.passport_series;
            objDataContext.SaveChanges();
        }
        public List<contact> GetContacts()
        {
            return objDataContext.contacts.ToList<contact>();
        }
        public void ContactDelete(int id)
        {
            contact c = objDataContext.contacts.FirstOrDefault(o => o.id == id);
            objDataContext.DeleteObject(c);
            objDataContext.SaveChanges();
        }
        public void ContactDelete(string name)
        {
            contact c = objDataContext.contacts.FirstOrDefault(o => o.name == name);
            objDataContext.DeleteObject(c);
            objDataContext.SaveChanges();
        }

        #endregion CONTACTS

        */

        #region Currency

        public List<currency> GetCurrency()
        {
            return objDataContext.currencies.ToList<currency>();
        }

        public currency GetCurrency(int CurrencyID)
        {
            return objDataContext.currencies.FirstOrDefault(s => s.id == CurrencyID);
        }

        public currency GetCurrency(string code)
        {
            return objDataContext.currencies.FirstOrDefault(s => s.code.Contains(code));
        }

        public void CurrencyCreate(currency _obj)
        {
            //_obj.created_on = DateTime.Now;
            objDataContext.AddTocurrencies(_obj);
            objDataContext.SaveChanges();
            if (_obj.is_basic > 0)
            {
                SetBasicCurrency(_obj.id);
            }
        }

        public decimal getLastUsdToByr()
        {
            currency_rate r = objDataContext.currency_rate.OrderByDescending(i => i.rate_date).FirstOrDefault(c => c.currency.code.Contains("USD"));
            if (r != null)
                return r.rate;
            else
                return 0;
        }

        public decimal getLastEurToByr()
        {
            currency_rate r = objDataContext.currency_rate.OrderByDescending(i => i.rate_date).FirstOrDefault(c => c.currency.code.Contains("EUR"));
            if (r != null)
                return r.rate;
            else
                return 0;
        }

        public decimal getLastRurToByr()
        {
            currency_rate r = objDataContext.currency_rate.OrderByDescending(i => i.rate_date).FirstOrDefault(c => c.currency.code.Contains("RUR"));
            if (r != null)
                return r.rate;
            else
                return 0;
        }

        private void SetBasicCurrency(int id)
        {
            foreach (currency cur in objDataContext.currencies)
            {
                if (cur.id == id)
                    cur.is_basic = 1;
                else
                    cur.is_basic = 0;
            }
        }

        public bool AreCurrenciesUpToDate()
        {
            List<currency_rate> curs = (from c in objDataContext.currency_rate
                                        where ((c.rate_date.Day == DateTime.Now.Day)
                                                 && (c.rate_date.Month == DateTime.Now.Month)
                                                 && (c.rate_date.Year == DateTime.Now.Year))
                                        select c).ToList();
            /*List<currency_rate> curs0 = (from c in objDataContext.currency_rate select c).ToList();
            List<currency_rate> curs1 = (from c in curs0
                                         where (c.rate_date.Year == DateTime.Now.Year)
                                        select c).ToList();
            List<currency_rate> curs2 = (from c in curs1
                                         where (c.rate_date.Month == DateTime.Now.Month)
                                         select c).ToList();
            List<currency_rate> curs = (from c in curs2
                                         where (c.rate_date.Day == DateTime.Now.Day)
                                         select c).ToList();*/
            if (curs.Count > 2)
                return true;
            else
                return false;
        }

        public void CurrencyEdit(currency i)
        {
            currency original = objDataContext.currencies.FirstOrDefault(b => b.id == i.id);
            if (original != null)
            {
                original.name = i.name;
                original.short_name = i.short_name;
                original.code = i.code;
                original.is_basic = i.is_basic;
                if (i.is_basic > 0)
                {
                    SetBasicCurrency(original.id);
                }
                objDataContext.SaveChanges();
            }
        }

        public void CurrencyDelete(int id)
        {
            currency b = objDataContext.currencies.FirstOrDefault(o => o.id == id);
            objDataContext.DeleteObject(b);
            objDataContext.SaveChanges();
        }

        public void CurrencyDelete(string name)
        {
            currency b = objDataContext.currencies.FirstOrDefault(o => o.name == name);
            objDataContext.DeleteObject(b);
            objDataContext.SaveChanges();
        }

        #endregion Currency

        #region Currency Rates

        public currency_rate getCurrencyRate(string CurrencyCode, DateTime dt)
        {
            currency_rate rate = null;
            List<currency_rate> rates = (from r in objDataContext.currency_rate
                                         where (r.currency.code.Contains(CurrencyCode)
                                                && r.rate_date.Year == dt.Year
                                                && r.rate_date.Month == dt.Month
                                                && r.rate_date.Day == dt.Day)
                                         select r).ToList();
            /*if (rates.Count == 0)
            {
                rates = (from r in objDataContext.currency_rate
                         where r.currency.code.Contains(CurrencyCode)
                         orderby r.rate_date descending
                         select r).ToList();
                rate = rates[0];
            }*/
            if (rates.Count > 0)
                rate = rates[0];
            return rate;
        }

        public currency_rate getCurrencyRate(string CurrencyCode)
        {
            currency_rate rate = null;
            List<currency_rate> rates = (from r in objDataContext.currency_rate
                                         where (r.currency.code.Contains(CurrencyCode) && r.rate > 0 )
                                         orderby r.rate_date descending
                                         select r).ToList();
            if (rates.Count > 0)
                rate = rates[0];
            return rate;
        }
        public decimal getCurrencyRateValue(string CurrencyCode)
        {
            decimal rate = 1; // default value
            List<currency_rate> rates = (from r in objDataContext.currency_rate
                                         where (r.currency.code.Contains(CurrencyCode) && r.rate > 0)
                                         orderby r.rate_date descending
                                         select r).ToList();
            if (rates.Count > 0)
                rate = rates[0].rate;
            return rate;
        }

        public currency_rate getLastCurrencyRateByCode(string CurrencyName)
        {
            return objDataContext.currency_rate.Where(i => i.currency.code.Equals(CurrencyName)).OrderByDescending(s => s.rate_date).FirstOrDefault();
        }

        public string getBasicCurrencyCode()
        {
            string RES = "";
            currency c = objDataContext.currencies.FirstOrDefault(s => s.is_basic == 1);
            if (c != null)
                return c.code;
            else
                return RES;
        }

        public string getDefaultIncomeCurrencyCode()
        {
            string RES = "";
            currency c = objDataContext.currencies.FirstOrDefault(s => s.is_basic == 1);
            if (c != null)
                return c.code;
            else
                return RES;
        }

        public void UpdateBasicCurrencyCode(string NewBasicCurrencyCode)
        {
            objDataContext.currencies.FirstOrDefault(s => s.is_basic == 1).is_basic = 0;
            objDataContext.currencies.FirstOrDefault(s => s.code.Equals(NewBasicCurrencyCode)).is_basic = 1;
            objDataContext.SaveChanges();
        }

        public void RecalculateBasics(string OldBasicCode)
        {
            // пересчитать приходы
            // получить курс старой базовой к новой
            currency_rate r = getLastCurrencyRateByCode(OldBasicCode);
            foreach (spare_in_spare_income s in objDataContext.spare_in_spare_income)
            {
                // P in
                decimal PInBasic = s.PInBasic.Value;
                s.PInBasic = CurrencyHelper.GetBasicPrice(OldBasicCode, PInBasic);

                // P out
                decimal M = (decimal)s.Markup;
                if (s.vat_rate == null)
                    s.vat_rateReference.Load();
                decimal V = (decimal)s.vat_rate.rate;
                s.POutBasic = s.PInBasic * (1 + M / 100 + V / 100);

                // calculate Sum in basic
                s.SBasic = s.QIn * s.POutBasic.Value;
            }
            objDataContext.SaveChanges();
        }

        public void CurrencyRateCreate(currency_rate _obj)
        {
            objDataContext.AddTocurrency_rate(_obj);
            objDataContext.SaveChanges();
        }

        public void CurrencyRatesCreate(List<currency_rate> lst)
        {
            foreach (currency_rate r in lst)
            {
                objDataContext.AddTocurrency_rate(r);
            }
            objDataContext.SaveChanges();
        }

        public void CurrencyRatesEdit(List<currency_rate> lst)
        {
            objDataContext = new DriveEntities();
            foreach (currency_rate r in lst)
            {
                currency_rate o = getCurrencyRate(r.currency.code, r.rate_date);
                if (o != null)
                    o.rate = r.rate;
            }
            objDataContext.SaveChanges();
        }

        public void CurrencyRateEdit(currency_rate item)
        {
            currency_rate original = objDataContext.currency_rate.FirstOrDefault(b => b.id == item.id);
            if (original != null)
            {
                original.rate_date = item.rate_date;
                original.rate = item.rate;
                objDataContext.SaveChanges();
            }
        }

        public void CurrencyRateDelete(int id)
        {
            currency_rate b = objDataContext.currency_rate.FirstOrDefault(o => o.id == id);
            objDataContext.DeleteObject(b);
            objDataContext.SaveChanges();
        }

        #endregion Currency Rates

        #region SPARES

        public List<SpareView> GetSparesDemand()
        {
            List<SpareView> items1 = new List<SpareView>();
            List<SpareView> items = new List<SpareView>();
            items = SpareContainer.Instance.Spares.Where(i => i.q_demand.HasValue).Where(o => o.q_demand > 0).ToList();
            if (items.Count == 0)
                return items1;
            items1 = items.Where(i => (i.QRest.Value < (double)i.q_demand.Value) || (i.QRest == null)).ToList();
            return items1;
        }

        public List<SpareView> GetSpares()
        {
            return (from s in objDataContext.SpareViews select s).ToList();
        }

        public List<SpareView> GetSparesExt()
        {
            return objDataContext.GetSpareViews(null).OrderBy(s => s.codeShatem).ToList();
        }

        public List<SpareView> GetSparesAvailable()
        {
            return (from s in objDataContext.GetSpareViews(null) where s.QRest > 0 select s).ToList();
        }

        public spare GetSpare(int id)
        {
            return objDataContext.spares.FirstOrDefault(s => s.id == id);
        }

        public spare GetSpare(string Code1C)
        {
            if (objDataContext.spares.Where(s => s.code1C == Code1C).Count() > 0)
                return objDataContext.spares.FirstOrDefault(s => s.code1C == Code1C);
            else
                return null;
        }

        public spare GetSpareByCode(string Code)
        {
            if (objDataContext.spares.Where(s => s.code == Code).Count() > 0)
                return objDataContext.spares.FirstOrDefault(s => s.code == Code);
            else
                return null;
        }

        public spare GetSpareByName(string Name)
        {
            if (objDataContext.spares.Where(s => s.name == Name).Count() > 0)
                return objDataContext.spares.FirstOrDefault(s => s.name == Name);
            else
                return null;
        }

        public SpareView GetSpareView(int id)
        {
            objDataContext = new DriveEntities();
            return objDataContext.SpareViews.FirstOrDefault(s => s.id == id);
        }

        public string FixAnalogues()
        {
            int cntFixed = 0;
            List<int> arr = new List<int>();
            foreach (SpareAnaloguesView analogue in objDataContext.SpareAnaloguesViews)
            {
                if (!arr.Contains(analogue.id))
                {
                    int s1 = analogue.spareId1;
                    int s2 = analogue.spareId2;
                    if (objDataContext.SpareAnaloguesViews.Where(x => x.spareId1 == s2 && x.spareId2 == s1).Count() > 0)
                    {
                        objDataContext.spare_analogue.FirstOrDefault(x => x.id == analogue.id).is_equal = 1;
                        int toDelId = objDataContext.SpareAnaloguesViews.FirstOrDefault(x => x.spareId1 == s2 && x.spareId2 == s1).id;
                        arr.Add(toDelId);
                        objDataContext.DeleteObject(objDataContext.spare_analogue.FirstOrDefault(x => x.id == toDelId));
                        cntFixed++;
                    }
                }
            }
            objDataContext.SaveChanges();
            return "Фикс: " + cntFixed.ToString();
        }

        public List<SpareView> GetAnalogues(int spareId)
        {
            List<SpareView> items = new List<SpareView>();
            List<spare_analogue> analogues = objDataContext.spare_analogue.Where(a => a.spare.id == spareId).ToList();
            foreach (spare_analogue a in analogues)
            {
                a.spare1Reference.Load();
                SpareView an_sp = objDataContext.SpareViews.FirstOrDefault(i => i.id == a.spare1.id);
                an_sp.is_equal = AnaloguesAreEqual(spareId, an_sp.id);
                items.Add(an_sp);
            }
            return items;
        }

        private bool AnaloguesAreEqual(int spareId1, int spareId2)
        {
            bool fl1 = objDataContext.SpareAnaloguesViews.Where(x => x.spareId1 == spareId1 && x.spareId2 == spareId2).Count() > 0;
            bool fl2 = objDataContext.SpareAnaloguesViews.Where(x => x.spareId1 == spareId2 && x.spareId2 == spareId1).Count() > 0;
            return fl1 && fl2;
        }

        public List<SpareView> GetAnalogues2(int spareId)
        {
            List<SpareView> items = new List<SpareView>();

            // 1.выбираются все записи, где S1 - слева
            List<spare_analogue> analogues = objDataContext.spare_analogue.Where(a => a.spare.id == spareId).ToList();
            foreach (spare_analogue a in analogues)
            {
                a.spare1Reference.Load();
                SpareView an_sp = objDataContext.SpareViews.FirstOrDefault(i => i.id == a.spare1.id);
                an_sp.is_equal = a.is_equal == 0 ? false : true;
                items.Add(an_sp);
            }

            // 2. выбираются все записи, где S1 справа и стоит галочка “Взаимный”
            List<SpareView> eq_analogues = getEqualAnalogues(spareId);
            foreach (SpareView a in eq_analogues)
            {
                items.Add(a);
            }
            return items;
        }

        private List<SpareView> getEqualAnalogues(int spareId)
        {
            List<SpareView> items = new List<SpareView>();
            List<spare_analogue> analogues = objDataContext.spare_analogue.Where(a => a.spare1.id == spareId).Where(x => x.is_equal == 1).ToList();
            foreach (spare_analogue a in analogues)
            {
                a.spareReference.Load();
                SpareView an_sp = objDataContext.SpareViews.FirstOrDefault(i => i.id == a.spare.id);
                an_sp.is_equal = true;
                items.Add(an_sp);
            }
            return items;
        }

        /*Feb15
        public List<SpareView> GetSpares(string groupName)
        {
            if (groupName.Equals("Все товары"))
            {
                var spares = from s in objDataContext.SpareViews orderby s.code select s;
                return spares.ToList();
            }
            else
            {
                List<SpareView> items = getSparesByGroupName(groupName);
                List<string> childGroups = getChildGroups(groupName);
                foreach (string g in childGroups)
                {
                    List<SpareView> tmp = getSparesByGroupName(g);
                    foreach (SpareView t in tmp)
                    {
                        items.Add(t);
                    }
                }
                var sorted = from s in items orderby s.code select s;
                return sorted.ToList();
            }
        }*/
        /*Feb15
        public List<SpareView> GetSparesAvailable(string groupName)
        {
            if (groupName.Equals("Все товары"))
            {
                return GetSparesAvailable();
            }
            else
            {
                List<SpareView> items = getSparesByGroupName(groupName);
                List<string> childGroups = getChildGroups(groupName);
                foreach (string g in childGroups)
                {
                    List<SpareView> tmp = getSparesByGroupName(g);
                    foreach (SpareView t in tmp)
                    {
                        items.Add(t);
                    }
                }
                var sorted = from s in items orderby s.code where s.QRest>0 select s;
                return sorted.ToList();
            }
        }
        */
        /*Feb15
        public List<SpareView> GetSparesAvailable(int GroupID)
        {
            if (GroupID == 1)
            {
                return GetSparesAvailable();
            }
            else
            {
                List<SpareView> items = getSparesByGroupID(GroupID);
                List<string> childGroups = getChildGroups(GroupID);
                foreach (string g in childGroups)
                {
                    List<SpareView> tmp = getSparesByGroupName(g);
                    foreach (SpareView t in tmp)
                    {
                        items.Add(t);
                    }
                }
                var sorted = from s in items orderby s.code where s.QRest > 0 select s;
                return sorted.ToList();
            }
        }
        public List<SpareView> GetSpares(int GroupID)
        {
            if (GroupID == 39)
            {
                return objDataContext.SpareViews.ToList();
            }
            else
            {
                List<SpareView> items = getSparesByGroupID(GroupID);
                List<spare_group> childGroups = getChildGroupsByGroupID(GroupID);
                foreach (spare_group g in childGroups)
                {
                    List<SpareView> tmp = getSparesByGroupID(g.id);
                    foreach (SpareView t in tmp)
                    {
                        items.Add(t);
                    }
                }
                var sorted = from s in items orderby s.code select s;
                return sorted.ToList();
            }
        }
        public List<SpareView> GetSparesAvailable(int GroupID)
        {
            if (GroupID == 39)
            {
                return GetSparesAvailable();
            }
            else
            {
                List<SpareView> items = getSparesByGroupID(GroupID);
                List<spare_group> childGroups = getChildGroupsByGroupID(GroupID);
                foreach (spare_group g in childGroups)
                {
                    List<SpareView> tmp = getSparesByGroupID(g.id);
                    foreach (SpareView t in tmp)
                    {
                        items.Add(t);
                    }
                }
                var sorted = from s in items orderby s.code where s.QRest>0 select s;
                return sorted.ToList();
            }
        }
        public List<SpareView> GetSpares(int searchFieldIndex, string searchString)
        {
            List<SpareView> items = new List<SpareView>();
            switch (searchFieldIndex)
            {
                case 0:// ПОИСК ПО КОДУ
                    items = objDataContext.SpareViews.Where(s => s.code.ToLower().Contains(searchString.ToLower())).ToList();
                    break;

                case 1:// ПОИСК ПО НАИМЕНОВАНИЮ
                    items = objDataContext.SpareViews.Where(s => s.name.ToLower().Contains(searchString.ToLower())).ToList();
                    break;

                case 2:// ПОИСК ПО КОДУ ШАТЕ-М
                    items = objDataContext.SpareViews.Where(s => s.codeShatem.ToLower().Contains(searchString.ToLower())).ToList();
                    break;
            }
            return items.ToList();
        }
        public List<SpareView> GetSparesAvailable(int searchFieldIndex, string searchString)
        {
            List<SpareView> items = new List<SpareView>();
            switch (searchFieldIndex)
            {
                case 0:// ПОИСК ПО КОДУ
                    items = objDataContext.SpareViews.Where(s => s.QRest>0).Where(s => s.code.ToLower().Contains(searchString.ToLower())).ToList();
                    break;

                case 1:// ПОИСК ПО НАИМЕНОВАНИЮ
                    items = objDataContext.SpareViews.Where(s => s.QRest > 0).Where(s => s.name.ToLower().Contains(searchString.ToLower())).ToList();
                    break;

                case 2:// ПОИСК ПО КОДУ ШАТЕ-М
                    items = objDataContext.SpareViews.Where(s => s.QRest > 0).Where(s => s.codeShatem.ToLower().Contains(searchString.ToLower())).ToList();
                    break;
            }
            return items.ToList();
        }
        /*Feb15
        List<string> getChildGroups(string groupName)
        {
            List<string> items = new List<string>();
            List<spare_group> groups = objDataContext.spare_group.Where(g => g.ParentGroup.name == groupName).ToList();
            foreach (spare_group g in groups)
            {
                items.Add(g.name);
                List<string> tmp = getChildGroups(g.name);
                foreach (string s in tmp)
                {
                    items.Add(s);
                }
            }
            return items;
        }*/

        private List<string> getChildGroups(int GroupID)
        {
            List<string> items = new List<string>();
            spare_group group = objDataContext.spare_group.FirstOrDefault(sg => sg.id == GroupID);
            List<spare_group> groups = group.spare_group1.ToList();

            //Feb15 List<spare_group> groups = objDataContext.spare_group.Where(g => g.ParentGroup.id == groupName).ToList();
            foreach (spare_group g in groups)
            {
                items.Add(g.name);
                List<string> tmp = getChildGroups(g.id);
                foreach (string s in tmp)
                {
                    items.Add(s);
                }
            }
            return items;
        }

        private List<spare_group> getChildGroupsByGroupID(int parentID)
        {
            List<spare_group> items = new List<spare_group>();
            List<spare_group> groups = objDataContext.spare_group.Where(g => g.ParentGroup.id == parentID).ToList();
            foreach (spare_group g in groups)
            {
                items.Add(g);
                List<spare_group> tmp = getChildGroupsByGroupID(g.id);
                foreach (spare_group s in tmp)
                {
                    items.Add(s);
                }
            }
            return items;
        }

        /*
        List<string> getChildGroupsByGroupID(int parentID)
        {
            List<string> items = new List<string>();
            List<spare_group> groups = objDataContext.spare_group.Where(g => g.spare_group2.id == parentID).ToList();
            foreach (spare_group g in groups)
            {
                items.Add(g.name);
                List<string> tmp = getChildGroupsByGroupID(g.id);
                foreach (string s in tmp)
                {
                    items.Add(s);
                }
            }
            return items;
        }*/

        /*Feb15
        public List<SpareView> GetSpares(string groupName, string brandName)
        {
            List<SpareView> items = objDataContext.SpareViews.Where(s => s.GroupName == groupName).Where(g => g.BrandName == brandName).ToList();
            var sorted = from s in items orderby s.code select s;
            return sorted.ToList();
        }
        */
        /*Feb15
        public List<SpareView> GetSparesAvailable(string groupName, string brandName)
        {
            List<SpareView> items = objDataContext.SpareViews.Where(s => s.GroupName == groupName).Where(g => g.BrandName == brandName).ToList();
            var sorted = from s in items orderby s.code where s.QRest >0 select s;
            return sorted.ToList();
        }*/

        public List<SpareView> GetSparesAvailable(int GroupID, int BrandID)
        {
            List<SpareView> items = objDataContext.SpareViews.Where(s => s.GroupID == GroupID).Where(g => g.BrandID == BrandID).ToList();
            var sorted = from s in items orderby s.code where s.QRest > 0 select s;
            return sorted.ToList();
        }

        /*Feb15
        public List<SpareView> GetSparesByGroupIDBrandName(int GroupID, string BrandName)
        {
            return objDataContext.SpareViews.Where(s => s.GroupID == GroupID).Where(g => g.BrandName == BrandName).ToList();
        }*/

        public List<SpareView> GetSparesByGroupIDBrandName(int GroupID, int BrandID)
        {
            return objDataContext.SpareViews.Where(s => s.GroupID == GroupID).Where(g => g.BrandID == BrandID).ToList();
        }

        /*Feb15
        public List<SpareView> GetSparesByGroupIDBrandNameAvailable(int GroupID, string BrandName)
        {
            return objDataContext.SpareViews.Where(s=> s.QRest >0).Where(s => s.GroupID == GroupID).Where(g => g.BrandName == BrandName).ToList();
        }*/

        public List<SpareView> GetSparesByGroupIDBrandNameAvailable(int GroupID, int BrandID)
        {
            return objDataContext.SpareViews.Where(s => s.QRest > 0).Where(s => s.GroupID == GroupID).Where(g => g.BrandID == BrandID).ToList();
        }

        public void SpareCreate(spare _obj)
        {
            _obj.q_rest = 0;

            //_obj.created_on = DateTime.Now;
            objDataContext.AddTospares(_obj);
            objDataContext.SaveChanges();
        }

        public bool SpareExist(string SpareCode, string SpareName)
        {
            int count = objDataContext.spares.Where(i => (i.code.Equals(SpareCode) && i.name.Contains(SpareName))).Count();
            return (count != 0);
        }

        public void SpareCreate(string GrandParentGroupName, string ParentGroupName, string BrandName, string SpareCode, string SpareName, string SpareCars)
        {
            if (GrandParentGroupName.Equals(ParentGroupName))
                ParentGroupName += "_";

            // =============================================
            // grand pa group
            spare_group gsg = null;

            //get group
            gsg = objDataContext.spare_group.FirstOrDefault(i => i.name.Equals(GrandParentGroupName));
            if (gsg == null)
            {   // create new brand
                gsg = new spare_group();
                gsg.name = GrandParentGroupName;
                gsg.ParentGroup = root;
                objDataContext.AddTospare_group(gsg);
                objDataContext.SaveChanges();
            }

            // =============================================
            // pa group
            spare_group sg = null;

            //get group
            sg = objDataContext.spare_group.FirstOrDefault(i => i.name.Equals(ParentGroupName));
            if (sg == null)
            {   // create new brand
                sg = new spare_group();
                sg.name = ParentGroupName;
                sg.ParentGroup = gsg;
                objDataContext.AddTospare_group(sg);
                objDataContext.SaveChanges();
            }

            // =============================================
            // brand
            brand b = null;

            //get brand
            b = objDataContext.brands.FirstOrDefault(i => i.name.Equals(BrandName));
            if (b == null)
            {   // create new brand
                b = new brand();
                b.name = BrandName;
                objDataContext.AddTobrands(b);
                objDataContext.SaveChanges();
            }

            // spare
            spare s = new spare();
            s.spare_group2 = root;
            s.spare_group1 = gsg;
            s.spare_group = sg;
            s.brand = b;

            // =============================================
            // code
            s.code = SpareCode;

            // =============================================
            // name + models
            s.name = SpareName + "( " + SpareCars + ")";

            // =============================================
            // defs
            s.description = "";
            s.q_rest = 0;
            s.q_demand = 0;
            s.cashless = 0;
            s.unit = u;
            s.q_demand_clear = 0;
            s.codeShatem = "";

            // add & save
            objDataContext.AddTospares(s);
            objDataContext.SaveChanges();
        }

        public spare SpareCreate(spare _obj, string brandName, string groupName, string unitName)
        {
            _obj.q_rest = 0;
            if (_obj.code == null)
                _obj.code = "";
            _obj.brand = objDataContext.brands.FirstOrDefault(i => i.name == brandName);

            // group
            _obj.spare_group = objDataContext.spare_group.FirstOrDefault(i => (i.name == groupName && i.ChildCount == 0));
            int ParentID1 = GetParentGroupID(_obj.spare_group.id);
            if (ParentID1 > 0)
            {
                _obj.spare_group1 = objDataContext.spare_group.FirstOrDefault(i => i.id == ParentID1);
                int ParentID2 = GetParentGroupID(_obj.spare_group1.id);
                if (ParentID2 > 0)
                {
                    _obj.spare_group2 = objDataContext.spare_group.FirstOrDefault(i => i.id == ParentID2);
                    int ParentID3 = GetParentGroupID(_obj.spare_group2.id);
                    if (ParentID3 > 0)
                    {
                        _obj.spare_group3 = objDataContext.spare_group.FirstOrDefault(i => i.id == ParentID3);
                    }
                }
            }
            _obj.unit = objDataContext.units.FirstOrDefault(i => i.name == unitName);
            objDataContext.AddTospares(_obj);
            objDataContext.SaveChanges();
            return objDataContext.spares.FirstOrDefault(i => i.id == _obj.id);
        }

        public spare SpareCreate(spare _obj, int BrandID, int GroupID, int UnitID)
        {
            _obj.q_rest = 0;
            if (_obj.code == null)
                _obj.code = "";
            _obj.brand = objDataContext.brands.FirstOrDefault(i => i.id == BrandID);

            // group
            _obj.spare_group = objDataContext.spare_group.FirstOrDefault(i => (i.id == GroupID && i.ChildCount == 0));
            int ParentID1 = GetParentGroupID(_obj.spare_group.id);
            if (ParentID1 > 0)
            {
                _obj.spare_group1 = objDataContext.spare_group.FirstOrDefault(i => i.id == ParentID1);
                int ParentID2 = GetParentGroupID(_obj.spare_group1.id);
                if (ParentID2 > 0)
                {
                    _obj.spare_group2 = objDataContext.spare_group.FirstOrDefault(i => i.id == ParentID2);
                    int ParentID3 = GetParentGroupID(_obj.spare_group2.id);
                    if (ParentID3 > 0)
                    {
                        _obj.spare_group3 = objDataContext.spare_group.FirstOrDefault(i => i.id == ParentID3);
                    }
                }
            }
            _obj.unit = objDataContext.units.FirstOrDefault(i => i.id == UnitID);
            objDataContext.AddTospares(_obj);
            objDataContext.SaveChanges();
            return objDataContext.spares.FirstOrDefault(i => i.id == _obj.id);
        }

        public spare SpareCreate(spare _obj, int BrandID, int GroupID, string UnitName)
        {
            _obj.q_rest = 0;
            if (_obj.code == null)
                _obj.code = "";
            _obj.brand = objDataContext.brands.FirstOrDefault(i => i.id == BrandID);

            // group
            _obj.spare_group = objDataContext.spare_group.FirstOrDefault(i => (i.id == GroupID && i.ChildCount == 0));
            int ParentID1 = GetParentGroupID(_obj.spare_group.id);
            if (ParentID1 > 0)
            {
                _obj.spare_group1 = objDataContext.spare_group.FirstOrDefault(i => i.id == ParentID1);
                int ParentID2 = GetParentGroupID(_obj.spare_group1.id);
                if (ParentID2 > 0)
                {
                    _obj.spare_group2 = objDataContext.spare_group.FirstOrDefault(i => i.id == ParentID2);
                    int ParentID3 = GetParentGroupID(_obj.spare_group2.id);
                    if (ParentID3 > 0)
                    {
                        _obj.spare_group3 = objDataContext.spare_group.FirstOrDefault(i => i.id == ParentID3);
                    }
                }
            }
            _obj.unit = objDataContext.units.FirstOrDefault(i => i.name == UnitName);
            objDataContext.AddTospares(_obj);
            objDataContext.SaveChanges();
            return objDataContext.spares.FirstOrDefault(i => i.id == _obj.id);
        }

        public bool CodeExist(string code)
        {
            IList items = (from s in objDataContext.spares where s.code.Equals(code) select s).ToList();
            return (items.Count > 0 ? true : false);
        }

        public bool ExistCode1C(string Code1C)
        {
            return ((from s in objDataContext.spares where s.code1C.Equals(Code1C) select s).ToList().Count > 0 ? true : false);
        }

        public void SpareUpdateQRealToZero()
        {
            var spares = objDataContext.spares.Where(i => i.q_rest > 0);
            foreach (spare s in spares)
            {
                s.q_rest = 0;
            }
            objDataContext.SaveChanges();
            SpareContainer.Instance.Update();
        }

        public void SpareUpdateQReal(int SpareID, decimal Q)
        {
            spare s = objDataContext.spares.FirstOrDefault(i => i.id == SpareID);
            s.q_rest = Q;
            objDataContext.SaveChanges();
            SpareContainer.Instance.Update(SpareID);
        }

        public spare SpareEdit(spare item)
        {
            spare original = objDataContext.spares.FirstOrDefault(b => b.id == item.id);
            if (original != null)
            {
                original.name = item.name;

                //original.description = item.description;
                //if (original.created_on == null)
                //    original.created_on = DateTime.Now;
                objDataContext.SaveChanges();
            }
            return original;
        }

        public void SpareEdit(int SpareID, string xml)
        {
            spare s = objDataContext.spares.FirstOrDefault(i => i.id == SpareID);
            s.QRest = xml;
            objDataContext.SaveChanges();
        }

        public bool SpareEdit(string CodeShateM, string Code)
        {
            // get spare
            spare original = objDataContext.spares.FirstOrDefault(b => b.code == CodeShateM);
            if (original != null)
            {
                original.code = Code;
                original.codeShatem = CodeShateM;
                objDataContext.SaveChanges();
                return true;
            }
            else
                return false;
        }

        public spare SpareEdit(spare item, string brandName, string groupName, string unitName)
        {
            spare original = objDataContext.spares.FirstOrDefault(b => b.id == item.id);
            if (original != null)
            {
                original.name = item.name;
                original.brand = objDataContext.brands.FirstOrDefault(i => i.name == brandName);
                if (original.spare_group == null)
                    original.spare_groupReference.Load();
                int OldGroupID = original.spare_group.id;
                original.spare_group = GetSpareGroupChildFree(groupName);
                if (original.spare_group.id != OldGroupID)
                {
                    int ParentID1 = GetParentGroupID(original.spare_group.id);
                    if (ParentID1 > 0)
                    {
                        original.spare_group1 = objDataContext.spare_group.FirstOrDefault(i => i.id == ParentID1);
                        int ParentID2 = GetParentGroupID(original.spare_group1.id);
                        if (ParentID2 > 0)
                        {
                            original.spare_group2 = objDataContext.spare_group.FirstOrDefault(i => i.id == ParentID2);
                            int ParentID3 = GetParentGroupID(original.spare_group2.id);
                            if (ParentID3 > 0)
                            {
                                original.spare_group3 = objDataContext.spare_group.FirstOrDefault(i => i.id == ParentID3);
                            }
                        }
                    }
                }
                original.unit = objDataContext.units.FirstOrDefault(i => i.name == unitName);
                original.description = item.description;
                original.cashless = item.cashless;
                original.code = item.code;
                original.codeShatem = item.codeShatem;
                original.q_demand = item.q_demand;
                original.q_rest = item.q_rest;
                objDataContext.SaveChanges();
            }
            return original;
        }

        public spare SpareEdit(spare item, int BrandID, int GroupID, int UnitID)
        {
            spare original = objDataContext.spares.FirstOrDefault(b => b.id == item.id);
            if (original != null)
            {
                original.name = item.name;
                original.brand = objDataContext.brands.FirstOrDefault(i => i.id == BrandID);
                if (original.spare_group == null)
                    original.spare_groupReference.Load();
                int OldGroupID = original.spare_group.id;
                original.spare_group = objDataContext.spare_group.FirstOrDefault(i => i.id == GroupID);
                if (original.spare_group.id != OldGroupID)
                {
                    int ParentID1 = GetParentGroupID(original.spare_group.id);
                    if (ParentID1 > 0)
                    {
                        original.spare_group1 = objDataContext.spare_group.FirstOrDefault(i => i.id == ParentID1);
                        int ParentID2 = GetParentGroupID(original.spare_group1.id);
                        if (ParentID2 > 0)
                        {
                            original.spare_group2 = objDataContext.spare_group.FirstOrDefault(i => i.id == ParentID2);
                            int ParentID3 = GetParentGroupID(original.spare_group2.id);
                            if (ParentID3 > 0)
                            {
                                original.spare_group3 = objDataContext.spare_group.FirstOrDefault(i => i.id == ParentID3);
                            }
                        }
                    }
                }
                original.unit = objDataContext.units.FirstOrDefault(i => i.id == UnitID);
                original.description = item.description;
                original.cashless = item.cashless;
                original.code = item.code;
                original.codeShatem = item.codeShatem;
                original.q_demand = item.q_demand;
                original.q_rest = item.q_rest;
                objDataContext.SaveChanges();
            }
            return original;
        }

        public spare SpareCreate(spare item, string BrandCode1C, string GroupCode1C)
        {
            item.q_rest = 0;
            string UnitName = "шт.";
            item.brand = objDataContext.brands.FirstOrDefault(i => i.code1C == BrandCode1C);
            item.spare_group = objDataContext.spare_group.FirstOrDefault(i => i.code1C == GroupCode1C);
            int spare_group1ID = GetParentGroupID(item.spare_group.id);

            if (spare_group1ID > 0)
            {
                item.spare_group1 = objDataContext.spare_group.FirstOrDefault(i => i.id == spare_group1ID);
                int spare_group2ID = GetParentGroupID(spare_group1ID);
                if (spare_group2ID > 0)
                {
                    item.spare_group2 = objDataContext.spare_group.FirstOrDefault(i => i.id == spare_group2ID);
                    int spare_group3ID = GetParentGroupID(spare_group2ID);
                    if (spare_group3ID > 0)
                        item.spare_group3 = objDataContext.spare_group.FirstOrDefault(i => i.id == spare_group3ID);
                }
            }

            item.unit = objDataContext.units.FirstOrDefault(i => i.name == UnitName);
            objDataContext.AddTospares(item);
            objDataContext.SaveChanges();
            return item;
        }

        private int GetParentGroupID(int ID)
        {
            spare_group g = objDataContext.spare_group.FirstOrDefault(i => i.id == ID);
            if (g != null)
            {
                g.ParentGroupReference.Load();
                if (g.ParentGroup != null)
                {
                    return g.ParentGroup.id;
                }
            }
            return 0;
        }

        private int GetGroupIDByCode1C(string Code1C)
        {
            spare_group g = objDataContext.spare_group.FirstOrDefault(i => i.code1C == Code1C);
            if (g != null)
                return g.id;
            else
                return 0;
        }

        public int SpareDelete(int id)
        {
            // delete spare in spare income
            InOfferingDeleteBySpareID(id);

            // delete spare in invoice
            InvoiceOfferingsDeleteBySpareID(id);

            // delete spare in overpricing
            OverpricingOfferingsDeleteBySpareID(id);

            // delete spare analogues
            DeleteSpareAnalogues(id);

            spare b = objDataContext.spares.FirstOrDefault(o => o.id == id);
            objDataContext.DeleteObject(b);
            objDataContext.SaveChanges();

            return id;
        }

        private int DeleteSpareAnalogues(int spareId)
        {
            int deleted = 0;
            List<spare_analogue> items = objDataContext.spare_analogue.Where(i => i.spare.id == spareId).ToList();
            foreach (spare_analogue a in items)
            {
                deleted++;
                objDataContext.DeleteObject(a);
            }
            items = objDataContext.spare_analogue.Where(i => i.spare1.id == spareId).ToList();
            foreach (spare_analogue a in items)
            {
                deleted++;
                objDataContext.DeleteObject(a);
            }
            if (deleted > 0)
                objDataContext.SaveChanges();
            return deleted;
        }

        public void SpareDelete(string name)
        {
            spare b = objDataContext.spares.FirstOrDefault(o => o.name == name);
            objDataContext.DeleteObject(b);
            objDataContext.SaveChanges();
        }

        #endregion SPARES

        #region SPARES ANALOGUES

        public List<spare_analogue> GetSpareAnalogues()
        {
            return objDataContext.spare_analogue.ToList<spare_analogue>();
        }

        public spare_analogue getSpareAnalogue(string spareName1, string spareName2)
        {
            spare_analogue s1 = objDataContext.spare_analogue.Where(s => s.spare.name == spareName1).FirstOrDefault(a => a.spare1.name == spareName2);
            spare_analogue s2 = objDataContext.spare_analogue.Where(s => s.spare.name == spareName2).Where(x => x.is_equal > 0).FirstOrDefault(a => a.spare1.name == spareName1);
            return s1 != null ? s1 : s2;
        }

        public void SpareAnalogueCreate(spare_analogue _obj)
        {
            //_obj.created_on = DateTime.Now;
            objDataContext.AddTospare_analogue(_obj);
            objDataContext.SaveChanges();
        }

        public void SpareAnalogueCreate(spare_analogue _obj, string spareName1, string spareName2)
        {
            _obj.spare = objDataContext.spares.FirstOrDefault(s => s.name == spareName1);
            _obj.spare1 = objDataContext.spares.FirstOrDefault(s => s.name == spareName2);
            objDataContext.AddTospare_analogue(_obj);
            objDataContext.SaveChanges();
        }

        public void SpareAnalogueCreate(spare_analogue _obj, int spareId1, int spareId2)
        {
            _obj.spare = objDataContext.spares.FirstOrDefault(s => s.id == spareId1);
            _obj.spare1 = objDataContext.spares.FirstOrDefault(s => s.id == spareId2);
            bool fl = _obj.is_equal.Value == 1 ? true : false;
            _obj.is_equal = 0;
            objDataContext.AddTospare_analogue(_obj);
            objDataContext.SaveChanges();
            if (fl)
            {
                spare_analogue sa1 = new spare_analogue();
                sa1.decsription = "";

                //sa1.is_equal = 1;
                sa1.spare = _obj.spare1;
                sa1.spare1 = _obj.spare;
                objDataContext.AddTospare_analogue(sa1);
                objDataContext.SaveChanges();
            }
        }

        public void SpareAnalogueCreate(int is_equal, string Spare1Code1C, string Spare2Code1C)
        {
            spare s1 = null;
            spare s2 = null;
            s1 = objDataContext.spares.FirstOrDefault(s => s.code1C == Spare1Code1C);
            s2 = objDataContext.spares.FirstOrDefault(s => s.code1C == Spare2Code1C);
            if (s1 != null && s2 != null)
            {
                spare_analogue _obj = new spare_analogue();
                _obj.is_equal = is_equal;
                _obj.spare = s1;
                _obj.spare1 = s2;
                objDataContext.AddTospare_analogue(_obj);
                objDataContext.SaveChanges();
            }
        }

        public void SpareAnalogueEdit(spare_analogue item, string spareName2)
        {
            // 30/01/2012
            // получим изначальную версию аналога
            spare_analogue original = objDataContext.spare_analogue.FirstOrDefault(b => b.id == item.id);

            // подгрузим запчасти в аналог
            if (original.spare == null)
                original.spareReference.Load();
            if (original.spare1 == null)
                original.spare1Reference.Load();
            spare Sp = original.spare;
            spare SpOld = original.spare1;
            spare SpNew = objDataContext.spares.FirstOrDefault(p => p.name == spareName2);
            if (SpNew == null)
                throw new Exception("Ошибка в базе: новая деталь-аналог не найдена.");

            // проверить, являлся ли до этого аналог взаимным
            int OrigIsEq = AnaloguesAreEqual(Sp.id, SpOld.id) ? 1 : 0;
            int ItemIsEq = item.is_equal.Value;

            // если запчасть та же самая ---------------
            if (original.spare1.name.Equals(spareName2))
            {
                // ---- если галочка стояла, но убрали
                if (OrigIsEq > ItemIsEq)
                {
                    // удалить "обратную" запись из таблицы аналогов
                    spare_analogue sad = objDataContext.spare_analogue.FirstOrDefault(b =>
                                            (b.spare.id == SpOld.id && b.spare1.id == Sp.id));
                    if (sad != null)
                    {
                        objDataContext.DeleteObject(sad);
                        objDataContext.SaveChanges();
                    }
                }
                else

                    // ---- если галочки не было, но поставили
                    if (OrigIsEq < ItemIsEq)
                    {
                        // добавить обратную запись в таблицу аналогов
                        spare_analogue sa1 = new spare_analogue();
                        sa1.decsription = "";
                        sa1.is_equal = 0;
                        sa1.spare = SpOld;
                        sa1.spare1 = Sp;
                        objDataContext.AddTospare_analogue(sa1);
                        objDataContext.SaveChanges();
                    }
            }
            else

            // если запчасть другая      ---------------
            {
                // ---- удалить старую основную связь
                spare_analogue sad1 = objDataContext.spare_analogue.FirstOrDefault(b =>
                                        (b.spare.id == Sp.id && b.spare1.id == SpOld.id));
                if (sad1 != null)
                {
                    objDataContext.DeleteObject(sad1);
                    objDataContext.SaveChanges();
                }

                // ---- если галочка стояла
                if (OrigIsEq > 0)
                {
                    // удалить "обратную" запись из таблицы аналогов для старого аналога
                    spare_analogue sad = objDataContext.spare_analogue.FirstOrDefault(b =>
                                            (b.spare.id == SpOld.id && b.spare1.id == Sp.id));
                    if (sad != null)
                    {
                        objDataContext.DeleteObject(sad);
                        objDataContext.SaveChanges();
                    }
                }

                // ---- если галочку поставили
                if (ItemIsEq > 0)
                {
                    // добавить обратную запись в таблицу аналогов для новой запчасти
                    spare_analogue sa1 = new spare_analogue();
                    sa1.decsription = "";
                    sa1.is_equal = 0;
                    sa1.spare = Sp;
                    sa1.spare1 = SpNew;
                    objDataContext.AddTospare_analogue(sa1);
                    objDataContext.SaveChanges();
                }
            }
        }

        public void SpareAnalogueDelete(int SpareID, int SpareID1)
        {
            List<spare_analogue> analoguesToDelete = objDataContext.spare_analogue.Where(a => a.spare.id == SpareID && a.spare1.id == SpareID1).ToList();
            while (analoguesToDelete.Count() > 0)
            {
                objDataContext.DeleteObject(analoguesToDelete[0]);
                analoguesToDelete.Remove(analoguesToDelete[0]);
            }
            List<spare_analogue> analoguesToDeleteBack = objDataContext.spare_analogue.Where(a => a.spare.id == SpareID1 && a.spare1.id == SpareID).ToList();
            while (analoguesToDeleteBack.Count() > 0)
            {
                objDataContext.DeleteObject(analoguesToDeleteBack[0]);
                analoguesToDeleteBack.Remove(analoguesToDeleteBack[0]);
            }
            objDataContext.SaveChanges();
        }

        #endregion SPARES ANALOGUES

        #region SPARE GROUPS

        public List<spare_group> GetSpareGroups()
        {
            return objDataContext.spare_group.ToList();
        }

        public List<spare_group> GetOnlySpareGroups()
        {
            return objDataContext.spare_group.Where(i => (!i.IsBrand && i.ChildCount == 0)).ToList();
        }

        public List<spare_group> GetSpareGroups(int SearchFieldIndex, string SearchText)
        {
            if (SearchText.Length < 1)
                return GetOnlySpareGroups();
            switch (SearchFieldIndex)
            {
                case 0:
                    return GetOnlySpareGroups().Where(x => x.name.ToUpper().Contains(SearchText.ToUpper())).ToList();
                case 1:
                    return GetOnlySpareGroups().Where(x => x.description.ToUpper().Contains(SearchText.ToUpper())).ToList();
                default:
                    return GetOnlySpareGroups();
            }
        }

        public List<spare_group> GetSpareGroupEnds()
        {
            /*
            var AllGroups = objDataContext.spare_group;
            List<spare_group> Selected = new List<spare_group>();
            foreach (spare_group g in AllGroups)
            {
                    if (getSpareGroupChildsCount(g.id) == 0)
                    Selected.Add(g);
            }
            return Selected;*/
            //return objDataContext.SpareGroupEndsViews.ToList();
            return GetOnlySpareGroups();
        }

        public spare_group GetSpareGroup(string name)
        {
            return objDataContext.spare_group.FirstOrDefault(i => i.name == name);
        }

        public spare_group GetSpareGroup(int ID)
        {
            return objDataContext.spare_group.FirstOrDefault(i => i.id == ID);
        }

        public spare_group GetSpareGroupChildFree(string name)
        {
            return objDataContext.spare_group.FirstOrDefault(i => (i.name == name && i.ChildCount == 0));
        }

        public spare_group GetRootGroup()
        {
            return objDataContext.spare_group.FirstOrDefault(i => i.id == 1);
        }

        public ICollection<object> GetRoots()
        {
            return objDataContext.spare_group.Where(x => x.ParentGroup == null).ToList<object>();
        }

        public spare_group SpareGroupCreate(spare_group i)
        {
            if (SpareGroupNameExist(i.name))
                i.name += " ";
            objDataContext.AddTospare_group(i);
            objDataContext.SaveChanges();
            return i;
        }

        public int SpareGroupCreate(int GroupID, string BrandName)
        {
            spare_group i = new spare_group();
            i.IsBrand = true;
            i.name = BrandName;
            spare_group sg = objDataContext.spare_group.FirstOrDefault(s => s.id == GroupID);
            i.ParentGroupName = sg.name;
            i.ParentGroup = sg;
            i.description = BrandName;
            i.ChildCount = 0;
            i.code1C = "";

            objDataContext.AddTospare_group(i);
            objDataContext.SaveChanges();
            return 0;
        }

        public int SpareGroupCreate(int GroupID, int BrandID)
        {
            brand b = objDataContext.brands.FirstOrDefault(br => br.id == BrandID);
            if (b == null)
                return 0;
            spare_group i = new spare_group();
            i.IsBrand = true;
            i.name = b.name;
            spare_group sg = objDataContext.spare_group.FirstOrDefault(s => s.id == GroupID);
            i.ParentGroupName = sg.name;
            i.ParentGroup = sg;
            i.description = b.name;
            i.ChildCount = 0;
            i.code1C = "";

            objDataContext.AddTospare_group(i);
            objDataContext.SaveChanges();
            return 0;
        }

        /*
        public int SpareGroupCreate(string GroupName, string BrandName)
        {
            spare_group i = new spare_group();
            i.IsBrand = true;
            i.name = BrandName;
            i.ParentGroupName = GroupName;

            //i.ParentGroup = GetSpareGroup(GroupName);
            i.ParentGroup = GetSpareGroupChildFree(GroupName);

            //GetSpareGroup(GroupName).ChildCount++;
            i.description = BrandName;
            i.ChildCount = 0;
            i.code1C = "";

            objDataContext.AddTospare_group(i);
            objDataContext.SaveChanges();
            return 0;
        }*/

        public spare_group GetSpareGroup1stRow(string name)
        {
            spare_group g = null;
            g = objDataContext.spare_group.FirstOrDefault(i => i.id == 1).spare_group1.FirstOrDefault(x => x.name.ToUpper() == name.ToUpper());
            return g;
        }

        public spare_group GetSpareChildGroup(int ParentGroupID, string name)
        {
            spare_group g = null;
            g = objDataContext.spare_group.FirstOrDefault(i => i.id == ParentGroupID).spare_group1.FirstOrDefault(x => x.name.ToUpper() == name.ToUpper());
            return g;
        }

        private bool SpareGroupNameExist(string groupName)
        {
            if (objDataContext.spare_group.Where(g => g.name == groupName).ToList().Count != 0)
                return true;
            return false;
        }

        public int SpareGroupEdit(spare_group i)
        {
            spare_group original = objDataContext.spare_group.FirstOrDefault(b => b.id == i.id);
            if (original != null)
            {
                if (SpareGroupNameExist(i.name))
                    return -1;
                original.name = i.name;
                original.description = i.description;
                objDataContext.SaveChanges();
                return 0;
            }
            return -1;
        }

        public void SpareGroupUpdateParent(string Code1C, string ParentCode1C)
        {
            spare_group group = objDataContext.spare_group.FirstOrDefault(g => g.code1C == Code1C);
            spare_group parent = objDataContext.spare_group.FirstOrDefault(g => g.code1C == ParentCode1C);
            group.ParentGroup = parent;
            objDataContext.SaveChanges();
        }

        public void SpareGroupDelete(spare_group i)
        {
            var childs = objDataContext.spare_group.Where(g => g.ParentGroup.id == i.id);
            while (childs.Count() > 0)
            {
                spare_group g = childs.ToList()[0];
                moveSparesInGroupToRoot(g.id);
                SpareGroupDelete(g);
            }
            moveSparesInGroupToRoot(i.id);
            objDataContext.DeleteObject(i);
            objDataContext.SaveChanges();
        }

        /*Feb15
        public void SpareGroupDelete(string BrandName, int GroupID)
        {
            spare_group sgg = objDataContext.spare_group.FirstOrDefault(i => i.id == GroupID);
            if (sgg == null)
                return;
            spare_group BrandGroup = objDataContext.spare_group.FirstOrDefault(g => (g.IsBrand && g.name.Equals(BrandName) && g.ParentGroup.id == GroupID));
            if (BrandGroup == null)
                return;
            objDataContext.DeleteObject(BrandGroup);
            objDataContext.SaveChanges();
        }*/

        public void SpareGroupDelete(int BrandID, int GroupID)
        {
            brand b = objDataContext.brands.FirstOrDefault(br => br.id == BrandID);
            if (b == null)
                return;
            string BrandName = b.name;
            spare_group sgg = objDataContext.spare_group.FirstOrDefault(i => i.id == GroupID);
            if (sgg == null)
                return;
            spare_group BrandGroup = objDataContext.spare_group.FirstOrDefault(g => (g.IsBrand && g.name.Equals(BrandName) && g.ParentGroup.id == GroupID));
            if (BrandGroup == null)
                return;
            objDataContext.DeleteObject(BrandGroup);
            objDataContext.SaveChanges();
        }

        /*
        public void SpareGroupDelete(string BrandName, string GroupName)
        {
            spare_group sgg = GetSpareGroup(GroupName);
            if (sgg == null)
                return;
            if (sgg.ChildCount > 0)
                sgg.ChildCount--;
            int GroupID = GetSpareGroup(GroupName).id;
            spare_group sg = objDataContext.spare_group.FirstOrDefault(g => (g.IsBrand && g.name.Equals(BrandName) && g.ParentGroup.id == GroupID));
            objDataContext.DeleteObject(sg);
            objDataContext.SaveChanges();
        }*/

        private void moveSparesInGroupToRoot(int groupId)
        {
            int RootID = 1;
            spare_group root = objDataContext.spare_group.FirstOrDefault(g => g.id == RootID);
            var items = objDataContext.spares.Where(i => i.spare_group.id == groupId);
            foreach (spare item in items)
            {
                item.spare_group = root;
            }
            objDataContext.SaveChanges();
        }

        public bool isSpareGroup(string name)
        {
            return objDataContext.spare_group.Where(s => s.name == name).Count() > 0 ? true : false;
        }

        /*Feb15
        public List<string> getBrandsInSpareGroup(string groupName)
        {
            List<string> items = new List<string>();
            List<SpareView> spares = getSparesByGroupName(groupName);
            foreach (SpareView i in spares)
            {
                if (!items.Contains(i.BrandName))
                {
                    items.Add(i.BrandName);
                }
            }
            items.Sort();
            return items;
        }*/

        //List<spare> getSparesByGroupId(int groupId)
        //{
        //    List<spare> items = objDataContext.spares.Where(i => i.spare_group.id == groupId).ToList();
        //    return items;
        // }
        /*Feb15
        List<SpareView> getSparesByGroupName(string groupName)
        {
            return objDataContext.SpareViews.Where(i => i.GroupName == groupName).ToList();
        }*/

        private List<SpareView> getSparesByGroupID(int GroupID)
        {
            return objDataContext.SpareViews.Where(i => i.GroupID == GroupID).ToList();
        }

        public int getSpareGroupChildsCount(int groupId)
        {
            int count = 0;
            count = objDataContext.spare_group.Where(g => g.ParentGroup.id == groupId).ToList().Count;
            return count;
        }

        #endregion SPARE GROUPS

        #region UNITS

        public List<unit> GetUnits()
        {
            return objDataContext.units.ToList<unit>();
        }

        public List<unit> GetUnits(int SearchFieldIndex, string SearchText)
        {
            if (SearchText.Length < 1)
                return objDataContext.units.ToList<unit>();
            switch (SearchFieldIndex)
            {
                case 0:
                    return objDataContext.units.Where(x => x.name.Contains(SearchText)).ToList<unit>();
                case 1:
                    return objDataContext.units.Where(x => x.description.Contains(SearchText)).ToList<unit>();
                default:
                    return objDataContext.units.ToList<unit>();
            }
        }

        public unit GetUnit(int id)
        {
            return objDataContext.units.FirstOrDefault(i => i.id == id);
        }

        public void UnitCreate(unit _obj)
        {
            //_obj.created_on = DateTime.Now;
            objDataContext.AddTounits(_obj);
            objDataContext.SaveChanges();
        }

        public void UnitEdit(unit item)
        {
            unit original = objDataContext.units.FirstOrDefault(b => b.id == item.id);
            if (original != null)
            {
                original.name = item.name;
                original.description = item.description;

                //if (original.created_on == null)
                //    original.created_on = DateTime.Now;
                objDataContext.SaveChanges();
            }
        }

        public void UnitDelete(int id)
        {
            unit b = objDataContext.units.FirstOrDefault(o => o.id == id);
            objDataContext.DeleteObject(b);
            objDataContext.SaveChanges();
        }

        public void UnitDelete(string name)
        {
            unit b = objDataContext.units.FirstOrDefault(o => o.name == name);
            objDataContext.DeleteObject(b);
            objDataContext.SaveChanges();
        }

        #endregion UNITS

        #region VAT_RATE

        public List<vat_rate> GetVATRates()
        {
            return (from s in objDataContext.vat_rate orderby s.name select s).ToList();
        }

        public decimal getVatRate(int ID)
        {
            return objDataContext.vat_rate.FirstOrDefault(i => i.id == ID).rate;
        }

        public vat_rate getZeroVatRate()
        {
            return objDataContext.vat_rate.FirstOrDefault(i => i.rate == 0);
        }

        public void VATRateCreate(vat_rate _obj)
        {
            //_obj.created_on = DateTime.Now;
            objDataContext.AddTovat_rate(_obj);
            objDataContext.SaveChanges();
        }

        public void VATRateEdit(vat_rate item)
        {
            vat_rate original = objDataContext.vat_rate.FirstOrDefault(b => b.id == item.id);
            if (original != null)
            {
                original.name = item.name;
                original.rate = item.rate;

                //if (original.created_on == null)
                //    original.created_on = DateTime.Now;
                objDataContext.SaveChanges();
            }
        }

        public void VATRateDelete(int id)
        {
            vat_rate b = objDataContext.vat_rate.FirstOrDefault(o => o.id == id);
            objDataContext.DeleteObject(b);
            objDataContext.SaveChanges();
        }

        #endregion VAT_RATE

        #region Warehouses

        public List<warehouse> GetWarehouses()
        {
            return objDataContext.warehouses.ToList<warehouse>();
        }

        public warehouse GetWarehouse(int id)
        {
            return objDataContext.warehouses.FirstOrDefault(i => i.id == id);
        }

        public List<warehouse> GetWarehouses(int searchFieldIndex, string searchString)
        {
            List<warehouse> items = new List<warehouse>();
            switch (searchFieldIndex)
            {
                case 0:// ПОИСК ПО НАИМЕНОВАНИЮ
                    items = objDataContext.warehouses.Where(s => s.name.ToLower().Contains(searchString.ToLower())).ToList();
                    break;

                case 1:// ПОИСК ПО ОПИСАНИЮ(ПРИМЕЧАНИЮ)
                    items = objDataContext.warehouses.Where(s => s.description.ToLower().Contains(searchString.ToLower())).ToList();
                    break;
            }
            var sorted = from s in items orderby s.name select s;
            items = sorted.ToList();
            return items;
        }

        public void WarehouseCreate(warehouse _obj)
        {
            //_obj.created_on = DateTime.Now;
            objDataContext.AddTowarehouses(_obj);
            objDataContext.SaveChanges();
        }

        public void WarehouseEdit(warehouse item)
        {
            warehouse original = objDataContext.warehouses.FirstOrDefault(b => b.id == item.id);
            if (original != null)
            {
                original.name = item.name;
                original.description = item.description;

                //if (original.created_on == null)
                //    original.created_on = DateTime.Now;
                objDataContext.SaveChanges();
            }
        }

        public void WarehouseDelete(int id)
        {
            warehouse b = objDataContext.warehouses.FirstOrDefault(o => o.id == id);
            objDataContext.DeleteObject(b);
            objDataContext.SaveChanges();
        }

        public void WarehouseDelete(string name)
        {
            warehouse b = objDataContext.warehouses.FirstOrDefault(o => o.name == name);
            objDataContext.DeleteObject(b);
            objDataContext.SaveChanges();
        }

        #endregion Warehouses

        #region SPARE INCOME

        public int getRemainsInputID()
        {
            spare_income s = objDataContext.spare_income.FirstOrDefault(i => i.is_remains_input == 1);
            if (s != null)
                return s.id;
            else
            {
                s = new spare_income();
                s.si_date = DateTime.Now;
                s.is_remains_input = 1;
                objDataContext.AddTospare_income(s);
                objDataContext.SaveChanges();
                return s.id;
            }
        }

        public List<SpareIncomeView> SpareIncomeGetAll()
        {
            return (from s in objDataContext.SpareIncomeViews select s).Where(i => i.IsRemains != 1).ToList();
        }

        public spare_income SpareIncomeGet(int Id)
        {
            return objDataContext.spare_income.FirstOrDefault(i => i.id == Id);
        }

        public int SpareIncomeGetMaxId()
        {
            int res = 0;
            if (objDataContext.spare_income.Count() == 0)
                return 1;
            else
            {
                try
                {
                    res = objDataContext.spare_income.Max(s => s.IDN.Value) + 1;
                }
                catch (Exception)
                {
                    res = 1;
                }
            }
            return res;
        }

        public int SpareIncomeCreate(spare_income _obj, string AccountName, string WarehouseName, string CurrencyCode)
        {
            objDataContext = new DriveEntities();
            _obj.is_remains_input = 0;
            if (AccountName.Length > 0)
                _obj.account = objDataContext.accounts.FirstOrDefault(i => i.name == AccountName);
            _obj.warehouse = objDataContext.warehouses.FirstOrDefault(i => i.name == WarehouseName);
            _obj.currency = objDataContext.currencies.FirstOrDefault(i => i.code == CurrencyCode);
            objDataContext.AddTospare_income(_obj);
            objDataContext.SaveChanges();
            return _obj.id;
        }

        public void SpareIncomeEdit(spare_income item, string AccountName, string WarehouseName, string CurrencyCode)
        {
            objDataContext = new DriveEntities();
            spare_income original = objDataContext.spare_income.FirstOrDefault(b => b.id == item.id);
            if (original != null)
            {
                if (AccountName.Length > 0)
                    original.account = objDataContext.accounts.FirstOrDefault(i => i.name == AccountName);
                original.IDN = item.IDN;
                original.num = item.num;
                original.base_doc = item.base_doc;
                original.base_doc_date = item.base_doc_date;
                original.currency = objDataContext.currencies.FirstOrDefault(i => i.code == CurrencyCode);
                original.si_date = item.si_date;
                original.warehouse = objDataContext.warehouses.FirstOrDefault(i => i.name == WarehouseName);
                original.cashless = item.cashless;
                objDataContext.SaveChanges();
            }
        }

        public void SpareIncomeDelete(int SpareIncomeID)
        {
            objDataContext = new DriveEntities();
            spare_income item = objDataContext.spare_income.FirstOrDefault(i => i.id == SpareIncomeID);
            if (item == null)
                return;
            // удалить все прикрепленные товары Spare In SpareIncome
            //InOfferingDeleteBySpareIncomeId(spareIncomeId);
            spare_in_spare_income sisi = objDataContext.spare_in_spare_income.FirstOrDefault(i => i.spare_income.id == SpareIncomeID);
            while (sisi != null)
            {
                if (sisi.spare == null)
                    sisi.spareReference.Load();
                int SpareID = sisi.spare.id;
                int SpareInSpareIncomeID = sisi.id;

                // удалить завязанные на данный приход Spare In SpareOutgo
                //FixOfferingsOutsOnDeletingIncome(item.id);
                List<spare_in_spare_outgo> sisos = objDataContext.spare_in_spare_outgo.Where(x => x.spare_in_spare_income.id == SpareInSpareIncomeID).ToList();
                foreach (spare_in_spare_outgo siso in sisos)
                {
                    siso.spare_in_spare_income = null;
                }
                objDataContext.SaveChanges();

                objDataContext.DeleteObject(sisi);
                objDataContext.SaveChanges();

                SpareContainer.Instance.Update(SpareID);

                sisi = objDataContext.spare_in_spare_income.FirstOrDefault(i => i.spare_income.id == SpareIncomeID);
            }

            // удалить SpareIncome
            objDataContext = new DriveEntities();
            item = objDataContext.spare_income.FirstOrDefault(i => i.id == SpareIncomeID);
            if (item == null)
                return;
            objDataContext.DeleteObject(item);
            objDataContext.SaveChanges();
        }

        public List<SpareInSpareIncomeView> GetIncomes(int spareId)
        {
            return objDataContext.SpareInSpareIncomeViews.Where(a => a.SpareID == spareId && a.QRest > 0).ToList();
        }

        public SpareInSpareIncomeView GetLastIncome(int SpareID)
        {
            if (objDataContext.SpareInSpareIncomeViews.Count(x => x.QRest > 0 && x.SpareID == SpareID) == 0)
                return null;
            return objDataContext.SpareInSpareIncomeViews.Where(x => x.SpareID == SpareID && x.QRest > 0).OrderByDescending(i => i.SpareIncomeDate).First();
        }

        public List<SpareInSpareIncomeView> GetIncomesByIncomeID(int SpareIncomeId)
        {
            var items = objDataContext.SpareInSpareIncomeViews.Where(a => a.SpareIncomeID == SpareIncomeId);
            return items.ToList();
        }

        public List<SpareInSpareIncomeView> GetIncomes()
        {
            return objDataContext.SpareInSpareIncomeViews.ToList();
        }

        public List<SpareInSpareIncomeView> GetActualIncomes()
        {
            return objDataContext.SpareInSpareIncomeViews.Where(i => i.QRest > 0).ToList();
        }

        public List<SpareInSpareIncomeView> GetIncomes(DateTime dateFrom, DateTime dateTo)
        {
            var list = from s in objDataContext.SpareInSpareIncomeViews where (s.SpareIncomeDate >= dateFrom && s.SpareIncomeDate <= dateTo) select s;
            return list.ToList();
        }

        public List<ReportIncome> GetReportIncomes(DateTime dateFrom, DateTime dateTo)
        {
            var list = from s in objDataContext.ReportIncomes where (s.IncomeDate >= dateFrom && s.IncomeDate <= dateTo) select s;
            return list.ToList();
        }

        public List<ReportIncome> GetReportIncomes(int IncomeID)
        {
            var list = from s in objDataContext.ReportIncomes where s.SpareIncomeID == IncomeID select s;
            return list.ToList();
        }

        public List<ReportOutgo> GetReportOutgoes(DateTime dateFrom, DateTime dateTo)
        {
            var list = from s in objDataContext.ReportOutgoes where (s.OutgoDate >= dateFrom && s.OutgoDate <= dateTo) select s;
            return list.ToList();
        }

        public List<ReportOutgo> GetReportOutgoes(int OutgoId)
        {
            var list = from s in objDataContext.ReportOutgoes where s.OutgoID == OutgoId select s;
            return list.ToList();
        }

        public List<ReportOutgo> GetReportOutgoes(Sale sale)
        {
            List<ReportOutgo> outgoes = new List<ReportOutgo>();
            List<Basket> baskets = objDataContext.Baskets.Where(x => x.SaleID == sale.ID).ToList();
            List<OfferingInBasketItem> oibis = new List<OfferingInBasketItem>();
            foreach (Basket basket in baskets)
            {
                oibis.AddRange(objDataContext.OfferingInBasketItems.Where(x => x.BasketItemID == basket.ID));
            }
            foreach (OfferingInBasketItem oibi in oibis)
            {
                outgoes.Add(objDataContext.ReportOutgoes.FirstOrDefault(x => x.id == oibi.OfferingOutgoID));
            }
            return outgoes.ToList();
        }

        public List<SpareInSpareOutgoView> SpareInSpareOutgoViewGet(Sale sale)
        {
            List<SpareInSpareOutgoView> outgoes = new List<SpareInSpareOutgoView>();
            List<Basket> baskets = objDataContext.Baskets.Where(x => x.SaleID == sale.ID).ToList();
            List<OfferingInBasketItem> oibis = new List<OfferingInBasketItem>();
            foreach (Basket basket in baskets)
            {
                oibis.AddRange(objDataContext.OfferingInBasketItems.Where(x => x.BasketItemID == basket.ID));
            }
            foreach (OfferingInBasketItem oibi in oibis)
            {
                outgoes.Add(objDataContext.SpareInSpareOutgoViews.FirstOrDefault(x => x.id == oibi.OfferingOutgoID));
            }
            return outgoes.ToList();
        }

        public List<SpareInSpareIncomeView> GetReportRemains()
        {
            return objDataContext.SpareInSpareIncomeViews.ToList();
        }

        public List<SpareInSpareIncomeView> GetRemains(int SpareIncomeID)
        {
            return objDataContext.SpareInSpareIncomeViews.Where(a => a.SpareIncomeID == SpareIncomeID).ToList();
        }

        public List<SpareInSpareIncomeView> GetRemains(int SpareIncomeID, int SearchFieldIndex, string SearchText)
        {
            List<SpareInSpareIncomeView> res = objDataContext.SpareInSpareIncomeViews.Where(a => a.SpareIncomeID == SpareIncomeID).ToList();
            switch (SearchFieldIndex)
            {
                case 0: // код магазина
                    res = res.Where(s => s.SpareCode != null).Where(s => s.SpareCode.ToLower().Contains(SearchText.ToLower())).ToList();
                    break;

                case 1: // код ШАТЕ-М
                    res = res.Where(s => s.SpareCodeShatem != null).Where(s => s.SpareCodeShatem.ToLower().Contains(SearchText.ToLower())).ToList();
                    break;

                case 2: // наименование
                    res = res.Where(s => s.SpareName.ToLower().Contains(SearchText.ToLower())).ToList();
                    break;
            }
            return res;
        }

        #endregion SPARE INCOME

        #region Offerings in SPARE INCOME

        public List<spare_in_spare_income> SpareInSpareIncomeGet(int SpareIncomeId)
        {
            var items = from s in objDataContext.spare_in_spare_income where s.spare_income.id == SpareIncomeId orderby s.num select s;
            return items.ToList();
        }

        public List<SpareInSpareOutgoView> GetSpareInSpareOutgoByDate(DateTime dt)
        {
            var items = from s in objDataContext.SpareInSpareOutgoViews where s.OutgoDate.Value.Year == dt.Year && s.OutgoDate.Value.Month == dt.Month && s.OutgoDate.Value.Day == dt.Day orderby s.SpareName select s;
            return items.ToList();
        }

        public List<SpareInSpareOutgoView> GetSpareInSpareOutgoByPeriod(DateTime dfrom, DateTime dto)
        {
            var items = from s in objDataContext.SpareInSpareOutgoViews
                        where (s.OutgoDate.Value >= dfrom
                        && s.OutgoDate.Value <= dto)
                        orderby s.OutgoDate.Value
                        select s;
            return items.ToList();
        }

        public List<SpareInSpareOutgoView> GetSpareInSpareOutgoByPeriod(DateTime dfrom, DateTime dto, int warehouseID)
        {
            var items = from s in objDataContext.SpareInSpareOutgoViews
                        where (s.OutgoDate.Value >= dfrom
                        && s.OutgoDate.Value <= dto
                        && s.WarehouseID == warehouseID)
                        orderby s.OutgoDate.Value
                        select s;
            return items.ToList();
        }

        public List<SpareInSpareOutgoView> GetSpareInSpareOutgoByCodePeriod(int SpareID, DateTime dfrom, DateTime dto, int WarehouseID)
        {
            if (WarehouseID == 0)
            {
                return GetSpareInSpareOutgoByCodePeriod(SpareID, dfrom, dto);
            }
            var items = from s in objDataContext.SpareInSpareOutgoViews
                        where (s.OutgoDate.Value >= dfrom
                        && s.OutgoDate.Value <= dto
                        && s.spare_id == SpareID
                        && s.WarehouseID == WarehouseID)
                        orderby s.OutgoDate.Value
                        select s;
            return items.ToList();
        }

        public List<SpareInSpareOutgoView> GetSpareInSpareOutgoByCodePeriod(int SpareID, DateTime dfrom, DateTime dto)
        {
            var items = from s in objDataContext.SpareInSpareOutgoViews
                        where (s.OutgoDate.Value >= dfrom
                        && s.OutgoDate.Value <= dto
                        && s.spare_id == SpareID)
                        orderby s.OutgoDate.Value
                        select s;
            return items.ToList();
        }

        public void InOfferingCreate(spare_in_spare_income _obj, int SpareID)
        {
            objDataContext.AddTospare_in_spare_income(_obj);
            spare sp = objDataContext.spares.FirstOrDefault(s => s.id == SpareID);
            _obj.spare = sp;
            sp.q_rest += _obj.QIn;
            objDataContext.SaveChanges();
        }

        public void InOfferingCreate(spare_in_spare_income _obj, int spareId, int spareIncomeId, string VatRateName)
        {
            string CurrentCurrencyCode = GetCurrency(_obj.CurrencyID.Value).code;

            _obj.QRest = _obj.QIn;
            spare sp = objDataContext.spares.FirstOrDefault(s => s.id == spareId);
            _obj.spare = sp;
            _obj.spare_income = objDataContext.spare_income.FirstOrDefault(s => s.id == spareIncomeId);
            _obj.vat_rate = objDataContext.vat_rate.FirstOrDefault(s => s.name == VatRateName);

            if (CurrentCurrencyCode == CurrencyHelper.BasicCurrencyCode)
            {
                _obj.POutBasic = _obj.POut;
                _obj.PInBasic = _obj.PIn;
            }
            else
            {
                _obj.POutBasic = CurrencyHelper.GetBasicPrice(CurrentCurrencyCode, _obj.POut.Value);
                _obj.PInBasic = CurrencyHelper.GetBasicPrice(CurrentCurrencyCode, _obj.PIn.Value);
            }
            _obj.SBasic = _obj.POutBasic * _obj.QIn;

            objDataContext.AddTospare_in_spare_income(_obj);

            //save quantity
            sp.q_rest += _obj.QIn;
            objDataContext.SaveChanges();
        }

        public void InOfferingCreate(spare_in_spare_income _obj)
        {
            string CurrentCurrencyCode = GetCurrency(_obj.CurrencyID.Value).code;

            _obj.QRest = _obj.QIn;
            _obj.PInBasic = CurrencyHelper.GetBasicPrice(CurrentCurrencyCode, _obj.PIn.Value);
            _obj.POutBasic = _obj.PInBasic * (1 + _obj.Markup / 100 + _obj.vat_rate.rate / 100);
            _obj.SBasic = _obj.PIn * _obj.QIn;

            objDataContext.AddTospare_in_spare_income(_obj);

            //objDataContext.Attach(_obj);
            //save quantity
            _obj.spare.q_rest += _obj.QIn;
            objDataContext.SaveChanges();
        }

        public void InOfferingCreate(int spareId, decimal q, decimal pin, decimal m, decimal pout, decimal s, double incomeId)
        {
            spare_in_spare_income _obj = new spare_in_spare_income();
            _obj.description = "";
            _obj.Markup = m;
            _obj.num = 0;
            _obj.PIn = pin;
            _obj.POut = pout;
            _obj.QIn = q;
            _obj.QRest = q;
            _obj.S = s;

            spare sp = objDataContext.spares.FirstOrDefault(spp => spp.id == spareId);
            spare_income income = objDataContext.spare_income.FirstOrDefault(sii => sii.id == incomeId);
            _obj.spare = sp;
            _obj.spare_income = income;
            _obj.vat_rate = objDataContext.vat_rate.FirstOrDefault(r => r.name == "Без НДС");
            if (income.currency == null)
                income.currencyReference.Load();
            _obj.CurrencyID = income.currency.id;
            string CurrentCurrencyCode = GetCurrency(_obj.CurrencyID.Value).code;
            _obj.QRest = _obj.QIn;
            _obj.PInBasic = CurrencyHelper.GetBasicPrice(CurrentCurrencyCode, _obj.PIn.Value);
            _obj.POutBasic = _obj.PInBasic * (1 + _obj.Markup / 100 + _obj.vat_rate.rate / 100);
            _obj.SBasic = _obj.PIn * _obj.QIn;

            objDataContext.AddTospare_in_spare_income(_obj);

            //save quantity
            sp.q_rest += _obj.QIn;
            objDataContext.SaveChanges();
        }

        public int InOfferingCreate(spare_in_spare_income _obj, string SpareCode1C, int spareIncomeId, string VatRateName)
        {
            try
            {
                _obj.QRest = _obj.QIn;
                spare sp = objDataContext.spares.FirstOrDefault(s => s.code1C == SpareCode1C);
                _obj.spare = sp;
                _obj.spare_income = objDataContext.spare_income.FirstOrDefault(s => s.id == spareIncomeId);
                _obj.vat_rate = objDataContext.vat_rate.FirstOrDefault(s => s.name == VatRateName);
                objDataContext.AddTospare_in_spare_income(_obj);

                //save quantity
                sp.q_rest += _obj.QIn;
                objDataContext.SaveChanges();
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        public void InOfferingEdit(int? OfferingID, decimal Q, decimal P, decimal PInBasic)
        {
            spare_in_spare_income s = objDataContext.spare_in_spare_income.FirstOrDefault(i => i.id == OfferingID.Value);
            s.QIn = Q;
            s.QRest = Q;
            s.POut = P;
            s.S = Q * P;
            s.PInBasic = PInBasic;
            if (s.vat_rate == null)
                s.vat_rateReference.Load();
            s.POutBasic = s.PInBasic * (1 + s.Markup / 100 + s.vat_rate.rate / 100);
            s.SBasic = s.PIn * s.QIn;
            objDataContext.SaveChanges();
        }

        public void InOfferingEdit(spare_in_spare_income e, string VatRateName)
        {
            spare_in_spare_income o = objDataContext.spare_in_spare_income.FirstOrDefault(i => i.id == e.id);
            if (o != null)
            {
                string CurrentCurrencyCode = GetCurrency(o.CurrencyID.Value).code;

                o.description = e.description;
                o.QIn = e.QIn;
                o.QRest = e.QRest;
                o.Markup = e.Markup;
                o.vat_rate = objDataContext.vat_rate.FirstOrDefault(s => s.name == VatRateName);

                o.PIn = e.PIn;
                o.PInBasic = CurrencyHelper.GetBasicPrice(CurrentCurrencyCode, o.PIn.Value);
                o.POut = e.POut;
                o.POutBasic = o.PInBasic * (1 + o.Markup / 100 + o.vat_rate.rate / 100);
                o.S = e.S;
                o.SBasic = o.PIn * o.QIn;

                spare sp = objDataContext.spares.FirstOrDefault(s => s.id == o.spare.id);
                sp.q_rest += e.QIn - o.QIn;

                objDataContext.SaveChanges();
            }
        }

        public void InOfferingDelete(int offeringId)
        {
            int SpareID = -1;

            // получаем запись, которую собираемся удалять
            spare_in_spare_income offering = objDataContext.spare_in_spare_income.FirstOrDefault(i => i.id == offeringId);
            if (offering != null)
            {
                OfferingInBasketItem oibi = objDataContext.OfferingInBasketItems.FirstOrDefault(x => x.OfferingIncomeID == offeringId);
                if (oibi != null)
                {
                    objDataContext.DeleteObject(oibi);
                    objDataContext.SaveChanges();
                }

                if (offering.spare == null)
                    offering.spareReference.Load();
                SpareID = offering.spare.id;

                // удаляем связанные с этим приходом переоценки
                OvOfferingDeleteBySISIID(offeringId);

                // удаляем связанные с этим поступлением отгрузки
                OutgoOfferingDeleteBySISIID(offeringId);
                try
                {
                    objDataContext = new DriveEntities();
                    offering = objDataContext.spare_in_spare_income.FirstOrDefault(i => i.id == offeringId);
                    if (offering != null)
                        objDataContext.DeleteObject(offering);
                }
                catch (Exception)
                {
                }

                // delete offering in overpricing
                objDataContext.SaveChanges();
                SpareContainer.Instance.Update(SpareID);
            }
        }

        public void OvOfferingDeleteBySISIID(int SpareInSpareIncomeID)
        {
            spare_in_overpricing item = objDataContext.spare_in_overpricing.FirstOrDefault(i => i.spare_in_spare_income.id == SpareInSpareIncomeID);
            while (item != null)
            {
                if (item.spare == null)
                    item.spareReference.Load();
                int SpareID = item.spare.id;
                objDataContext.DeleteObject(item);
                objDataContext.SaveChanges();
                item = objDataContext.spare_in_overpricing.FirstOrDefault(i => i.spare_in_spare_income.id == SpareInSpareIncomeID);
            }
        }

        public void OutgoOfferingDeleteBySISIID(int SpareInSpareIncomeID)
        {
            spare_in_spare_outgo item = objDataContext.spare_in_spare_outgo.FirstOrDefault(i => i.spare_in_spare_income.id == SpareInSpareIncomeID);
            while (item != null)
            {
                if (item.spare == null)
                    item.spareReference.Load();
                int SpareID = item.spare.id;
                objDataContext.DeleteObject(item);
                objDataContext.SaveChanges();
                SpareContainer.Instance.Update(SpareID);
                item = objDataContext.spare_in_spare_outgo.FirstOrDefault(i => i.spare_in_spare_income.id == SpareInSpareIncomeID);
            }
        }

        public void InOfferingDeleteBySpareIncomeId(int spareIncomeId)
        {
            spare_in_spare_income item = objDataContext.spare_in_spare_income.FirstOrDefault(i => i.spare_income.id == spareIncomeId);
            while (item != null)
            {
                if (item.spare == null)
                    item.spareReference.Load();
                int SpareID = item.spare.id;
                FixOfferingsOutsOnDeletingIncome(item.id);
                objDataContext.DeleteObject(item);
                objDataContext.SaveChanges();
                SpareContainer.Instance.Update(SpareID);
                item = objDataContext.spare_in_spare_income.FirstOrDefault(i => i.spare_income.id == spareIncomeId);
            }
        }

        public void InOfferingDeleteBySpareID(int SpareID)
        {
            spare_in_spare_income item = objDataContext.spare_in_spare_income.FirstOrDefault(i => i.spare.id == SpareID);
            while (item != null)
            {
                if (item.spare == null)
                    item.spareReference.Load();

                //int SpareID = item.spare.id;
                FixOfferingsOutsOnDeletingIncome(item.id);
                objDataContext.DeleteObject(item);
                objDataContext.SaveChanges();
                SpareContainer.Instance.Update(SpareID);
                item = objDataContext.spare_in_spare_income.FirstOrDefault(i => i.spare.id == SpareID);
            }
        }

        public spare_in_spare_income InOfferingGet(int? offeringId)
        {
            return objDataContext.spare_in_spare_income.FirstOrDefault(i => i.id == offeringId.Value);
        }

        #endregion Offerings in SPARE INCOME

        #region INVOICES

        public List<SpareInInvoiceView> GetSparesByInvoiceID(int invoiceId)
        {
            return objDataContext.SpareInInvoiceViews.Where(a => a.InvoiceID == invoiceId).ToList();
        }

        public void InvoiceOfferingCreate(spare_in_invoice o, int spareId, int InvoiceId, string VatRateName, int SpareInSpareIncomeID)
        {
            spare_in_invoice _obj = new spare_in_invoice();
            _obj.quantity = o.quantity;
            _obj.total_sum = o.total_sum;
            _obj.total_with_vat = o.total_with_vat;
            _obj.price = o.price;
            _obj.vat_rate_sum = o.vat_rate_sum;
            _obj.spare = objDataContext.spares.FirstOrDefault(s => s.id == spareId);
            _obj.invoice = objDataContext.invoices.FirstOrDefault(s => s.id == InvoiceId);
            _obj.vat_rate = objDataContext.vat_rate.FirstOrDefault(s => s.name == VatRateName);
            _obj.spare_in_spare_income = objDataContext.spare_in_spare_income.FirstOrDefault(s => s.id == SpareInSpareIncomeID);
            objDataContext.AddTospare_in_invoice(_obj);
            objDataContext.SaveChanges();
        }

        public spare_in_invoice InvoiceOfferingGet(int offeringId)
        {
            return objDataContext.spare_in_invoice.FirstOrDefault(i => i.id == offeringId);
        }

        public invoice InvoiceCreate(invoice _obj)
        {
            objDataContext = new DriveEntities();
            if (objDataContext.invoices.Count() > 0)
                _obj.InvoiceNumber = objDataContext.invoices.Max(x => x.InvoiceNumber) + 1;
            else
                _obj.InvoiceNumber = 1;
            objDataContext.AddToinvoices(_obj);
            objDataContext.SaveChanges();

            //_obj.InvoiceNumber = _obj.id;
            return _obj;
        }

        public void InvoiceEdit(invoice item)
        {
            invoice original = objDataContext.invoices.FirstOrDefault(b => b.id == item.id);
            if (original != null)
            {
                original.InvoiceDate = item.InvoiceDate;
                original.InvoiceSum = item.InvoiceSum;
                original.AccountBankName = item.AccountBankName;
                original.BankAccountID = item.BankAccountID;
                original.AccountName = item.AccountName;
                original.AccountUNN = item.AccountUNN;
                original.AccountAddress = item.AccountAddress;
                original.AccountBankMFO = item.AccountBankMFO;
                original.account = item.account;
                original.InvoiceNumber = item.InvoiceNumber;

                //original.account = objDataContext.accounts.FirstOrDefault(i => i.name == AccountName);
                //original.base_doc = item.base_doc;
                //original.base_doc_date = item.base_doc_date;
                //original.currency = objDataContext.currencies.FirstOrDefault(i => i.code == CurrencyCode);
                //original.si_date = item.si_date;
                //original.warehouse = objDataContext.warehouses.FirstOrDefault(i => i.name == WarehouseName);
                //original.cashless = item.cashless;
                objDataContext.SaveChanges();
            }
        }

        public void InvoiceEdit(invoice item, int AccountID)
        {
            objDataContext = new DriveEntities();
            invoice original = objDataContext.invoices.FirstOrDefault(b => b.id == item.id);
            if (original != null)
            {
                original.InvoiceDate = item.InvoiceDate;
                original.InvoiceSum = item.InvoiceSum;
                original.AccountBankName = item.AccountBankName;
                original.BankAccountID = item.BankAccountID;

                //original.AccountName = item.AccountName;
                original.AccountUNN = item.AccountUNN;
                original.AccountAddress = item.AccountAddress;
                original.AccountBankMFO = item.AccountBankMFO;
                original.InvoiceNumber = item.InvoiceNumber;

                //original.account = item.account;
                if (AccountID > 0)
                {
                    original.account = objDataContext.accounts.FirstOrDefault(i => i.id == AccountID);
                    original.AccountName = original.account.name;
                }

                //original.base_doc = item.base_doc;
                //original.base_doc_date = item.base_doc_date;
                //original.currency = objDataContext.currencies.FirstOrDefault(i => i.code == CurrencyCode);
                //original.si_date = item.si_date;
                //original.warehouse = objDataContext.warehouses.FirstOrDefault(i => i.name == WarehouseName);
                //original.cashless = item.cashless;
                objDataContext.SaveChanges();
            }
        }

        public void InvoiceDelete(int id)
        {
            objDataContext = new DriveEntities();
            invoice item = objDataContext.invoices.FirstOrDefault(i => i.id == id);
            if (item == null)
                return;
            // сначала удалить все прикрепленные товары
            InvoiceOfferingsDelete(id);

            objDataContext.DeleteObject(item);
            objDataContext.SaveChanges();
        }

        public void InvoiceOfferingDelete(int offeringId)
        {
            spare_in_invoice offering = objDataContext.spare_in_invoice.FirstOrDefault(i => i.id == offeringId);
            objDataContext.DeleteObject(offering);
            objDataContext.SaveChanges();
        }

        public void InvoiceOfferingsDelete(int InvoiceID)
        {
            spare_in_invoice item = objDataContext.spare_in_invoice.FirstOrDefault(i => i.invoice.id == InvoiceID);
            while (item != null)
            {
                objDataContext.DeleteObject(item);
                objDataContext.SaveChanges();
                item = objDataContext.spare_in_invoice.FirstOrDefault(i => i.invoice.id == InvoiceID);
            }
        }

        public void InvoiceOfferingsDeleteBySpareID(int SpareID)
        {
            spare_in_invoice item = objDataContext.spare_in_invoice.FirstOrDefault(i => i.spare.id == SpareID);
            while (item != null)
            {
                objDataContext.DeleteObject(item);
                objDataContext.SaveChanges();
                item = objDataContext.spare_in_invoice.FirstOrDefault(i => i.spare.id == SpareID);
            }
        }

        public void OverpricingOfferingsDeleteBySpareID(int SpareID)
        {
            spare_in_overpricing item = objDataContext.spare_in_overpricing.FirstOrDefault(i => i.spare.id == SpareID);
            while (item != null)
            {
                objDataContext.DeleteObject(item);
                objDataContext.SaveChanges();
                item = objDataContext.spare_in_overpricing.FirstOrDefault(i => i.spare.id == SpareID);
            }
        }

        public List<invoice> InvoiceGet()
        {
            return (from s in objDataContext.invoices orderby s.InvoiceNumber descending select s).ToList();
        }

        public List<Sale> SaleGet()
        {
            return (from s in objDataContext.Sales orderby s.Number descending select s).ToList();
        }

        public Sale SaleGet(int SaleID)
        {
            return objDataContext.Sales.FirstOrDefault(x => x.ID == SaleID);
        }

        public List<BasketView> BasketViewGet(int SaleID)
        {
            return (from s in objDataContext.BasketViews where s.SaleID == SaleID select s).ToList();
        }

        public invoice InvoiceGet(int id)
        {
            return objDataContext.invoices.FirstOrDefault(s => s.id == id);
        }

        public int InvoiceGetMaxId()
        {
            int res = 0;
            if (objDataContext.invoices.Count() == 0)
                return 1;
            else
                res = objDataContext.invoices.Max(s => s.id) + 1;
            return res;
        }

        #endregion INVOICES

        public int GetMaxSaleID()
        {
            if (objDataContext.Sales.Count() > 0)
                return objDataContext.Sales.Max(s => s.Number) + 1;
            else
                return 1;
        }

        #region OVERPRICING

        public void RecalcIncomesPricesBack(int OverpricingID)
        {
            List<SpareInOverpricingView> items = OverpricingOfferingGet(OverpricingID);
            foreach (SpareInOverpricingView siov in items)
            {
                SetNewIncomePrice(siov.IncomeID, siov.percentOld.Value);
            }
        }

        public bool CheckIfIncomeIsAlreadyIncluded(int OverpricingID, int IncomeID)
        {
            return (objDataContext.spare_in_overpricing.Where(i => i.overpricing.id == OverpricingID && i.spare_in_spare_income.id == IncomeID).Count() > 0);
        }

        public void SetNewIncomePrice(int IncomeID, int MarkupNew)
        {
            spare_in_spare_income income = objDataContext.spare_in_spare_income.FirstOrDefault(i => i.id == IncomeID);
            if (income.vat_rate == null)
                income.vat_rateReference.Load();
            income.Markup = MarkupNew;

            income.POut = income.PIn * (1 + income.Markup / 100 + income.vat_rate.rate / 100);
            income.S = income.POut * income.QIn;

            income.POutBasic = income.PInBasic * (1 + income.Markup / 100 + income.vat_rate.rate / 100);
            income.SBasic = income.POutBasic * income.QIn;

            objDataContext.SaveChanges();
        }

        public void OverpricingOfferingCreate(spare_in_overpricing o)
        {
            if (!CheckIfIncomeIsAlreadyIncluded(o.overpricing.id, o.spare_in_spare_income.id))
            {
                objDataContext.AddTospare_in_overpricing(o);
                objDataContext.SaveChanges();
            }
        }

        //public spare_in_overpricing OverpricingOfferingGet(int offeringId)
        //{
        //  return objDataContext.spare_in_overpricing.FirstOrDefault(i => i.id == offeringId);
        //}
        public void OverpricingOfferingEdit(int ID, int i1, decimal d2, decimal d3)
        {
            objDataContext = new DriveEntities();
            spare_in_overpricing s = objDataContext.spare_in_overpricing.FirstOrDefault(i => i.id == ID);
            s.percentNew = i1;
            s.priceNew = d2;
            s.sumNew = d3;
            objDataContext.SaveChanges();
        }

        public void OverpricingOfferingEdit(SpareInOverpricingView siov)
        {
            objDataContext = new DriveEntities();
            spare_in_overpricing s = objDataContext.spare_in_overpricing.FirstOrDefault(i => i.id == siov.id);
            s.percentNew = siov.percentNew;
            s.priceNew = siov.priceNew;
            s.sumNew = siov.sumNew;
            objDataContext.SaveChanges();
        }

        public List<SpareInOverpricingView> OverpricingOfferingGet(int OverpricingID)
        {
            return objDataContext.SpareInOverpricingViews.Where(i => i.overpricing_id == OverpricingID).ToList();
        }

        public overpricing OverpricingCreate(overpricing _obj)
        {
            objDataContext = new DriveEntities();
            objDataContext.AddTooverpricings(_obj);
            objDataContext.SaveChanges();
            return _obj;
        }

        public overpricing OverpricingEdit(overpricing item)
        {
            overpricing original = objDataContext.overpricings.FirstOrDefault(b => b.id == item.id);
            if (original != null)
            {
                original.commited = item.commited;
                original.createdOn = item.createdOn;
                original.increasePerc = item.increasePerc;

                objDataContext.SaveChanges();
            }
            return original;
        }

        public void OverpricingDelete(int id)
        {
            objDataContext = new DriveEntities();
            overpricing item = objDataContext.overpricings.FirstOrDefault(i => i.id == id);
            if (item == null)
                return;
            // откатить
            if (item.commited.Value)
            {
                RecalcIncomesPricesBack(item.id);
            }

            // сначала удалить все прикрепленные товары
            OfferingsDeleteByOverpricingId(id);

            objDataContext.DeleteObject(item);
            objDataContext.SaveChanges();
        }

        public void OverpricingOfferingDelete(int offeringId)
        {
            spare_in_overpricing offering = objDataContext.spare_in_overpricing.FirstOrDefault(i => i.id == offeringId);
            objDataContext.DeleteObject(offering);
            objDataContext.SaveChanges();
        }

        public void OfferingsDeleteByOverpricingId(int ParentID)
        {
            while (objDataContext.spare_in_overpricing.Where(i => i.overpricing.id == ParentID).Count() > 0)
            {
                spare_in_overpricing sio = objDataContext.spare_in_overpricing.FirstOrDefault(i => i.overpricing.id == ParentID);
                objDataContext.DeleteObject(sio);
                objDataContext.SaveChanges();
            }
        }

        public List<overpricing> OverpricingGet()
        {
            return (from s in objDataContext.overpricings select s).ToList();
        }

        public overpricing OverpricingGet(int id)
        {
            return objDataContext.overpricings.FirstOrDefault(s => s.id == id);
        }

        #endregion OVERPRICING

        #region REVISION

        public revision RevisionCreate(revision _obj)
        {
            objDataContext = new DriveEntities();
            objDataContext.AddTorevisions(_obj);
            objDataContext.SaveChanges();
            return _obj;
        }

        public revision revisionEdit(revision item)
        {
            revision original = objDataContext.revisions.FirstOrDefault(b => b.id == item.id);
            if (original != null)
            {
                original.RevisionDate = item.RevisionDate;

                objDataContext.SaveChanges();
            }
            return original;
        }

        public void OfferingsDeleteByRevisionId(int ParentID)
        {
            while (objDataContext.spare_in_revision.Where(i => i.revision.id == ParentID).Count() > 0)
            {
                spare_in_revision sio = objDataContext.spare_in_revision.FirstOrDefault(i => i.revision.id == ParentID);
                objDataContext.DeleteObject(sio);
                objDataContext.SaveChanges();
            }
        }

        public void RevisionDelete(int id)
        {
            objDataContext = new DriveEntities();
            revision item = objDataContext.revisions.FirstOrDefault(i => i.id == id);
            if (item == null)
                return;

            // сначала удалить все прикрепленные товары
            OfferingsDeleteByRevisionId(id);

            objDataContext.DeleteObject(item);
            objDataContext.SaveChanges();
        }

        public List<revision> RevisionGet()
        {
            return (from s in objDataContext.revisions select s).ToList();
        }

        public revision RevisionGet(int id)
        {
            return objDataContext.revisions.FirstOrDefault(s => s.id == id);
        }

        public int revisionGetMaxId()
        {
            int res = 0;
            if (objDataContext.revisions.Count() == 0)
                return 1;
            else
                res = objDataContext.revisions.Max(s => s.id) + 1;
            return res;
        }

        #endregion REVISION

        #region SPARE Outgo

        public SpareOutgoView GetSpareOutgoView(int ID)
        {
            return objDataContext.SpareOutgoViews.FirstOrDefault(i => i.id == ID);
        }

        public List<SpareOutgoView> SpareOutgoGetAll()
        {
            return (from s in objDataContext.SpareOutgoViews select s).ToList();
        }

        public int SpareOutgoCreate(spare_outgo _obj, string CurrencyCode)
        {
            _obj.currency = objDataContext.currencies.FirstOrDefault(i => i.code == CurrencyCode);
            objDataContext.AddTospare_outgo(_obj);
            objDataContext.SaveChanges();
            return _obj.id;
        }

        public int SpareOutgoOpenedID()
        {
            spare_outgo sout = objDataContext.spare_outgo.FirstOrDefault(s => s.opened == 1);
            if (sout != null)
                return sout.id;
            else
                return 0;
        }

        public spare_outgo SpareOutgoOpened()
        {
            spare_outgo sout = objDataContext.spare_outgo.FirstOrDefault(s => s.opened == 1);
            if (sout != null)
                return sout;
            else
                return null;
        }

        public void SpareOutgoSetOpened(int SpareOutgoID)
        {
            spare_outgo sout = objDataContext.spare_outgo.FirstOrDefault(s => s.opened == 1);
            if (sout != null)
                sout.opened = 0;
            objDataContext.spare_outgo.FirstOrDefault(s => s.id == SpareOutgoID).opened = 1;
            objDataContext.SaveChanges();
        }

        public void SpareOutgoEdit(spare_outgo i, string CurrencyCode)
        {
            spare_outgo o = objDataContext.spare_outgo.FirstOrDefault(b => b.id == i.id);
            if (o != null)
            {
                o.currency = objDataContext.currencies.FirstOrDefault(c => c.code == CurrencyCode);
                o.description = i.description;

                o.unn = i.unn;
                o.unloading = i.unloading;
                o.truckowner = i.truckowner;
                o.truck = i.truck;
                o.tripsheet = i.tripsheet;
                o.trailer = i.trailer;
                o.driver = i.driver;
                o.deliverer = i.deliverer;
                o.AccountID = i.AccountID;
                o.basement = i.basement;
                o.address = i.address;
                o.accepter = i.accepter;
                o.warrant = i.warrant;

                objDataContext.SaveChanges();
            }
        }

        public void SpareOutgoDelete(int spareOutgoId)
        {
            objDataContext = new DriveEntities();
            spare_outgo item = objDataContext.spare_outgo.FirstOrDefault(i => i.id == spareOutgoId);

            // сначала удалить все прикрепленные товары
            OutOfferingDeleteBySpareOutgoId(spareOutgoId);

            objDataContext.DeleteObject(item);
            objDataContext.SaveChanges();
        }

        public spare_outgo SpareOutgoGet(int Id)
        {
            return objDataContext.spare_outgo.FirstOrDefault(i => i.id == Id);
        }

        public int SpareOutgoGetMaxId()
        {
            int res = 0;
            if (objDataContext.spare_outgo.Count() == 0)
                return 1;
            else
                res = objDataContext.spare_outgo.Max(s => s.IDN.Value) + 1;
            return res;
        }

        public int OverpricingGetMaxID()
        {
            int res = 0;
            if (objDataContext.overpricings.Count() == 0)
                return 1;
            else
                res = objDataContext.overpricings.Max(s => s.num.Value) + 1;
            return res;
        }

        #endregion SPARE Outgo

        #region Offering In Spare Outgo

        public void OutOfferingDeleteBySpareOutgoId(int spareOutgoId)
        {
            spare_in_spare_outgo item = objDataContext.spare_in_spare_outgo.FirstOrDefault(i => i.spare_outgo.id == spareOutgoId);
            while (item != null)
            {
                item.spareReference.Load();
                int SpareID = item.spare.id;
                item.spare_in_spare_incomeReference.Load();
                if (item.spare_in_spare_income != null)
                {
                    spare_in_spare_income inc = objDataContext.spare_in_spare_income.FirstOrDefault(x => x.id == item.spare_in_spare_income.id);
                    inc.QRest += item.quantity;
                }
                objDataContext.DeleteObject(item);
                objDataContext.SaveChanges();
                SpareContainer.Instance.Update(SpareID);
                item = objDataContext.spare_in_spare_outgo.FirstOrDefault(i => i.spare_outgo.id == spareOutgoId);
            }
        }

        public void FixOfferingsOutsOnDeletingIncome(int SpareInSpareIncomeID)
        {
            bool fl = false;
            var items = objDataContext.spare_in_spare_outgo.Where(x => x.spare_in_spare_income.id == SpareInSpareIncomeID);
            foreach (spare_in_spare_outgo outgo in items)
            {
                outgo.spare_in_spare_income = null;
                if (!fl)
                    fl = true;
            }
            if (fl)
                objDataContext.SaveChanges();
        }

        public List<spare_in_spare_outgo> SpareInSpareOutgoGet(int SpareOutgoId)
        {
            var items = from s in objDataContext.spare_in_spare_outgo where s.spare_outgo.id == SpareOutgoId orderby s.num select s;
            return items.ToList();
        }

        public List<SpareInSpareOutgoView> SpareInSpareOutgoViewGet(int SpareOutgoId)
        {
            var items = from s in objDataContext.SpareInSpareOutgoViews where s.spare_outgo_id == SpareOutgoId select s;
            return items.ToList();
        }

        public int SpareInSpareOutgoCreate(int SpareInSpareIncomeID, decimal quantity, int SpareOutgoID
                                            , decimal Price, decimal BasicPrice, decimal discount, decimal VatRate)
        {
            // создать запись spare_in_spare_outgo
            spare_in_spare_outgo sout = new spare_in_spare_outgo();

            // отминусовать от актуального количества в соотвествующей spare_in_spare_income
            // если в накладной стоит галочка "Без списания" - не миусуем.
            spare_outgo outgo = objDataContext.spare_outgo.FirstOrDefault(s => s.id == SpareOutgoID);
            spare_in_spare_income sin = objDataContext.spare_in_spare_income.FirstOrDefault(i => i.id == SpareInSpareIncomeID);
            if (outgo.isGhost != 1)
                sin.QRest -= quantity;

            sin.spareReference.Load();
            sout.spare = sin.spare;
            sout.spare_in_spare_income = sin;
            sout.spare_outgo = outgo;
            sout.purchase_price = Price;
            sout.basic_price = BasicPrice;
            sout.quantity = quantity;
            sout.markup_percentage = sin.Markup;
            sout.total_sum = quantity * sout.purchase_price.Value;
            sout.discount = discount;
            sout.num = 0;
            objDataContext.AddTospare_in_spare_outgo(sout);

            objDataContext.SaveChanges();
            return sin.spare.id;
        }

        public spare_in_spare_outgo OutOfferingGet(int offeringId)
        {
            return objDataContext.spare_in_spare_outgo.FirstOrDefault(i => i.id == offeringId);
        }

        public void OutOfferingDelete(int offeringId)
        {
            objDataContext = new DriveEntities();
            spare_in_spare_outgo offering = objDataContext.spare_in_spare_outgo.FirstOrDefault(i => i.id == offeringId);
            if (offering == null)
                return;
            // delete linked basket item
            OfferingInBasketItem oibi = objDataContext.OfferingInBasketItems.FirstOrDefault(x => x.OfferingOutgoID == offeringId);
            if (oibi != null)
            {
                int BasketID = oibi.BasketItemID;
                objDataContext.DeleteObject(oibi);
                Basket basket = objDataContext.Baskets.FirstOrDefault(x => x.ID == BasketID);
                if (basket != null)
                    objDataContext.DeleteObject(basket);

                //objDataContext.SaveChanges();
            }

            if (offering.spare_in_spare_income == null)
                offering.spare_in_spare_incomeReference.Load();
            if (offering.spare_in_spare_income != null)
            {
                spare_in_spare_income sin = objDataContext.spare_in_spare_income.FirstOrDefault(s => s.id == offering.spare_in_spare_income.id);
                offering.spare_outgoReference.Load();
                spare_outgo outgo = objDataContext.spare_outgo.FirstOrDefault(i => i.id == offering.spare_outgo.id);
                if (sin != null && outgo.isGhost != 1)
                    sin.QRest += offering.quantity;
            }
            objDataContext.DeleteObject(offering);
            objDataContext.SaveChanges();
        }

        public void OutOfferingCreate(spare_in_spare_outgo _obj, int spareId, int spareMovementId)
        {
            _obj.spare = objDataContext.spares.FirstOrDefault(s => s.id == spareId);
            _obj.spare_outgo = objDataContext.spare_outgo.FirstOrDefault(s => s.id == spareMovementId);
            objDataContext.AddTospare_in_spare_outgo(_obj);
            objDataContext.SaveChanges();
        }

        public void OutOfferingEdit(spare_in_spare_outgo e, int spareId)
        {
            spare_in_spare_outgo o = objDataContext.spare_in_spare_outgo.FirstOrDefault(i => i.id == e.id);
            if (o != null)
            {
                o.description = e.description;
                o.markup_percentage = e.markup_percentage;

                //o.num = e.num;
                o.purchase_price = e.purchase_price;
                o.quantity = e.quantity;
                o.spare = objDataContext.spares.FirstOrDefault(s => s.id == spareId);
                o.total_sum = e.total_sum;

                //o.vat_rate =
                objDataContext.SaveChanges();
            }
        }

        #endregion Offering In Spare Outgo

        #region COMMOMS

        public IList getItems(string ClassName)
        {
            return getItems(ClassName, 0);
        }

        public IList getItems(string ClassName, int ParentItemID)
        {
            IList items = null;
            if (ClassName.Equals((new account()).ToString()))
            {
                items = GetAllAccounts();
            }
            if (ClassName.Equals((new bank()).ToString()))
            {
                items = GetAllBanks();
            }
            if (ClassName.Equals((new warehouse()).ToString()))
            {
                items = GetWarehouses();
            }
            if (ClassName.Equals((new brand()).ToString()))
            {
                items = GetBrands();
            }
            if (ClassName.Equals((new car_producer()).ToString()))
            {
                items = GetCarProducers();
            }
            if (ClassName.Equals((new spare_group()).ToString()))
            {
                items = GetSpareGroupEnds();
            }
            if (ClassName.Equals((new unit()).ToString()))
            {
                items = GetUnits();
            }
            if (ClassName.Equals((new bank_account()).ToString()))
            {
                items = BankAccountViewGet(ParentItemID);
            }
            if (ClassName.Equals((new BankAccountView()).ToString()))
            {
                items = BankAccountViewGet(ParentItemID);
            }
            return items;
        }

        public IList getItems(int searchFieldIndex, string searchString, string ClassName)
        {
            return getItems(searchFieldIndex, searchString, ClassName, 0);
        }

        public IList getItems(int searchFieldIndex, string searchString, string ClassName, int ParentItemID)
        {
            IList items = null;
            if (ClassName.Equals((new account()).ToString()))
            {
                items = GetAccounts(searchFieldIndex, searchString);
            }
            if (ClassName.Equals((new bank_account()).ToString()))
            {
                items = BankAccountViewGet(searchFieldIndex, searchString, ParentItemID);
            }
            if (ClassName.Equals((new BankAccountView()).ToString()))
            {
                items = BankAccountViewGet(searchFieldIndex, searchString, ParentItemID);
            }
            if (ClassName.Equals((new warehouse()).ToString()))
            {
                items = GetWarehouses(searchFieldIndex, searchString);
            }
            if (ClassName.Equals((new bank()).ToString()))
            {
                items = GetBanks(searchFieldIndex, searchString);
            }
            if (ClassName.Equals((new car_producer()).ToString()))
            {
                items = GetCarProducers(searchFieldIndex, searchString);
            }
            if (ClassName.Equals((new brand()).ToString()))
            {
                items = GetBrands(searchFieldIndex, searchString);
            }
            if (ClassName.Equals((new spare_group()).ToString()))
            {
                items = GetSpareGroups(searchFieldIndex, searchString);
            }
            if (ClassName.Equals((new unit()).ToString()))
            {
                items = GetUnits(searchFieldIndex, searchString);
            }
            return items;
        }

        #endregion COMMOMS

        #region PROFILES

        public settings_profile getProfileCurrent()
        {
            if (objDataContext.settings_profile.Count() == 0)
            {
                settings_profile p = new settings_profile();
                p.AddressFact = "";
                p.AddressJur = "";
                p.BasicCurrencyCode = "BYR";
                p.CompanyHead = "";
                p.CompanyName = "";
                p.DefaultIncomeCurrencyCode = "BYR";
                p.DefaultIsCashless = 0;
                p.DefSearchFieldIndex = 0;
                p.IsCurrent = 1;
                p.LoadPoint = "";
                p.OKPO = "";
                p.SeniorAccountant = "";
                p.UNN = "";
                p.UseScanner = 0;
                objDataContext.AddTosettings_profile(p);
                objDataContext.SaveChanges();
            }
            return objDataContext.settings_profile.FirstOrDefault(i => i.IsCurrent == 1);
        }

        public ProfileBankAccountView getProfileBankAccountCurrent()
        {
            ProfileBankAccountView res = null;
            res = objDataContext.ProfileBankAccountViews.FirstOrDefault(i => i.IsMain == 1);
            //if (res == null)
            //    res = objDataContext.ProfileBankAccountViews.ToList()[0];
            return res;
        }

        public int ProfileCreate(settings_profile profile)
        {
            objDataContext.AddTosettings_profile(profile);
            return profile.id;
        }

        public List<settings_profile> getProfiles()
        {
            return objDataContext.settings_profile.ToList();
        }

        public int ProfileEdit(settings_profile p)
        {
            settings_profile o = objDataContext.settings_profile.FirstOrDefault(i => i.id == p.id);
            o.AddressFact = p.AddressFact;
            o.AddressJur = p.AddressJur;
            o.BasicCurrencyCode = p.BasicCurrencyCode;
            o.CompanyHead = p.CompanyHead;
            o.CompanyName = p.CompanyName;
            o.DefaultIncomeCurrencyCode = p.DefaultIncomeCurrencyCode;
            o.DefaultIsCashless = p.DefaultIsCashless;

            //o.IsCurrent = p.IsCurrent;
            o.LoadPoint = p.LoadPoint;
            o.OKPO = p.OKPO;
            o.SeniorAccountant = p.SeniorAccountant;
            o.UNN = p.UNN;
            o.DefSearchFieldIndex = p.DefSearchFieldIndex;
            o.UseScanner = p.UseScanner;
            o.SimpleInput = p.SimpleInput;
            objDataContext.SaveChanges();
            return o.id;
        }

        public void ProfileDelete(int id)
        {
            settings_profile p = objDataContext.settings_profile.FirstOrDefault(i => i.id == id);
            objDataContext.DeleteObject(p);
            objDataContext.SaveChanges();
        }

        #endregion PROFILES
    }
}