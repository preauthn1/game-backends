using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Language
{
    /// <summary>
    /// 语言资源，管理一组语言包
    /// </summary>
    public class LanguageResource : Dictionary<string, LanguagePackage>
    {
        public LanguageResource()
            : base(StringComparer.CurrentCultureIgnoreCase)
        { }

        public string Name
        {
            get;
            set;
        }


    }
}
