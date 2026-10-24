using FW.Core.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Language
{
    public static class LanguageExtension
    {
        private static ILanguageSelector LanguageSelector
        {
            get
            {
                return EngineContext.Current.Resolve<ILanguageSelector>();
            }
        }

        public static string GetLang(this LanguageSettings ls, Type serviceType, string key)
        {
            string packageName = LanguageSelector.GetPackageName(serviceType);

            if (ls.ContainsKey(packageName))
            {
                var lp = ls[packageName];
                string cultureName = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
                if (lp.ContainsKey(cultureName))
                {
                    return lp[cultureName][key];
                }
            }

            return "";
        }

        public static string GetLang(this LanguageSettings ls,string key)
        {
            var serviceType = new StackTrace().GetFrames()[1].GetMethod().DeclaringType;
            return GetLang(ls, serviceType, key);
        }
    }
}
