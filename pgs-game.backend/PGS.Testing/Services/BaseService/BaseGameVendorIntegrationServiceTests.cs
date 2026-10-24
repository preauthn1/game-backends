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
using target.BL.Services;
using target.Models.Table;

namespace target.Testing
{
    [TestClass]
    public class BaseGameVendorIntegrationServiceTests : BaseTest
    {


        public BetRecordAGGRepository BetRecordAGGRepository { get; set; }



        [TestMethod]
        public void BetRecordConvertToPerGame_Test()
        {
            var BaseGameVendorIntegrationService = new BaseGameVendorIntegrationService();

            var query = BetRecordAGGRepository.GetPagingByFilter(new QueryBetRecordGeneralRequestViewModel
            {
                Status = BetRecordStatusEnum.SettleComplete.ToInteger(),
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-7),
                PageSize = 5
            });

            if (query.Items.IsNotEmptyNull())
            {
                var matchFirst = query.Items.FirstOrDefault();

                var mySettleTime = DateTimeHelper.GetCurrentTime().AddDays(-2);

                var record = matchFirst.ReturnInjecteObject<BetRecordAGGEntity>();
                record.ThirdSettleTime = DateTimeHelper.GetCurrentTime().AddDays(-2);

                var perGame = BaseGameVendorIntegrationService.BetRecordConvertToPerGame(record);

                //之前把code改壞了 加測試吧
                Assert.IsTrue(perGame.ThirdSettleDate == mySettleTime.Date);
                Assert.IsTrue(perGame.GameTime == record.SettlementTime);

                var myTags = CommonLogicHelper.CheckAnyTagToAdd(perGame);

                Assert.IsTrue(myTags.Any(x => x == SystemTagEnum.PerGameDetailLog_ThirdSettleDateNotEqual));
            }


            return;
        }



    }
}

