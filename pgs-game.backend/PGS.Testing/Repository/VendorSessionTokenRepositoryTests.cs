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
    public class VendorSessionTokenRepositoryTests : BaseTest
    {

        public VendorSessionTokenRepository VendorSessionTokenRepository { get; set; }

        [TestMethod]
        public void GetPlayerLatestToken_Test()
        {
            int playerID = _TEST_DATA.Default.PlayerID;

            var query1 = VendorSessionTokenRepository.GetPagingByFilter(new QueryVendorSessionTokenRequestViewModel
                {
                    PlayerIDs = playerID.ToListCollection(),
                    PageSize = 5
                })
                .Items
                .OrderByDescending(x => x.Id)
                .FirstOrDefault();

            var query2 = VendorSessionTokenRepository.GetPlayerLatestToken(playerID);

            if(query1 != null && query2 != null)
            {
                Assert.IsTrue(query1.Id == query2.Id);
            }

            return;
        }



        [TestMethod]
        public void ResetAllPlayerSessionToken_Test()
        {
            VendorSessionTokenRepository.ResetPlayerAllSessionToken(_TEST_DATA.Default.PlayerID);
            return;
        }

    }
}

