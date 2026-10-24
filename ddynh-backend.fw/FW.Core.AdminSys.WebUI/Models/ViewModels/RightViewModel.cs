using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FW.Core.AdminSys.Provider.Entity
{
    public class RightViewModel : Right
    {
        public int RoleRightCount { get; set; }

        public int UserRightCount { get; set; }


        public string RoleIDsText { get; set; }

        public string UserIDsText { get; set; }



    }
}