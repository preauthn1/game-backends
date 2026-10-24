using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using System.Linq;
using target.Models.CoreLibs;
using target.Models.ViewModels;
using target.DA.Repository;
using target.Models.Enums;
using System.Collections.Generic;
using System;
using target.Models;

namespace target.Testing
{
    [TestClass]
    public class AuthServiceTest : BaseTest
    {

        public IGamesService ServiceInstance { get; set; }


        [TestMethod]
        public void ErrorMerchantSecretKey_Test()
        {
            try
            {
                var query = ServiceInstance.GetMerchantEnableGames(new GameInfoRequestModel
                {
                    MerchantID = _TEST_DATA.Default.MerchantID,
                    MerchantSecretKey = "XXXXXXXXXXXQQQQQQQQQQ",
                });

                Assert.Fail("預期不符合");
            }
            catch(Exception ex)
            {
                AssertExpectException(ex, new List<ApiErrorCode>
                {   
                    ApiErrorCode.NotFoundMerchant , 
                    ApiErrorCode.NotFoundMerchantOrWrongApiKey
                });

                return;
            }

            return;
        }


        [TestMethod]
        public void MagicSecretKey_Test()
        {
            var query = ServiceInstance.GetMerchantEnableGames(new GameInfoRequestModel
            {
                MerchantID = _TEST_DATA.Default.MerchantID,
                MerchantSecretKey = SystemConstant.DevEnvOnly_MagicKey,
            });

            Assert_NotEmptyCollection(query);

            Assert.IsTrue(query.Any(z => z.SupportedOrientation.IsNotEmptyTextNotNull()));

            return;
        }


    }
}

