using Newtonsoft.Json;
using Repository.ViewModel;
using Service.Modules.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using static Repository.ViewModel.UserLoginInfo;

namespace Service.Helper
{
    public static class UserHelper
    {      
        public static UserLoginInfo User
        {
            get
            {
                var identity = HttpContext.Current.User.Identity as FormsIdentity;

                if (identity == null)
                    return null;
               
                return JsonConvert.DeserializeObject<UserLoginInfo>(identity.Ticket.UserData);              
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

        public static List<PlatformArea> PlatformArea
        {
            get
            {
                string key = string.Concat("platformArea_", User.UserID);
                ICacheProvider Cache = new DefaultCacheProvider();
                var platformArea = Cache.Get(key) as List<PlatformArea>;

                if (platformArea != null)
                    return platformArea;
                else
                    return null;
            }
        }
    }
}
