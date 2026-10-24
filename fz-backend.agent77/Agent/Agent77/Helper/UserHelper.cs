using Newtonsoft.Json;
using Repository.Models;
using Repository.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using static Repository.ViewModel.UserLoginInfo;

namespace Agent77.Helper
{
    public static class UserHelper
    {
        private static UserLoginInfo _user { get; set; }

        public static UserLoginInfo User
        {
            get
            {
                var identity = HttpContext.Current.User.Identity as FormsIdentity;

                if(identity ==  null)
                    return null;

                if (_user == null)
                    _user = JsonConvert.DeserializeObject<UserLoginInfo>(identity.Ticket.UserData);

                return _user; 
            }
        }

        public static bool IsAdmin
        {
            get
            {                
                return User.Role != RoleType.Agent;
            }
        }

        public static int? AdminID
        {
            get
            {
                if (IsAdmin)
                    return User.UserID;
                else
                    return null;
            }
        }
    }
}