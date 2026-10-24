using FW.Core.Engine;
using FW.Core.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace FW.Core.Web.Mvc
{
    public abstract class LocalizedWebViewPage<TModel> : WebViewPage<TModel>
    {
        private static LanguageSettings _settings;

        /// <summary>
        /// cctor
        /// </summary>
        static LocalizedWebViewPage()
        {
            _settings = EngineContext.Current.Resolve<LanguageSettings>();
            if (_settings == null)
            {
                _settings = EngineContext.Current.Resolve<IResourceLoader>().Load();
            }
        }

        public virtual string Localize(string key)
        {
            return Localize("WebUI", key);
        }

        public virtual string Localize(string category,string key)
        {
            string culture = System.Threading.Thread.CurrentThread.CurrentCulture.Name;

            return _settings[category][culture][key];
        }

        
    }

    public abstract class LocalizedWebViewPage : LocalizedWebViewPage<dynamic>
    { }
}
