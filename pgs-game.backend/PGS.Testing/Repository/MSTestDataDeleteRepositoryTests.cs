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

    /// <summary>
    /// 主要用來刪除一些MSTest產生 但又清除的資料
    /// </summary>
    [TestClass]
    public class MSTestDataDeleteRepositoryTests : BaseTest
    {

        public MSTestDataDeleteRepository MSTestDataDeleteRepository { get; set; }

        public IUsersService UsersService { get; set; }

        public IMerchantService MerchantService { get; set; }


        [TestMethod]
        public void DeleteIPTemplate_Test()
        {
            MSTestDataDeleteRepository.DeleteIPTemplate();
            return;
        }


        [TestMethod]
        public void Delete_UserAccounts_Test()
        {
            string myKeyword = "dev-demo_18";

            //createTime 不要近日
            var queryUsers = UsersService.GetUsersPagingByFilter(_TEST_DATA.Default.UserID, new QueryUserApiRequestParameterModel
            {
                Keyword = myKeyword,
                PageSize = 20,
                CreateTimeEnd = DateTimeHelper.GetCurrentTime().AddHours(-1 * 48)
            });

            foreach(var eachUser in queryUsers.Items)
            {
                UsersService.Delete(eachUser.UserID);
            }

            return;
        }


        [TestMethod]
        public void Delete_Merchant_Test()
        {
            string myKeyword = "MS_Batch2";
            //string myKeyword = "(測試用) MSTest_17";

            //createTime 不要近日
            var queryMerchant = MerchantService.GetByPagingFilter(new QueryMerchantApiRequestParameterModel 
            {
                Keyword = myKeyword,
                PageSize = 20,
                CreateTimeEnd = DateTimeHelper.GetCurrentTime().AddHours(-1 * 48)
            });

            foreach (var eachItem in queryMerchant.Items)
            {
                MerchantService.Delete(_TEST_DATA.Default.UserID, eachItem.MerchantID);
            }

            return;
        }

    }
}

