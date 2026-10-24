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
    public class TransactionRecordServiceTests : BaseTest
    {

        public ITransactionRecordService ServiceInstance { get; set; }


        [TestMethod]
        public void TransactionRecordService_ServiceInstance_Test()
        {
            var query = ServiceInstance.GetPagingByFilter(_TEST_DATA.Default.UserID, new QueryTransactionRecordRequestParameterModel
            {
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime()
            });

            Assert_NotEmptyCollection(query.Items);
            Assert.IsNotNull(new CRUDResultModel());

            if(query.Items.IsNotEmptyNull())
            {
                Assert.IsTrue(query.Items.All(x => x.PlayerID > 0));
                Assert.IsTrue(query.Items.All(x => x.MerchantID > 0));
                Assert.IsTrue(query.Items.All(x => x.MerchantName.IsNotEmptyTextNotNull()));
            }

            return;
        }



        [TestMethod]
        public void TransactionRecordService_QueryByPlayerID_Test()
        {
            int targetPlayerID = _TEST_DATA.Default.PlayerID;

            var query1 = ServiceInstance.GetPagingByFilter(_TEST_DATA.Default.UserID, new QueryTransactionRecordRequestParameterModel
            {
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime(),
                PlayerID = targetPlayerID
            });

            AssertExpectExceptionProcess
                (() =>
                {
                    var query2 = ServiceInstance.GetPagingByFilter(_TEST_DATA.NotSuperMerchant.UserID, new QueryTransactionRecordRequestParameterModel
                    {
                        CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                        CreateTimeEnd = DateTimeHelper.GetCurrentTime(),
                        PlayerID = targetPlayerID
                    });
                }
                , ApiErrorCode.Forbidden_WatchDataPermissionNotAllow 
                , ApiErrorCode.NotFoundPlayer_IsThisAccountOrPlayerID
            );

            return;
        }



        [TestMethod]
        public void TransactionRecordService_NotSuperAdmin_Test()
        {
            int targetPlayerID = _TEST_DATA.Default.PlayerID;


            int userID = _TEST_DATA.NotSuperMerchant.UserID;

            var query1 = ServiceInstance.GetPagingByFilter(_TEST_DATA.NotSuperMerchant.UserID, new QueryTransactionRecordRequestParameterModel
            {
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime(),
            });

            AssertExpectExceptionProcess
                (() =>
                {
                    var query2 = ServiceInstance.GetPagingByFilter(_TEST_DATA.NotSuperMerchant.UserID, new QueryTransactionRecordRequestParameterModel
                    {
                        CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                        CreateTimeEnd = DateTimeHelper.GetCurrentTime(),
                        PlayerID = targetPlayerID
                    });
                }
                , ApiErrorCode.Forbidden_WatchDataPermissionNotAllow
                , ApiErrorCode.NotFoundPlayer_IsThisAccountOrPlayerID
            );

            return;
        }


    }
}

