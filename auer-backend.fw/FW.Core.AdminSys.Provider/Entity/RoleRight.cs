using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.AdminSys.Provider.Entity
{
    public class RoleRight 
    {
        public int Id
        {
            get;
            set;
        }

        public int RoleID
        {
            get;
            set;
        }

        public int RightID
        {
            get;
            set;
        }

        public virtual Role Role
        {
            get;
            set;
        }

        public virtual Right Right
        {
            get;
            set;
        }
    }
}
