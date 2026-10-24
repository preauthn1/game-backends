using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FW.Core.Helper;
using game.Entity.AdminSys;

namespace game.AdminSystem
{
    public class AdminUserViewModel
    {

        public int UserID { get; set; }

        public string LoginName { get; set; }

        public string DisplayName { get; set; }

        public string OrganizationName { get; set; }


    }
}