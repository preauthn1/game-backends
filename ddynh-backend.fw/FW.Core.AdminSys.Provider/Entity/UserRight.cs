using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.AdminSys.Provider.Entity
{
    public class UserRight
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

        public int RightID
        {
            get;
            set;
        }

        public virtual AdminUser User
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
