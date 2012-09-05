using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bycar;
using System.Collections;

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
                List<SpareView> items = spares;
                return items; 
            }
        }
        
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
        
        public void Update(SpareView spare) // обновление без удаления
        {
            List<SpareView> items = this.Spares;
            SpareView ind = items.Where(x => x != null).Where(x => x.id != null).FirstOrDefault(x => x.id == spare.id);
            if (ind != null)
            {
                int i = spares.IndexOf(ind);
                spares[i] = spare;
            }
            else
            {
                spares.Add(spare);
            }
        }
        public void Update(int SpareId)
        {
            List<SpareView> items = this.Spares.ToList();
            SpareView ind = items.Where(x => x != null).Where(x => x.id != null).FirstOrDefault(x => x.id == SpareId);
            if (ind != null)
            {
                int i = spares.IndexOf(ind);
                spares[i] = ind;
            }
            else
            {
                DataAccess db = new DataAccess();
                SpareView sv = db.GetSpareView(SpareId);
                spares.Add(sv);
            }
        } 
        /*
        public void Update(int SpareId)
        {            
            if (spares.Where(x => x != null).Where(x => x.id == SpareId).Count() > 0)
            {
                spares.Remove(spares.Where(x => x != null).FirstOrDefault(x => x.id == SpareId));
            }
            DataAccess da = new DataAccess();
            SpareView sv = da.GetSpareView(SpareId);
            spares.Add(sv);            
        }*/        
        public void Remove(int id)
        {
            SpareView sv = spares.FirstOrDefault(x => x.id == id);            
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

    }
}
