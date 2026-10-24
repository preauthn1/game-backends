using System;
using example.invalid;
using System.Web;

namespace FW.Core.Helper
{
    public static class WebWorkContext
    {
        /// <summary>
        /// 重启应用程序
        /// </summary>
        /// <param name="redirectUrl"></param>
        public static void RestartAppDomain(string redirectUrl = "")
        {
            if (GetTrustLevel() > AspNetHostingPermissionLevel.Medium)
            {
                //full trust
                HttpRuntime.UnloadAppDomain();

                TryWriteGlobalAsax();
            }
            else
            {
                //medium trust
                bool success = TryWriteWebConfig();
                if (!success)
                {
                    throw new Exception("应用程序通过修改web.config方式重启失败");
                }

                success = TryWriteGlobalAsax();
                if (!success)
                {
                    throw new Exception("应用程序通过修改global.asax方式重启失败");
                }
            }

            if (!String.IsNullOrEmpty(redirectUrl))
            {
                HttpContext.Current.Response.Redirect(redirectUrl, true);
            }
        }

        /// <summary>
        /// 尝试修改web.Config
        /// </summary>
        /// <returns></returns>
        public static bool TryWriteWebConfig()
        {
            try
            {
                File.SetLastWriteTimeUtc(HttpContext.Current.Server.MapPath("~/web.config"), DateTime.UtcNow);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 尝试修改glabal.asax
        /// </summary>
        /// <returns></returns>
        public static bool TryWriteGlobalAsax()
        {
            try
            {
                File.SetLastWriteTimeUtc(HttpContext.Current.Server.MapPath("~/global.asax"), DateTime.UtcNow);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取应用程序信任级别
        /// </summary>
        /// <returns></returns>
        public static AspNetHostingPermissionLevel GetTrustLevel()
        {
            try
            {
                new AspNetHostingPermission(AspNetHostingPermissionLevel.Unrestricted).Demand();

                return AspNetHostingPermissionLevel.Unrestricted;
            }
            catch { }

            try
            {
                new AspNetHostingPermission(AspNetHostingPermissionLevel.High).Demand();

                return AspNetHostingPermissionLevel.High;
            }
            catch { }

            try
            {
                new AspNetHostingPermission(AspNetHostingPermissionLevel.Medium).Demand();

                return AspNetHostingPermissionLevel.Medium;
            }
            catch { }

            try
            {
                new AspNetHostingPermission(AspNetHostingPermissionLevel.Low).Demand();

                return AspNetHostingPermissionLevel.Low;
            }
            catch { }

            try
            {
                new AspNetHostingPermission(AspNetHostingPermissionLevel.Minimal).Demand();

                return AspNetHostingPermissionLevel.Minimal;
            }
            catch { }

            return AspNetHostingPermissionLevel.None;
        }


        /// <summary>
        /// 是否爬虫客户端
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool IsSearchEngine(HttpContextBase context)
        {
           
            if (context == null)
                return false;

            bool result = false;
            try
            {
                result = context.Request.Browser.Crawler;
            }
            catch {}

            return result;
        }

    }
}
