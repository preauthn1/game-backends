using FW.Core.Web.Mvc.ThemeableViewEngine;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.AdminSys.Provider
{
    public class AdminSysInfo
    {

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="systemName"></param>
        /// <param name="logo"></param>
        /// <param name="defaultPageName"></param>
        /// <param name="themeName"></param>
        public AdminSysInfo(string systemName, string logo, string defaultPageName, string themeName)
        {
            SystemName = systemName;
            Logo = logo;
            DefaultPageName = defaultPageName;
            ThemeName = themeName;
        }

        /// <summary>
        /// 程序名称
        /// </summary>
        public string SystemName
        {
            get;
            private set;
        }

        /// <summary>
        /// Logo信息
        /// </summary>
        public string Logo
        {
            get;
            private set;
        }

        /// <summary>
        /// 首页地址
        /// </summary>
        public string DefaultPageName
        {
            get;
            private set;
        }

        /// <summary>
        /// 主题
        /// </summary>
        public string ThemeName
        {
            get;
            private set;
        }

        public string DefaultPageHtml
        {
            get
            {
                if (String.IsNullOrWhiteSpace(DefaultPageName))
                {
                    return String.Empty;
                }

                string fileFullPath = System.AppDomain.CurrentDomain.BaseDirectory + DefaultPageName;
                if (File.Exists(fileFullPath))
                {
                    using (var reader = new StreamReader(fileFullPath))
                    {
                        string data = reader.ReadToEnd();
                        reader.Close();
                        reader.Dispose();

                        return data;
                    }
                }

                return String.Empty;
            }
        }

    }

    public interface ISysInfoProvider : IThemeProvider
    {
        AdminSysInfo LoadSystemInfo();
    }

    public class DefaultSysInfoProvider : ISysInfoProvider
    {
        private AdminSysInfo _config;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="systemName"></param>
        /// <param name="logo"></param>
        /// <param name="defaultPageName"></param>
        /// <param name="themeName"></param>
        public DefaultSysInfoProvider(string systemName = "后台管理系统", string logo = "", string defaultPageName = "index.html", string themeName = "Bootstrap")
        {
            _config = new AdminSysInfo(systemName, logo, defaultPageName, themeName); 
        }

        public AdminSysInfo LoadSystemInfo()
        {
            return _config;
        }

        public string GetThemeName(System.Web.Routing.RequestContext requestCtx)
        {
            return _config.ThemeName;
        }
    }

    public class ConfigSysInfoProvider : ISysInfoProvider
    {

        public AdminSysInfo LoadSystemInfo()
        {
            var appKeys = System.Configuration.ConfigurationManager.AppSettings.AllKeys;

            var defaultSystemInfo = new DefaultSysInfoProvider().LoadSystemInfo();

            string systemName = defaultSystemInfo.SystemName;
            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains("systemName"))
            {
                systemName = System.Configuration.ConfigurationManager.AppSettings["systemName"].ToString();
            }

            string logo = defaultSystemInfo.Logo;
            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains("logo"))
            {
                logo = System.Configuration.ConfigurationManager.AppSettings["logo"].ToString();
            }

            string defaultPageName = defaultSystemInfo.DefaultPageName;
            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains("defaultPageName"))
            {
                defaultPageName = System.Configuration.ConfigurationManager.AppSettings["defaultPageName"].ToString();
            }

            string theme = defaultSystemInfo.ThemeName;
            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains("theme"))
            {
                theme = System.Configuration.ConfigurationManager.AppSettings["theme"].ToString();
            }

            return new AdminSysInfo(systemName, logo, defaultPageName, theme);
        }

        public string GetThemeName(System.Web.Routing.RequestContext requestCtx)
        {
            return LoadSystemInfo().ThemeName;
        }
    }

}
