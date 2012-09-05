using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bycar.Utils;

namespace bycar
{   
    public partial class account : CommonSelectItem
    {

        public int _Id
        {
            get
            {
                return id;
            }
            set
            {                
            }
        }

        public string _Name
        {
            get
            {
                return name;
            }
            set
            {
                
            }
        }

        public string __Description
        {
            get
            {
                return description;
            }
            set
            {                
            }
        }
    }

    public partial class warehouse : CommonSelectItem
    {
        public int _Id
        {
            get
            {
                return id;
            }
            set
            {
                
            }
        }

        public string _Name
        {
            get
            {
                return name;
            }
            set
            {
                
            }
        }

        public string __Description
        {
            get
            {
                return description;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }

    public partial class bank : CommonSelectItem
    {
        public int _Id
        {
            get
            {
                return id;
            }
            set
            {

            }
        }

        public string _Name
        {
            get
            {
                return name;
            }
            set
            {

            }
        }

        public string __Description
        {
            get
            {
                return (this.mfo + ", " + this.address);
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }

    public partial class brand : CommonSelectItem
    {

        public int _Id
        {
            get
            {
                return id;
            }
            set
            {
            }
        }

        public string _Name
        {
            get
            {
                return name;
            }
            set
            {

            }
        }

        public string __Description
        {
            get
            {
                return description;
            }
            set
            {
            }
        }
    }

    public partial class spare_group : CommonSelectItem
    {

        public int _Id
        {
            get
            {
                return id;
            }
            set
            {
            }
        }

        public string _Name
        {
            get
            {
                return name;
            }
            set
            {

            }
        }

        public string __Description
        {
            get
            {
                return description;
            }
            set
            {
            }
        }
    }

    public partial class unit : CommonSelectItem
    {

        public int _Id
        {
            get
            {
                return id;
            }
            set
            {
            }
        }

        public string _Name
        {
            get
            {
                return name;
            }
            set
            {

            }
        }

        public string __Description
        {
            get
            {
                return description;
            }
            set
            {
            }
        }
    }
  
    /*
    public partial class bank_account : CommonSelectItem
    {

        public int _Id
        {
            get
            {
                return id;
            }
            set
            {
            }
        }

        public string _Name
        {
            get
            {
                return BankAccount;
            }
            set
            {

            }
        }

        public string __Description
        {
            get
            {
                //if (bank == null)
                //    if (bankReference != null)
                //        bankReference.Load();
                //if (bank != null)
                //    return bank.name;
                //else
                    return Description;
            }
            set
            {
            }
        }
    }*/

    public partial class BankAccountView : CommonSelectItem
    {

        public int _Id
        {
            get
            {
                return id;
            }
            set
            {
            }
        }

        public string _Name
        {
            get
            {
                return BankAccount;
            }
            set
            {

            }
        }

        public string __Description
        {
            get
            {
                return BankName;                
            }
            set
            {
            }
        }
    }


    public partial class car_producer : CommonSelectItem
    {

        public int _Id
        {
            get
            {
                return id;
            }
            set
            {
            }
        }

        public string _Name
        {
            get
            {
                return name;
            }
            set
            {

            }
        }

        public string __Description
        {
            get
            {
                return descripton;
            }
            set
            {
            }
        }
    }
  
}
