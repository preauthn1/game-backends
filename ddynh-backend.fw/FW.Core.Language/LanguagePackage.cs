using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Language
{
    /// <summary>
    /// 语言包,管理一组语言项
    /// </summary>
    public class LanguagePackage : List<LanguageItem>
    {
        public LanguagePackage()
            :this("")
        { }

        public LanguagePackage(string name)
        {
            this.Name = name;
        }

        public string Name
        {
            get;
            set;
        }

        public string this[string key]
        {
            get
            {
                var item = this.Where(l => String.Compare(key, l.Key, true) == 0)
                    .FirstOrDefault();
                if (item != null)
                {
                    return item.Value;
                }

                return null;
            }
        }

    }
}
