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
    public class EventDrawPrizeServiceTests : BaseTest
    {

        public IEventMultiplePrizeService ServiceInstance { get; set; }


        [TestMethod]
        public void EventDrawPrizeService_GetByPagingFilter_Test()
        {
            var query = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID , new QueryEventDrawPrizeApiRequestParameterModel
            {
                DisplayNameLikeQuery = "MSTest"
            });

            Assert_NotEmptyCollection(query.Items);

            var matchFirst = query.Items.FirstOrDefault();

            //測試單筆查詢
            var queryDetail = ServiceInstance.GetDrawPrizeDetail(_TEST_DATA.Default.UserID, matchFirst.EventDrawPrizeID);

            //測試單更新
            ServiceInstance.UpdateDrawPrizeDetail(_TEST_DATA.Default.UserID , queryDetail);

            return;
        }


        [TestMethod]
        public void EventDrawPrizeService_CloneDrawPrize_Test()
        {
            var result = ServiceInstance.CloneDrawPrize(_TEST_DATA.Default.UserID, new EventDrawPrizeCloneRequestModel
            {
                DisplayName = "MSTest複製_" + DateTimeHelper.GetCurrentTime().ToString("yyyyMMddHHmmss"),
                EventDrawPrizeID = _TEST_DATA.Default.EventDrawPrizeID
            });

            //清除資料
            ServiceInstance.Delete(_TEST_DATA.Default.UserID , result.ResultId);
            return;
        }


    }
}

