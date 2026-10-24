using System;
using example.invalid;
using example.invalid;
using System.Text;
using System.Web;

namespace game.Infrastructure
{
    public static class AppHelper
    {

        /// <summary>
        /// 取得完整請求url
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentRequestFullUrl()
        {          
            try
            {
                var request = System.Web.HttpContext.Current?.Request;

                if (request == null)
                    return string.Empty;

                return request.Url.AbsoluteUri;
            }
            catch
            {

            }

            return string.Empty;
        }

        /// <summary>
        /// windows作業系統 登入者的電腦名稱
        /// </summary>
        /// <returns></returns>
        public static string GetWindowsComputerUserName()
        {
            try
            {
                return System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            }
            catch(Exception ex)
            {
            }

            return string.Empty;
        }





        public static string GetCurrentRequestFullUrlDebugInfo()
        {
            string url = GetCurrentRequestFullUrl();
            return $" 當前請求網址：{url} ";
        }




    }
}
