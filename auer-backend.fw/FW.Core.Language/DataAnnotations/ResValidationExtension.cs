using FW.Core.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FW.Core.Language.DataAnnotations
{
    public static class ResValidationExtension
    {
        private static LanguageSettings ls;

        /// <summary>
        /// cctor
        /// </summary>
        static ResValidationExtension()
        {
            ls = EngineContext.Current.Resolve<LanguageSettings>();
            if (ls == null)
            {
                ls = EngineContext.Current.Resolve<IResourceLoader>().Load();
            }
        }

        private static ILanguageSelector LanguageSelector
        {
            get
            {
                return EngineContext.Current.Resolve<ILanguageSelector>();
            }
        }

        /// <summary>
        /// 设置验证错误消息
        /// </summary>
        /// <param name="attr"></param>
        /// <param name="catalogName"></param>
        /// <param name="key"></param>
        /// <param name="validationContext"></param>
        public static void SetValidErrorMessage(this ValidationAttribute attr,  string catalogName, string key, ValidationContext validationContext)
        {
            if (!String.IsNullOrWhiteSpace(attr.ErrorMessage))
            {
                return;
            }

            if (String.IsNullOrWhiteSpace(catalogName))
            {
                catalogName =LanguageSelector.GetPackageName(validationContext.ObjectType);
            }
            if (String.IsNullOrWhiteSpace(key))
            {
                key = LanguageSelector.GetLangKey(validationContext.ObjectType, validationContext.MemberName, attr.GetType());
            }

            string cultureName = System.Threading.Thread.CurrentThread.CurrentCulture.Name;

            if (ls != null && ls.ContainsKey(catalogName) && ls[catalogName].ContainsKey(cultureName))
            {
                string errorMsg = ls[catalogName][cultureName][key];
                if (string.IsNullOrWhiteSpace(errorMsg))
                {
                    var langitem = ls[catalogName][cultureName].Where(item => key.StartsWith(item.Key, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                    if (langitem != null)
                    {
                        errorMsg = langitem.Value;
                    }
                }
                if (!String.IsNullOrWhiteSpace(errorMsg))
                {
                    attr.ErrorMessage = errorMsg;
                }
            }
        }

    }
}
