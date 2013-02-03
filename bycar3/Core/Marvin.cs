using System;
using System.Linq;
using bycar;
using bycar3.External_Code;
using bycar3.Views;
using bycar3.Views.Main_window;

namespace bycar3.Core
{
    internal enum Views { Spares, Outgoes, Incomes, Spare };

    public class Marvin
    {
        public admin_unit CurrentUser = null;

        #region SINGLETON

        private static readonly Marvin instance = new Marvin();

        public static Marvin Instance
        {
            get { return instance; }
        }

        /// Защищенный конструктор нужен, чтобы предотвратить создание экземпляра класса Singleton
        protected Marvin()
        {
            try
            {
                timer = new System.Timers.Timer();
                timer.AutoReset = true;
                timer.Interval = 10000; //in milliseconds
                timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);

                // включаем таймер
                timer.Enabled = true;
            }
            catch (Exception e)
            {
                this.Log(e.Message);
            }
        }

        #endregion SINGLETON

        public static int LastCheckedID = 0;

        private void timer_Elapsed(object source, System.Timers.ElapsedEventArgs e)
        {
            lock (SpareContainer.Instance)
            {
                try
                {
                    if (!ready())
                        return;
                    DriveEntities db = new DriveEntities();
                    if (LastCheckedID == 0)
                        LastCheckedID = db.SpareLogs.Max(x => x.ID);
                    var events = from s in db.SpareLogs where (s.UserSPID != this.SPID && s.ID > LastCheckedID) select s;
                    foreach (SpareLog log in events)
                    {
                        if (log.EventType == "U" || log.EventType == "I")
                        {
                            int SpareID = log.RecordID;

                            SpareContainer.Instance.Update(SpareID);
                            mainWindowObj.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                new System.Windows.Threading.DispatcherOperationCallback(delegate
                            {
                                mainWindowObj.LoadSpares();
                                return null;
                            }), null);

                        }
                        else if (log.EventType == "D")
                        {
                            SpareContainer.Instance.Remove(log.RecordID);
                            mainWindowObj.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                            new System.Windows.Threading.DispatcherOperationCallback(delegate
                            {
                                mainWindowObj.LoadSpares();
                                return null;
                            }), null);
                        }
                    }
                    LastCheckedID = db.SpareLogs.Max(x => x.ID);
                }
                catch (Exception ex1)
                {
                    this.Log(ex1.Message);
                }
            }
        }

        #region ПОЛЯ

        private Views CurrentView = Views.Spares;
        private UCSpares mainWindowObj = null;
        private int spid = 0;
        protected System.Timers.Timer timer;

        #endregion ПОЛЯ

        #region СВОЙСТВА

        public UCSpares MainWindowObj
        {
            get { return mainWindowObj; }
            set { mainWindowObj = value; }
        }

        public int SPID
        {
            get
            {
                if (spid == 0)
                {
                    DataAccess da = new DataAccess();
                    spid = da.SPID;
                }
                return spid;
            }
        }

        #endregion СВОЙСТВА

        // МЕТОДЫ
        private bool ready()
        {
            if (mainWindowObj == null)

                //throw new Exception("mainWindowObj == null");
                return false;
            return true;
        }

        // логгирование
        public void Log(string text)
        {
            /*
            string LogName = "log.txt";
            StreamWriter log;
            if (!File.Exists(LogName))
            {
                log = new StreamWriter(LogName);
            }
            else
            {
                log = File.AppendText(LogName);
            }

            // Write to the file:
            log.WriteLine(DateTime.Now + " MachineName: " + Environment.MachineName + "; UserName: " + Environment.UserName);
            log.WriteLine(text);
            log.WriteLine();

            // Close the stream:
            log.Close();*/
        }

        // ЗАПЧАСТЬ - ДОБАВИТЬ
        public SpareView SpareCreate()
        {
            if (!ready())
                return null;// null;
            SpareView result = null;

            SpareEditView v = new SpareEditView();
            v._id = -1;
            v.ParentWorkspace = mainWindowObj;
            v.ShowDialog();
            if (v._spare != null)
            {
                DataAccess db = new DataAccess();
                result = db.GetSpareView(v._spare.id);

                mainWindowObj.LoadSpares();
                mainWindowObj.LoadGroups(false);
            }
            return result;
        }

        public SpareView SpareCreateSilent(string Name, string CodeShatem, string GroupName, string ParentGroupName, string BrandName, string UnitName, string Description)
        {
            DataAccess da = new DataAccess();
            spare s = da.SpareCreateSilent(Name, CodeShatem, GroupName, ParentGroupName, BrandName, UnitName, Description);
            SpareView SpareViewItem = da.GetSpareView(s.id);
            SpareContainer.Instance.Update(s.id);
            int BrandID = SpareViewItem.BrandID;
            int GroupID = SpareViewItem.GroupID;
            if (SpareContainer.Instance.Spares.Where(i => i.BrandID == BrandID && i.GroupID == GroupID).Count() == 1)
            {
                if (s.brand == null)
                    s.brandReference.Load();
                da.SpareGroupCreate(GroupID, s.brand.name);
            }
            return SpareViewItem;
        }

        // ЗАПЧАСТЬ - ДОБАВИТЬ - СОХРАНИТЬ
        public spare SpareCreate(string Name, string Code, string CodeShatem, int QDemand, int GroupID, int BrandID, int UnitID, string Description)
        {
            DataAccess da = new DataAccess();
            spare sp = new spare();
            sp.name = Name;
            sp.code = Code;
            sp.codeShatem = CodeShatem;
            sp.q_demand = QDemand;
            sp.q_demand_clear = QDemand;
            sp.q_rest = 0;
            sp.description = Description;
            spare s = da.SpareCreate(sp, BrandID, GroupID, UnitID);            
            SpareContainer.Instance.Update(s.id);

            if (SpareContainer.Instance.Spares.Where(i => i.BrandID == BrandID && i.GroupID == GroupID).Count() == 1)
            {
                if (s.brand == null)
                    s.brandReference.Load();
                da.SpareGroupCreate(GroupID, s.brand.name);
            }
            return s;
        }
       
        // ЗАПЧАСТЬ - ДОБАВИТЬ - СОХРАНИТЬ
        public spare SpareCreate(string Name, string Code, string CodeShatem, int QDemand, int GroupID, int BrandID, string UnitName, string Description)
        {
            DataAccess da = new DataAccess();
            spare sp = new spare();
            sp.name = Name;
            sp.code = Code;
            sp.codeShatem = CodeShatem;
            sp.q_demand = QDemand;
            sp.q_demand_clear = QDemand;
            sp.q_rest = 0;
            sp.description = Description;
            spare s = da.SpareCreate(sp, BrandID, GroupID, UnitName);

            SpareContainer.Instance.Update(s.id);

            if (SpareContainer.Instance.Spares.Where(
                    i => i.BrandID == BrandID && i.GroupID == GroupID).Count() == 1)
            {
                da.SpareGroupCreate(GroupID, BrandID);
            }
            return s;
        }

        // ЗАПЧАСТЬ - РЕДАКТИРОВАТЬ
        public void SpareEdit(int SpareID)
        {
            if (!ready())
                return;
            SpareEditView v = new SpareEditView();
            v.ParentWorkspace = mainWindowObj;
            v.LoadItem(SpareID);
            bool? res = v.ShowDialog();
            if (v._spare != null)
            {
                MainWindowObj.LoadSpares();
                MainWindowObj.dgSpares.SelectedItem = SpareContainer.Instance.Spares.FirstOrDefault(x => x.id == SpareID);
            }
        }

        // ЗАПЧАСТЬ - РЕДАКТИРОВАТЬ - СОХРАНИТЬ
        public spare SpareEdit(int SpareID, string Name, string Code, string CodeShatem, int QDemand, int GroupID, int BrandID, int UnitID, string Description)
        {
            DataAccess da = new DataAccess();
            spare sp = da.GetSpare(SpareID);
            sp.name = Name;
            sp.code = Code;
            sp.codeShatem = CodeShatem;
            sp.q_demand = QDemand;
            sp.q_demand_clear = QDemand;
            sp.q_rest = 0;
            sp.description = Description;
            if (sp.brand == null)
                sp.brandReference.Load();
            if (sp.spare_group == null)
                sp.spare_groupReference.Load();

            string OldBrandName = sp.BrandName;
            int OldBrandID = sp.brand.id;
            int OldGroupID = sp.spare_group.id;

            spare s = da.SpareEdit(sp, BrandID, GroupID, UnitID);            
            SpareContainer.Instance.Update(s.id);

            if (OldBrandID != BrandID || OldGroupID != GroupID)
            {
                if (SpareContainer.Instance.Spares.Where(i => i.BrandID == OldBrandID && i.GroupID == OldGroupID).Count() == 0)
                {
                    da.SpareGroupDelete(OldBrandID, OldGroupID);
                    mainWindowObj.LoadGroups(false);
                }
                if (SpareContainer.Instance.Spares.Where(i => i.BrandID == BrandID && i.GroupID == GroupID).Count() == 1)
                {
                    da.SpareGroupCreate(GroupID, BrandID);
                    mainWindowObj.LoadGroups(false);
                }
            }
            return s;
        }
        public spare SpareEditInBackground(int SpareID, string Name, string Code, string CodeShatem, int QDemand, int GroupID, int BrandID, int UnitID, string Description)
        {
            DataAccess da = new DataAccess();
            spare sp = da.GetSpare(SpareID);
            sp.name = Name;
            sp.code = Code;
            sp.codeShatem = CodeShatem;
            sp.q_demand = QDemand;
            sp.q_demand_clear = QDemand;
            sp.q_rest = 0;
            sp.description = Description;
            if (sp.brand == null)
                sp.brandReference.Load();
            if (sp.spare_group == null)
                sp.spare_groupReference.Load();

            string OldBrandName = sp.BrandName;
            int OldBrandID = sp.brand.id;
            int OldGroupID = sp.spare_group.id;

            spare s = da.SpareEdit(sp, BrandID, GroupID, UnitID);            
            return s;
        }
        // ЗАПЧАСТЬ - УДАЛИТЬ
        public void SpareDelete(SpareView item)
        {
            if (!ready())
                return;
            //try
            {
                DataAccess da = new DataAccess();
                int cnt = SpareContainer.Instance.Spares.Where(s => s.BrandID == item.BrandID && s.GroupID == item.GroupID).Count();
                spare sp = da.GetSpare(item.id);
                if (sp.spare_group == null)
                    sp.spare_groupReference.Load();
                if (sp.brand == null)
                    sp.brandReference.Load();
                int BrandID = item.BrandID;
                int GroupID = item.GroupID;
                da.SpareDelete(item.id);
                SpareContainer.Instance.Remove(item.id);

                // проверить, не последняя ли это деталь в брэнде в данной группе
                if (cnt < 2)
                {
                    da.SpareGroupDelete(BrandID, GroupID);
                }
            }

            //catch (Exception ex)
            {
                //    this.Log(ex.Message);
            }
        }
    }
}