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
    public class SingleWalletMerchantApiRequestLogServiceTests : BaseTest
    {

        public ISingleWalletMerchantApiRequestLogService ServiceInstance { get; set; }


        [TestMethod]
        public void SingleWalletMerchantApiRequestLogService_GetByPagingFilter_Test()
        {
            int daysRange = -1 * 30;

            var query1 = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID , new QuerySingleWalletMerchantApiRequestLogApiRequestParameterModel
            {
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(daysRange),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime(),
                PageSize = 5
            });

            var query2 = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID, new QuerySingleWalletMerchantApiRequestLogApiRequestParameterModel
            {
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(daysRange),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime(),
                TopSize = 200
            });

            return;
        }

    }
}

