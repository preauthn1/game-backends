
using Google.Authenticator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.AdminSystem
{
    public static class SiteService
    {
        /// <summary>
        /// 获取当前登录用户的UserID   -AdminSys数据库
        /// </summary>
        /// <returns></returns>
        public static int GetAdminUserID()
        {
            return Convert.ToInt32(FW.Core.Engine.EngineContext.Current.Resolve<FW.Core.Auth.AuthManager>().CurrentUser.UserID);
        }


        public static bool CheckCurrentPageAccess()
        {
            var isAccess = FW.Core.Engine.EngineContext.Current.Resolve<FW.Core.Auth.AuthManager>().CheckAccess();
            return isAccess;
        }


        /// <summary>
        /// 获取当前登录用户代理ID
        /// </summary>
        /// <returns></returns>
        public static int GetAgentID()
        {
            if (HttpContext.Current.Session["Session_CURRENT_AGENTID"] == null)
                return 0;

            return int.Parse(HttpContext.Current.Session["Session_CURRENT_AGENTID"].ToString());
        }

        public static string GetAdminUserName()
        {
            return FW.Core.Engine.EngineContext.Current.Resolve<FW.Core.Auth.AuthManager>().CurrentUser.LoginName;
        }

        /// <summary>
        /// 驗證当前登录用户的金鑰
        /// </summary>
        /// <returns></returns>
        public static bool VerifyAdminUserSecretKey(string txtScretKey)
        {
            var _userKey = FW.Core.Engine.EngineContext.Current.Resolve<FW.Core.Auth.AuthManager>().CurrentUser.SecretKey;
            var tfa = new TwoFactorAuthenticator();
            var result = tfa.ValidateTwoFactorPIN(_userKey.ToString(), txtScretKey, new TimeSpan(0, 0, 45));

            return result;
        }

    }
}