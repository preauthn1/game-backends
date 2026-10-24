using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
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
    public class PerGameDetailLogRepositoryTests : BaseTest
    {

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public PerGameDetailLogAccountingIgnoreRepository PerGameDetailLogAccountingIgnoreRepository { get; set; }

        public BetRecordAGGRepository BetRecordAGGRepository { get; set; }



        [TestMethod]
        public void GetByTransactionNo_WithGameTimeRange_Test()
        {
            var queryAGG = BetRecordAGGRepository.GetPagingByFilter(new QueryBetRecordGeneralRequestViewModel
            {
                PageSize = 5,
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-1 * 14),
                Status = BetRecordStatusEnum.SettleComplete.ToInteger(),
            });

            if(queryAGG.Items.IsNotEmptyNull())
            {
                var match = queryAGG.Items.FirstOrDefault();
                var query = PerGameDetailLogRepository.GetByTransactionNo_WithGameTimeRange(match.BetRecordNoPGS);

                if(query == null)
                {
                    var queryIgnore = PerGameDetailLogAccountingIgnoreRepository.GetByTransactionNo_WithGameTimeRange(match.BetRecordNoPGS);

                    if(queryIgnore == null)
                    {
                        throw new TerproException(ApiErrorCode.NotFoundBetRecord, new ErrorDetailModel($"{match.BetRecordNoPGS}"));
                    }

                    Assert.IsTrue(queryIgnore.TransactionNo == match.BetRecordNoPGS);
                }
                else
                {
                    Assert.IsNotNull(query);
                    Assert.IsTrue(query.TransactionNo == match.BetRecordNoPGS);
                }
            }

            return;
        }



        [TestMethod]
        public void GetByTransactionNo_CreateMany_Test()
        {
            int disableGameID = 330001;
            DateTime myGameTime = DateTimeHelper.GetCurrentTime().AddSeconds(-1);

            var myResult = PerGameDetailLogRepository.CreateTransactionScope(new List<PerGameDetailLogEntity>
            {
                new PerGameDetailLogEntity
                {
                    ServerID = 0,
                    GameID = disableGameID,
                    TransactionNo = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo(),
                    PlayerName = _TEST_DATA.Default.PlayerName,
                    PlayerID = _TEST_DATA.Default.PlayerID,
                    MerchantID = _TEST_DATA.Default.MerchantID,
                    BetAmount = 0,
                    GameTime = myGameTime,
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                },
                new PerGameDetailLogEntity
                {
                    ServerID = 0,
                    GameID = disableGameID,
                    TransactionNo = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo(),
                    PlayerName = _TEST_DATA.Default.PlayerName,
                    PlayerID = _TEST_DATA.Default.PlayerID,
                    MerchantID = _TEST_DATA.Default.MerchantID,
                    BetAmount = 0,
                    GameTime = myGameTime,
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                },
            });

            return;
        }


        [TestMethod]
        public void GetPerGameBySystemTag_V2_Test()
        {
            var begin = DateTimeHelper.GetCurrentTime().AddDays(-3);
            var end = DateTimeHelper.GetCurrentTime();

            var query = PerGameDetailLogRepository.GetPagingByFilter(new QueryDetailLogRequestViewModel
            {
                PageSize = 50,
                GameStartTimeBegin = begin,
                GameStartTimeEnd = end
            });

            var availableData = query.Items
                    .Where(x => x.ServerID > 0 && x.ServerID <= 99999)
                    .ToList();

            if (availableData.IsNotEmptyNull())
            {
                var highestCount = availableData
                    .GroupBy(x => x.ServerID)
                    .Select(x => new

                    {
                        VendorConfigID = x.Key,
                        Count = x.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .FirstOrDefault();

                var myResult = PerGameDetailLogRepository.GetPerGameBySystemTag_V2(SystemTagEnum.BetRecord_AccountingIgnore, begin, end, highestCount.VendorConfigID);
            }

            return;
        }


    }
}

