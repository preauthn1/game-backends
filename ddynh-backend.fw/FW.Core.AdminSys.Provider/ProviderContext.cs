using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.AdminSys.Provider
{
    public static class ProviderContext
    {
        private static IAdminSysProvider _adminSysProvider;

        static ProviderContext()
        {
           var config =  System.Configuration.ConfigurationManager.GetSection("adminSysConfig") as AdminSysConfig;
           _adminSysProvider = Activator.CreateInstance(config.AdminSysProviderType, new []{ config.NameOrConnectionString}) as IAdminSysProvider;
        }

        public static IAdminSysProvider AdminSysProvider
        {
            get
            {
                return _adminSysProvider;
            }
        }
    }
}
