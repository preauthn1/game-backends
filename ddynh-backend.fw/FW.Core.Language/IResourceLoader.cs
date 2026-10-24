using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Language
{
    public interface IResourceLoader
    {
        /// <summary>
        /// 加载语言包
        /// </summary>
        /// <returns></returns>
        LanguageSettings Load();
    }

}
