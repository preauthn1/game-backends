using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class PlayerFakeCreateRecordRepositoryTests : BaseTest
    {

        public PlayerFakeCreateRecordRepository PlayerFakeCreateRecordRepository { get; set; }



        [TestMethod]
        public void Create_Test()
        {
            var insert = new PlayerFakeCreateRecordEntity
            {
                CreateTime = DateTimeHelper.GetCurrentTime(),
                MerchantID = _TEST_DATA.Default.MerchantID,
                PlayerName = Guid.NewGuid().ToString()
            };

            PlayerFakeCreateRecordRepository.Create(insert);

            var query = PlayerFakeCreateRecordRepository.GetByPlayerNameAndMerchantID(insert.PlayerName, insert.MerchantID);

            Assert.IsTrue(query.PlayerName == insert.PlayerName);
            Assert.IsTrue(query.MerchantID == insert.MerchantID);
            return;
        }


        [TestMethod]
        public void GetByPlayerNameAndMerchantID_Test()
        {
            var query = PlayerFakeCreateRecordRepository.GetPagingByFilter(new QueryPlayerFakeCreateRecordRequestViewModel
            {
                PageSize = 10
            });

            if(query.Items.IsNotEmptyNull())
            {
                var matchFirst = query.Items.OrderByRandom().FirstOrDefault();

                var querySingleOne =  PlayerFakeCreateRecordRepository.GetByPlayerNameAndMerchantID(matchFirst.PlayerName, matchFirst.MerchantID);

                Assert.IsTrue(querySingleOne.PlayerName == matchFirst.PlayerName);
                Assert.IsTrue(querySingleOne.MerchantID == matchFirst.MerchantID);
            }

            return;
        }

    }
}

