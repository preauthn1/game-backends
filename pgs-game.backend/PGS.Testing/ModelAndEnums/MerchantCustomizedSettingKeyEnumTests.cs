using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.Models.Attributes;
using target.Models.Enums;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class MerchantCustomizedSettingKeyEnumTests : BaseTest
    {

        /// <summary>
        /// 檢查遊戲商是否有定義convertRate
        /// </summary>
        [TestMethod]
        public void CheckGameVendorConvertRate_Test()
        {
            var allVendors = ExtensionOfEnum.GetEnumAllValue<GameVendorTypeEnum>();

            foreach(GameVendorTypeEnum vendor in allVendors)
            {
                //三方的廠商才檢查
                if(vendor.ToInteger() > 9999)
                {
                    //對應的enum
                    var debug = MerchantCustomizedSettingKeyEnum.GameVendor_ConvertRate_AGG;
                    var convertRate = AppHelper.GetConvertRateCustomizedSettingKey((int)vendor);

                    if(!convertRate.IsDefine)
                    {
                        string msg = $"遊戲商:{vendor.ToInteger()} 沒定義converRate";
                        throw new TerproException(ApiErrorCode.Forbidden_NotValidDefineValue , msg);
                    }
                }
            }

            return;
        }



    }
}
