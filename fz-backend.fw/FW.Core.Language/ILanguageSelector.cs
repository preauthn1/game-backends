using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Language
{
    /// <summary>
    /// 语言选择器
    /// </summary>
    public interface ILanguageSelector
    {
        /// <summary>
        /// 获取语言包
        /// </summary>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        string GetPackageName(Type sourceType);

        /// <summary>
        /// 获取语言键
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="propertyName"></param>
        /// <param name="validatorType"></param>
        /// <returns></returns>
        string GetLangKey(Type sourceType, string propertyName, Type validatorType);
    }

    /// <summary>
    /// 默认的语言包选择器
    /// </summary>
    public class DefaultLanguageSelector : ILanguageSelector
    {

        public string GetPackageName(Type sourceType)
        {
            return sourceType.Assembly.ManifestModule.Name.Replace(".dll", "");
        }

        public string GetLangKey(Type sourceType, string propertyName, Type validatorType)
        {
            return String.Format("{0}_{1}_{2}", sourceType.FullName.Replace(GetPackageName(sourceType) + ".",""), propertyName, validatorType.BaseType.Name.Replace("Attribute", "")).ToUpper();
        }
    }
}
