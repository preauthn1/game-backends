using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Language
{
    /// <summary>
    /// 语言包集合，管理一组语言包资源
    /// </summary>
    public class LanguageSettings : Dictionary<string, LanguageResource>
    {
        public LanguageSettings()
            : base(StringComparer.CurrentCultureIgnoreCase)
        { }
    }
}
