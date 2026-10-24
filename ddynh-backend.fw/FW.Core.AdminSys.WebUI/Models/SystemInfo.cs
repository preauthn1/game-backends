using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FW.Core.AdminSys.WebUI.Models
{
    public static class SystemInfo
    {
        /// <summary>
        /// 程序名称
        /// </summary>
        public static string ProgramName
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["programName"].ToString();
            }
        }



    }
}