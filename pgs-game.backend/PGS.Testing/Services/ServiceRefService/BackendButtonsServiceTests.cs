using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class BackendButtonsServiceTests : BaseTest
    {

        public IBackendButtonsService ServiceInstance { get; set; }


        [TestMethod]
        public void GetDefineButtons_Test()
        {
            var query = ServiceInstance.GetDefineButtons(_TEST_DATA.Default.UserID);
            Assert_NotEmptyCollection(query.Items);

            var checkPlayerMoneyButton = query.Items
                .First(x => x.BackendButtonActionID == BackendButtonEnum.CheckPlayerMoney.ToInteger());

            CollectionAssert.AreEqual(new List<string> { "playerID" }, checkPlayerMoneyButton.RequiredInput);
            return;
        }


        [TestMethod]
        public void ExecuteDefineAction_ResetMerchantPlayerSessionToken_Test()
        {
            var query = ServiceInstance.ExecuteDefineAction(_TEST_DATA.Default.UserID, new BackendDefineButtonActionRequestModel
            {
                BackendButtonActionID = BackendButtonEnum.ResetMerchantPlayerSessionToken.ToInteger(),
                ExtraParameter = new BackendDefineButtonActionExtraParameterModel
                {
                    MerchantID = _TEST_DATA.NotSuperMerchant.MerchantID,
                }
            });

            return;
        }
    }
}

