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

namespace target.Testing
{
    [TestClass]
    public class UserOperateRecordServiceTests : BaseTest
    {

        public IUserOperateRecordService ServiceInstance { get; set; }



        [TestMethod]
        public void UserOperateRecordService_GetByPagingFilter_Test()
        {
            var filter = new QueryUserOperateRecordApiRequestParameterModel
            {
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-1 * 30),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime()
            };

            var query = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID, filter);
            Assert_NotEmptyCollection(query.Items);

            var query2 = ServiceInstance.GetByPagingFilter(_TEST_DATA.NotSuperMerchant.UserID, filter);
            var query3 = ServiceInstance.GetByPagingFilter(_TEST_DATA.CreditAgent.UserID, filter);

            return;
        }


        [TestMethod]
        public void UserOperateRecordService_GetPlayerLockRecordByPagingFilter_Test()
        {
            var query = ServiceInstance.GetPlayerLockRecordByPagingFilter(_TEST_DATA.Default.UserID , new QueryUserOperateRecordApiRequestParameterModel
            {
                OrderByColumn = "CreateTime",                
            });

            Assert_NotEmptyCollection(query.Items);
            return;
        }
        

        [TestMethod]
        public void UserOperateRecordService_GetPlayerLockRecordByPagingFilter_FilterMerchantID_Test()
        {
            var query = ServiceInstance.GetPlayerLockRecordByPagingFilter(_TEST_DATA.Default.UserID, new QueryUserOperateRecordApiRequestParameterModel
            {
                MerchantID = _TEST_DATA.Default.MerchantID
            });

            Assert_NotEmptyCollection(query.Items);
            return;
        }


    }
}

