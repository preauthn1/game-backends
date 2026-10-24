using Repository.ViewModel;
using Service.Modules.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Helper
{   
    public static class CacheHelper
    {
        public const string PLAT_FORM_AREA = "platformArea_";
        
        public static List<PlatformArea> GetPlatFormArea(string id)
        {
            string key = string.Concat(PLAT_FORM_AREA, id);

            ICacheProvider Cache = new DefaultCacheProvider();
            var platformArea = Cache.Get(key) as List<PlatformArea>;

            if (!Cache.IsSet(key))
            {
                int tnyAdminType = 1;
                int? intUserID = UserHelper.User.UserID;
                string chvAdminAccount = null;

                if (UserHelper.IsAdmin)
                {
                    tnyAdminType = 2;
                    intUserID = null;
                    chvAdminAccount = UserHelper.User.NickName;
                }

                platformArea = new AdminService(null).GetDbAgentUser(tnyAdminType, intUserID, chvAdminAccount).Item4;              

                Cache.Set(key, platformArea, CacheTime);
            }

            return platformArea;
        }

        public static void SetPlatFormArea(string id, List<PlatformArea> data)
        {
            string key = string.Concat(PLAT_FORM_AREA, id);

            ICacheProvider Cache = new DefaultCacheProvider();
            if (Cache.IsSet(key))
                Cache.Invalidate(key);

            Cache.Set(key, data, CacheHelper.CacheTime);
        }
        public static int CacheTime
        {
            get
            {
                return Convert.ToInt32(System.Web.Configuration.WebConfigurationManager.AppSettings["CacheTime"]);
            }
        }
        
    }
}
