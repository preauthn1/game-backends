using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using target.BL.Interface;
using target.BL.Services;
using target.Models.Paging;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class AdminSysComplexServiceCheckPlayerMoneyTests : BaseTest
    {
        [TestMethod]
        public void CheckPlayerMoney_Tests()
        {
            int userID = 1;
            var beginTime = new DateTime(2026, 8, 1);
            var endTime = new DateTime(2026, 8, 2);
            var perGameDetailService = Substitute.For<IPerGameDetailService>();
            var transactionRecordService = Substitute.For<ITransactionRecordService>();

            perGameDetailService.GetByPagingFilter(userID, Arg.Any<QueryPerGameDetailLogApiRequestParameterModel>())
                .Returns(new PagedList<PerGameDetailViewModel>
                {
                    Items = new List<PerGameDetailViewModel>
                    {
                        new PerGameDetailViewModel { GameID = 1, UpdateAmount = 10 },
                        new PerGameDetailViewModel { GameID = 1, UpdateAmount = -2 },
                        new PerGameDetailViewModel { GameID = 2, UpdateAmount = 5 },
                        new PerGameDetailViewModel { GameID = 0, UpdateAmount = 10 }
                    }
                });

            transactionRecordService.GetPagingByFilter(userID, Arg.Any<QueryTransactionRecordRequestParameterModel>())
                .Returns(new PagedList<TransactionRecordViewModel>
                {
                    TotalPage = 1,
                    Items = new List<TransactionRecordViewModel>
                    {
                        new TransactionRecordViewModel { GameID = 1, UpdateAmount = 8 },
                        new TransactionRecordViewModel { GameID = 3, UpdateAmount = 1 },
                        new TransactionRecordViewModel { GameID = 0, UpdateAmount = 20 }
                    }
                });

            var service = new AdminSysComplexService
            {
                PerGameDetailService = perGameDetailService,
                TransactionRecordService = transactionRecordService
            };

            var result = service.CheckPlayerMoney(userID, new CheckPlayerMoneyRequestModel
            {
                PlayerID = 100,
                BeginTime = beginTime,
                EndTime = endTime
            });

            perGameDetailService.Received(1).GetByPagingFilter(userID, Arg.Is<QueryPerGameDetailLogApiRequestParameterModel>(x =>
                x.PlayerID == 100 &&
                x.GameStartTimeBegin == beginTime &&
                x.GameStartTimeEnd == endTime &&
                x.PageIndex == 1 &&
                x.PageSize == 2000));

            transactionRecordService.Received(1).GetPagingByFilter(userID, Arg.Is<QueryTransactionRecordRequestParameterModel>(x =>
                x.PlayerID == 100 &&
                x.CreateTimeBegin == beginTime &&
                x.CreateTimeEnd == endTime &&
                x.PageIndex == 1 &&
                x.PageSize == 2000));

            Assert.AreEqual(100, result.PlayerID);
            Assert.IsFalse(result.IsAmountMatched);
            Assert.AreEqual(3, result.GameResults.Count);

            var game1Result = result.GameResults.Single(x => x.GameID == 1);
            Assert.AreEqual(8M, game1Result.PerGameAmount);
            Assert.AreEqual(8M, game1Result.TransactionAmount);
            Assert.IsTrue(game1Result.IsAmountMatched);

            Assert.IsFalse(result.GameResults.Single(x => x.GameID == 2).IsAmountMatched);
            Assert.IsFalse(result.GameResults.Single(x => x.GameID == 3).IsAmountMatched);
            Assert.IsFalse(result.GameResults.Any(x => x.GameID == 0));
        }
    }
}
