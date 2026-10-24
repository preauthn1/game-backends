using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Language
{
    public class CachedFileResourceLoader : IResourceLoader
    {
        private IResourceLoader _innerLoader;
        private LanguageSettings _languageSettings;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="innerLoader"></param>
        public CachedFileResourceLoader(IResourceLoader innerLoader)
        {
            _innerLoader = innerLoader;

            _languageSettings =  _innerLoader.Load();
        }

        public LanguageSettings Load()
        {
            return _languageSettings;
        }
    }
}
