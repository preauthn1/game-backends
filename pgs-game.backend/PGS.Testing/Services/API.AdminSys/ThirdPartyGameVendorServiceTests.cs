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
    public class ThirdPartyGameVendorServiceTests : BaseTest
    {

        public IThirdPartyGameVendorService ServiceInstance { get; set; }


        public BetRecordAGGLiveRepository BetRecordAGGLiveRepository { get; set; }


        [TestMethod]
        public void AGGLive_GetDetailUrlByTransactionNo_Test()
        {
            var query = BetRecordAGGLiveRepository.GetPagingByFilter(new QueryBetRecordGeneralRequestViewModel
            {
                OrderByColumn = "CreateTime",
                CreateTimeEnd = DateTimeHelper.GetCurrentTime().AddMinutes(-30),
                Status = (int)BetRecordStatusEnum.SettleComplete.ToInteger(), 
                PageSize = 1
            });

            if(query.Items.IsNotEmptyNull())
            {
                var matchFisrt = query.Items.FirstOrDefault();
                var urlResult = ServiceInstance.GetDetailUrlByTransactionNo(_TEST_DATA.Default.UserID , matchFisrt.BetRecordNoPGS);

                Assert.IsTrue(urlResult.Url.IsNotEmptyTextNotNull());
            }

            return;
        }

    }
}

