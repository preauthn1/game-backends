using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class PlayerGameSummaryRepositoryTests : BaseTest
    {

        public PlayerGameSummaryRepository PlayerGameSummaryRepository { get; set; }


        [TestMethod]
        public void PlayerGameSummaryValuePlus_Test()
        {
            int playerID = _TEST_DATA.Default.PlayerID;
            int gameID = _TEST_DATA.Default.GameID_AGG;
            decimal myAmount = RandomHelper.GetRandIntegerNumbers(100, 200);

            PlayerGameSummaryRepository.DeleteByModel(new
            {
                PlayerID = playerID,
                GameID = gameID
            });

            var query1 = PlayerGameSummaryRepository.GetByPlayerAndGameID(playerID, gameID);

            Assert.IsTrue(query1 == null);

            PlayerGameSummaryRepository.PlayerGameSummaryValuePlus(new PlayerGameSummaryUpdateParameter
            {
                PlayerID = playerID,
                GameID = gameID,
                BetAmount = myAmount,
                WinLossAmount = myAmount,
                PayoutAmount = myAmount
            });

            var query2 = PlayerGameSummaryRepository.GetByPlayerAndGameID(playerID, gameID);
            Assert.IsTrue(query2.BetAmount == myAmount);


            PlayerGameSummaryRepository.PlayerGameSummaryValuePlus(new PlayerGameSummaryUpdateParameter
            {
                PlayerID = playerID,
                GameID = gameID,
                BetAmount = myAmount,
                WinLossAmount = myAmount,
                PayoutAmount = myAmount
            });

            var query3 = PlayerGameSummaryRepository.GetByPlayerAndGameID(playerID, gameID);
            Assert.IsTrue(query3.BetAmount == myAmount * 2);

            return;
        }

    }
}

