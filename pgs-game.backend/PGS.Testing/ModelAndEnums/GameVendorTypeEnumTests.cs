using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.Models.Attributes;
using target.Models.Enums;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{

    [TestClass]
    public class GameVendorTypeEnumTests : BaseTest
    {

        /// <summary>
        /// 檢查ErrroCode編號是否重複
        /// </summary>
        [TestMethod]
        public void Check_GameVendorTypeEnum_And_ApiConfigCategoryEnumValue()
        {
            string typeName = typeof(ApiErrorCode).Name;
            var allEnums = ExtensionOfEnum.GetEnumAllValue<GameVendorTypeEnum>();

            //檢查是否是6位數字編號
            foreach(GameVendorTypeEnum myEnumValue in allEnums)
            {
                int numberValue = (int)myEnumValue;
                if (numberValue > 100000)
                {
                    var matchCategoryValue = (ApiConfigCategoryEnum)numberValue;

                    if(!matchCategoryValue.IsDefinedValue())
                    {
                        Assert.Fail($"列舉標號{numberValue} , 在ApiConfigCategoryEnum沒有定義。同時也請檢查Controller層ApiConfigCategory 標記是否正確");
                    }
                }                
            }
        }





    }
}
