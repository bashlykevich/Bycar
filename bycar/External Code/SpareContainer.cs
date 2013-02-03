using System.Collections.Generic;
using System.Linq;
using bycar;
using bycar.Utils;
using bycar.External_Code;
using System;

namespace bycar3.External_Code
{
    public class SpareContainer
    {
        private static SpareContainer instance = null;
        private List<SpareView> spares = null;

        public List<SpareView> Spares
        {
            get
            {
                if (spares == null)
                    Update();                
                return spares;
            }
        }

        // DISABLE
        /*
        double RS = -1;
        public double RemainsSum()
        {
            if (spares == null)
                Update();
            if (RS < 0)
            {
               DataAccess da = new DataAccess();                   
                RS = 0;
                var remains = from s in spares where s.QRest > 0 select s;
                //remains = remains.Where(i => i.QRest > 0).ToList();
                foreach (SpareView sv in remains)
                {
                    var incomes = da.GetIncomes(sv.id);
                    foreach (SpareInSpareIncomeView i in incomes)
                    {
                        decimal POutBasic = 0;
                        if (!i.POutBasic.HasValue)
                        {
                            string IncomeCurrencyCode = i.CurrencyCode;
                            decimal PIn = i.PIn.Value;
                            POutBasic = CurrencyHelper.GetBasicPrice(IncomeCurrencyCode, PIn);
                        }
                        else
                        {
                            POutBasic = i.POutBasic.Value;
                        }
                        RS += (double)(POutBasic * i.QRest.Value);
                    }
                }               
            }
            return RS;
        }*/
     
        public List<SpareView> Remains
        {
            get
            {
                if (spares == null)
                    Update();
                List<SpareView> remains = spares.Where(i => i.QRest.HasValue).ToList();
                return remains.Where(i => i.QRest > 0).ToList();
            }
        }

        public static SpareContainer Instance
        {
            get
            {
                if (instance == null)
                    instance = new SpareContainer();
                return instance;
            }
        }

        private SpareContainer()
        {
        }

        public void Update()
        {
            DataAccess da = new DataAccess();           
            spares = da.GetSpares();            
        }        
        /*
         * OLD
        public void Update(SpareView spare)
        {
            if (Spares.Where(x => x!= null).Where(x => x.id != null).Where(x => x.id == spare.id).Count() > 0)
            {
                SpareView sv = spares.Where(x => x != null).Where(x => x.id != null).FirstOrDefault(x => x.id == spare.id);
                if(sv != null)
                    Spares.Remove(sv);
            }
            Spares.Add(spare);
        }*/
        /* dec2012
        public void Update(SpareView NewSpareInstance, bool UpdateFromDB = true) // обновление без удаления
        {
            if (UpdateFromDB)
            {
                DataAccess da = new DataAccess();                
                SpareView OldSpareInstance = Spares.Where(x => x != null).FirstOrDefault(x => x.id == NewSpareInstance.id);
                // если в кэше такая есть, запоминает индекс и перезаписываем
                if (OldSpareInstance != null)
                {
                    int i = Spares.IndexOf(OldSpareInstance);
                    spares[i] = NewSpareInstance;
                }
                else
                {
                    spares.Add(NewSpareInstance);
                }
            }
            else
            {
            List<SpareView> items = this.Spares;
            SpareView ind = items.Where(x => x != null).FirstOrDefault(x => x.id == NewSpareInstance.id);
            if (ind != null)
            {
                int i = spares.IndexOf(ind);
                spares[i] = NewSpareInstance;
            }
            else
            {
                spares.Add(NewSpareInstance);
            }
        }
        }
        */
        public void Update(int SpareID, bool UpdateFromDB = true)
        {
            Helper.CalculateQRests(SpareID);
            if (UpdateFromDB)
            {
                DataAccess da = new DataAccess();
                SpareView OldSpareInstance = Spares.Where(x => x != null).FirstOrDefault(x => x.id == SpareID);
                SpareView NewSpareInstance = da.GetSpareView(SpareID);
                // если в кэше такая есть, запоминает индекс и перезаписываем
                if (OldSpareInstance != null)
                {
                    int i = Spares.IndexOf(OldSpareInstance);
                    spares[i] = NewSpareInstance;
                }
                else
                {                                     
                    spares.Add(NewSpareInstance);
                }
            }
            else
            {
                List<SpareView> items = this.Spares.ToList();
                SpareView ind = items.Where(x => x != null).FirstOrDefault(x => x.id == SpareID);
                if (ind != null)
                {
                    int i = spares.IndexOf(ind);
                    spares[i] = ind;
                }
                else
                {
                    DataAccess db = new DataAccess();
                    SpareView sv = db.GetSpareView(SpareID);
                    spares.Add(sv);
                }
            }            
        }          

