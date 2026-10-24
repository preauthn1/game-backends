using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.Models.Attributes;
using target.Models.Enums;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class EnumsValueCheckTests : BaseTest
    {

        
        /// <summary>
        /// 檢查enum的編號是否合乎規則 (有些會限制必須是六位數 之類的)
        /// </summary>
        [TestMethod]
        public void CheckEnumValueRangeIsValid()
        {
            CheckIsValidValueRange<CustomerColumnSpecialQueryEnum>(900000, 999999);
        }

        public void CheckIsValidValueRange_SpecialMerchantFilterType()
        {
            CheckIsValidValueRange<SpecialMerchantFilterType>(0, 9999);
        }


        private void CheckIsValidValueRange<T>(int min , int max) where T: System.Enum
        {
            var allEnums = ExtensionOfEnum.GetEnumAllValue<T>();

            string enumName = typeof(T).Name;

            foreach(System.Enum enumValue in allEnums)
            {
                int myValue = enumValue.GetHashCode();
                bool isValidValueRange = myValue >= min && myValue < max;

                if (!isValidValueRange)
                {
                    Assert.Fail($"{enumName} 驗證不合法。不合法的編號為:{myValue} {enumValue.ToString()}");
                }
            }

        }


    }



}
