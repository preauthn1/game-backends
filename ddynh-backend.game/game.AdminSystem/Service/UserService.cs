using FW.Core.AdminSys.WebUI.Themes.Bootstrap;
using game.Entity.Users;
using System.Web;
using System.Web.Mvc;

namespace game.AdminSystem
{
    public static class UserService
    {


        /// <summary>
        /// 获取用户
        /// </summary>
        public static UserInfo GetUser(int? userID, string loginName, string nickName)
        {
            if (userID.HasValue)
            {
                return GetUserByID(userID.Value);
            }
            else if (string.IsNullOrEmpty(loginName) == false)
            {
                return SiteHelper.GetAPIResult<UserInfo>("/User/GetUserByLoginName?loginName=" + HttpContext.Current.Server.UrlEncode(loginName));
            }
            else if (string.IsNullOrEmpty(nickName) == false)
            {
                return SiteHelper.GetAPIResult<UserInfo>("/User/GetUserByNickName?nickName=" + HttpContext.Current.Server.UrlEncode(nickName));
            }
            return null;
        }

        /// <summary>
        /// 获取用户ID
        /// </summary>
        public static int? GetUserID(int? userID, string loginName, string nickName)
        {
            var user = UserService.GetUser(userID, loginName, nickName);
            if (user == null)
                if (userID == null && string.IsNullOrEmpty(loginName) && string.IsNullOrEmpty(nickName))
                {
                    return null;
                }
                else
                {
                    return 0;
                }
            else
                return user.Id;
        }

        public static UserInfo GetUserByID(int userID)
        {
            return SiteHelper.GetAPIResult<UserInfo>("/User/GetUserByID?userID=" + userID.ToString());
        }


        public static bool CheckToken(string action, string method)
        {
            var routeData = HttpContext.Current.Request.RequestContext.RouteData;

            var area = string.Empty;
            var controller = string.Empty;

            if (routeData.Values.ContainsKey("area"))
            {
                area = routeData.Values["area"].ToString();
            }

            if (routeData.Values.ContainsKey("controller"))
            {
                controller = routeData.Values["controller"].ToString();
            }

            if (string.IsNullOrWhiteSpace(action))
            {
                if (routeData.Values.ContainsKey("action"))
                {
                    action = routeData.Values["action"].ToString();
                }
            }

            if (string.IsNullOrWhiteSpace(method))
            {
                method = "Get";
            }

            string token = [REDACTED]"{0}_{1}_{2}_{3}", area, controller, action, method);

            return BootstrapHtmlExtensions.CheckRightToken(token); ;
        }

    }
}