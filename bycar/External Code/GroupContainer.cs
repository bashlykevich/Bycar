using System;
using System.Collections.Generic;
using System.Linq;

namespace bycar.External_Code
{
    internal class GroupContainer
    {
        private static GroupContainer instance = null;
        private List<spare_group> items = null;

        public List<spare_group> Groups
        {
            get
            {
                if (items == null)
                    Update();
                return items;
            }
        }

        public static GroupContainer Instance
        {
            get
            {
                if (instance == null)
                    instance = new GroupContainer();
                return instance;
            }
        }

        private GroupContainer()
        {
        }

        public void Update()
        {
            DataAccess da = new DataAccess();
            items = da.GetSpareGroups();
        }

        public void Update(spare_group item) // обновление без удаления
        {
            spare_group ind = items.Where(x => x != null).FirstOrDefault(x => x.id == item.id);
            if (ind != null)
            {
                int i = items.IndexOf(ind);
                items[i] = item;
            }
            else
            {
                items.Add(item);
            }
        }

        public void Update(int GroupID)
        {
            throw new Exception("not implemented");
            /*
            if (items.Where(x => x != null).Where(x => x.id == GroupID).Count() > 0)
            {
                items.Remove(items.Where(x => x != null).FirstOrDefault(x => x.id == GroupID));
            }
            DataAccess da = new DataAccess();
            spare_group sv = da.GetSpareGroup(GroupID);
            spares.Add(sv);*/
        }

        public void Remove(int id)
        {
            spare_group sv = items.FirstOrDefault(x => x.id == id);
            items.Remove(sv);
        }

        public spare_group GetSpare(int ID)
        {
            return Groups.FirstOrDefault(s => s.id == ID);
        }

        public List<spare_group> GetGroups(int searchFieldIndex, string searchString)
        {
            throw new Exception("not implemented");

            //List<spare_group> items = new List<spare_group>();
            /*
            switch (searchFieldIndex)
            {
                case 0:// ПОИСК ПО КОДУ
                    items = Spares.Where(s => s != null).Where(s => s.codeShatem != null).Where(s => s.code.ToLower().Contains(searchString.ToLower())).ToList();
                    break;

                case 1:// ПОИСК ПО НАИМЕНОВАНИЮ
                    items = Spares.Where(s => s != null).Where(s => s.codeShatem != null).Where(s => s.name.ToLower().Contains(searchString.ToLower())).ToList();
                    break;

                case 2:// ПОИСК ПО КОДУ ШАТЕ-М
                    items = Spares.Where(s => s != null).Where(s => s.codeShatem != null).Where(s => s.codeShatem.ToLower().Contains(searchString.ToLower())).ToList();
                    break;
            }

            if (items == null)
                items = new List<SpareView>();
            */

            //return items.ToList();
        }

        /*
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
                    ResultList = spares.Where(x => (x.GroupID == GroupID)
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
                        ResultList = spares.Where(x => ((x.GroupID == GroupID)
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
                ResultList = ResultList.Where(i => i.QRest > 0).ToList();
            }

            //int SearchFieldIndex,
            //string SearchText,
            if (SearchText.Length > 0)
            {
                // разобрать в завимосости от выбранного для поиска поле
                switch (SearchFieldIndex)
                {
                    case 0:// ПОИСК ПО КОДУ
                        ResultList = ResultList.Where(s => s.code != null).Where(s => s.code.ToLower().Contains(SearchText.ToLower())).ToList();
                        break;

                    case 1:// ПОИСК ПО НАИМЕНОВАНИЮ
                        ResultList = ResultList.Where(s => s.name.ToLower().Contains(SearchText.ToLower())).ToList();
                        break;

                    case 2:// ПОИСК ПО КОДУ ШАТЕ-М
                        ResultList = ResultList.Where(s => s.codeShatem != null).Where(s => s.codeShatem.ToLower().Contains(SearchText.ToLower())).ToList();
                        break;
                }
            }

            return ResultList.ToList();
        }*/
    }
}