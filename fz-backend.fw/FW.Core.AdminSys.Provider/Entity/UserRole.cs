using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.AdminSys.Provider.Entity
{
    public class UserRole 
    {
        public int Id
        {
            get;
            set;
        }

        public int UserID
        {
            get;
            set;
        }

        public int RoleID
        {
            get;
            set;
        }

        public virtual AdminUser User
        {
            get;
            set;
        }

        public virtual Role Role
        {
            get;
            set;
        }

    }
}
