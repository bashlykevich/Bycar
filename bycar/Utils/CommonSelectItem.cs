using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bycar.Utils
{
    public interface CommonSelectItem
    {
        int _Id
        {
            get;
            set;
        }
        string _Name
        {
            get;
            set;
        }
        string __Description
        {
            get;
            set;
        }    
    }
}