        public void Remove(int id)
        {
            SpareView sv = Spares.FirstOrDefault(x => x.id == id);
            spares.Remove(sv);
        }

        public SpareView GetSpare(int ID)
        {
            return Spares.FirstOrDefault(s => s.id == ID);
        }

        public List<SpareView> GetSpares(int searchFieldIndex, string searchString)
        {
            List<SpareView> items = new List<SpareView>();
            searchString = searchString.ToLower();
            switch (searchFieldIndex)
            {
                case 0:// ПОИСК ПО КОДУ
                    items = Spares.Where(s => s != null).Where(s => s.code != null).Where(s => s.code.ToLower().Contains(searchString.ToLower())).ToList();
                    break;

                case 1:// ПОИСК ПО НАИМЕНОВАНИЮ
                    items = Spares.Where(s => s != null).Where(s => s.name != null).Where(s => s.name.ToLower().Contains(searchString.ToLower())).ToList();
                    break;

                case 2:// ПОИСК ПО КОДУ ШАТЕ-М
                    items = Spares.Where(s => s != null).Where(s => s.codeShatem != null).Where(s => s.codeShatem.ToLower().Contains(searchString.ToLower())).ToList();
                    break;
            }
            if (items == null)
                items = new List<SpareView>();
            return items.ToList();
        }

        public List<SpareView> GetSparesStrict(int searchFieldIndex, string searchString)
        {
            List<SpareView> items = new List<SpareView>();
            searchString = searchString.ToLower();
            switch (searchFieldIndex)
            {
                case 0:// ПОИСК ПО КОДУ
                    items = Spares.Where(s => s != null).Where(s => s.code != null).Where(s => s.code.ToLower().Equals(searchString)).ToList();
                    break;

                case 1:// ПОИСК ПО НАИМЕНОВАНИЮ
                    items = Spares.Where(s => s != null).Where(s => s.name != null).Where(s => s.name.ToLower().Equals(searchString)).ToList();
                    break;

                case 2:// ПОИСК ПО КОДУ ШАТЕ-М
                    items = Spares.Where(s => s != null).Where(s => s.codeShatem != null).Where(s => s.codeShatem.ToLower().Equals(searchString)).ToList();
                    break;
            }
            if (items == null)
                items = new List<SpareView>();
            return items.ToList();
        }

        public List<SpareView> GetSpares(
           int SearchFieldIndex,
           string SearchText,
           bool RemainsOnly,
           int GroupID,
           string BrandName)
        {
            DataAccess da = new DataAccess();
            List<SpareView> ResultList = new List<SpareView>();

            int BrandID = 0;
            if (BrandName.Length > 0)
                BrandID = da.GetBrandIDByName(BrandName);

            //int GroupID,
            if (GroupID > 0 && (!(BrandID > 0)))
            {
                if (GroupID > 0)
                {
                    ResultList = spares.ToList().Where(x => (x.GroupID == GroupID)
                        || (x.spare_group1_id == GroupID)
                        || (x.spare_group2_id == GroupID)
                        || (x.spare_group3_id == GroupID)).ToList();
                }
            }
            else

                //int BrandID
                if (BrandID > 0)
                {
                    if (GroupID > 0)
                        ResultList = spares.ToList().Where(x => ((x.GroupID == GroupID)
                        || (x.spare_group1_id == GroupID)
                        || (x.spare_group2_id == GroupID)
                        || (x.spare_group3_id == GroupID))
                        && (x.BrandID == BrandID)).ToList();
                }
                else
                    ResultList = spares;

            //bool RemainsOnly,
            if (RemainsOnly)
            {
                ResultList = ResultList.ToList().Where(i => i.QRest > 0).ToList();
            }

            //int SearchFieldIndex,
            //string SearchText,
            if (SearchText.Length > 0)
            {
                // разобрать в завимосости от выбранного для поиска поле
                switch (SearchFieldIndex)
                {
                    case 0:// ПОИСК ПО КОДУ
                        ResultList = ResultList.ToList().Where(s => s.code != null).Where(s => s.code.ToLower().Contains(SearchText.ToLower())).ToList();
                        break;

                    case 1:// ПОИСК ПО НАИМЕНОВАНИЮ
                        ResultList = ResultList.ToList().Where(s => s.name.ToLower().Contains(SearchText.ToLower())).ToList();
                        break;

                    case 2:// ПОИСК ПО КОДУ ШАТЕ-М
                        ResultList = ResultList.ToList().Where(s => s.codeShatem != null).Where(s => s.codeShatem.ToLower().Contains(SearchText.ToLower())).ToList();
                        break;
                }
            }

            return ResultList.ToList();
        }
        public void FixQuantity(int SpareID, decimal NewQuantity)
        {
            DataAccess db = new DataAccess();            
            db.FixIncomeQuantity(SpareID, NewQuantity);           
        }
    }
}