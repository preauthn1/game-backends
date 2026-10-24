using Lck.Utility.Extensions.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.BL.Services;
using target.Core.Helper;
using target.DA.Repository;
using target.Models;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.GameVendor;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.IOC;
using target.Utility.Utility;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace target.Testing
{
    [TestClass]
    public class GameDataCheckTests : BaseTest
    {

        public GameListRepository GameListRepository { get; set; }


        [TestMethod]
        public void GameID_And_GameVendor_Validate()
        {
            if (IsSkipTest_WhenSmallerThanTargetTime(_TEST_DATA.Default.Date_NewGameVendor_Intergrate))
                return;

            var queryGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                IsNotDelete = true
            });

            foreach(var item in queryGames)
            {
                if(!AppHelper.IsPgsSelfGame(item.GameID))
                {
                    //驗證gameID的編號必須符合 跟廠商Id位數一致的原則、請務必遵守 不然部分程式碼邏輯會掛掉
                    Assert.IsTrue(item.GameVendorType == AppHelper.GetGameVendorByGameID(item.GameID));
                }
            }

            return;
        }

    }

}
