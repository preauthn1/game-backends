using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.BL.Interface;
using target.Models.Enums;
using target.Models.Models;
using System.Linq;
using target.DA.Repository;
using System.Collections.Concurrent;
using target.Utility.Helper;
using target.Utility;

namespace target.BL.Services
{

    public class BetRecordRecoverService : BaseService, IBetRecordRecoverService
    {

        public PlayerRepository PlayerRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public BetRecordTransactionNoMappingRepository BetRecordTransactionNoMappingRepository { get; set; }

        public BetRecordExtraInfoRepository BetRecordExtraInfoRepository { get; set; }

        public BetRecordTagMappingRepository BetRecordTagMappingRepository { get; set; }



        public void RecoverBetRecordAndInsertData(RecoverBetRecordRequiredInfo item)
        {
            item.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();
            item.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();
            item.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            item.RuleFor(x => x.BetRecordNo).Rule_NotEmptyText();
            item.RuleFor(x => x.BetRecordNoPGS).Rule_NotEmptyText();

            int gameID = item.GameID;
            int gameVendor = AppHelper.GetGameVendorByGameID(gameID);
            var queryPlayer = PlayerRepository.GetBriefByCache(item.PlayerID);
            decimal convertRate = 1.0M;

            var vendorService = ServiceInstanceHelper.InstanceVendorService(gameVendor);
            var recoverTranNo = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(item.BetRecordNoPGS);

            var queryGame = GameListRepository.GetSingleByCache(gameID);

            var requiredInfo = new InsertRawBetRecordRequiredInfoModel
            {
                GameID = gameID,
                ThirdGameCode = queryGame.ThirdPartyGameCode,
                MerchantID = queryPlayer.MerchantID,
                PlayerID = queryPlayer.PlayerID,
                PlayerName = queryPlayer.PlayerName,
                BetRecordNoPGS = item.BetRecordNoPGS,
                BetRecordNo = item.BetRecordNo,
                BetAmount = item.BetAmount,
                BetGamePoint = item.BetAmount * convertRate, //偷懶
                ConvertRate = convertRate,
                VendorConfigID = item.VendorConfigID,
                AmountBeforeBet = -1,
                PartitionTime = recoverTranNo.Time.Value
            };

            var insertEntity = new BetRecordExtraInfoEntity
            {
                BetRecordNoPGS = requiredInfo.BetRecordNoPGS,
                ExtraInfo = "",
                IPAddress = ""
            };

            // step 3. 寫入一筆單號對應Mapping
            if(!item.IsSkipInsertTransactionNoMapping)
            {
                BetRecordTransactionNoMappingRepository.Create(item.BetRecordNoPGS, item.BetRecordNo, gameVendor);
            }

            // step 4. 寫入一筆投注紀錄 (raw betRecord)
            vendorService.WriteRawBetRecordWhenPlaceBet(requiredInfo, new EachGameVendorRawRequestInfoModel());

            // step 5. 額外的debug資訊紀錄在另張table (清除佔空間的資料也比較容易)
            BetRecordExtraInfoRepository.CreateAsync(insertEntity, false);

            // step 6. 標記額外tag
            BetRecordTagMappingRepository.Create(new BetRecordTagMappingEntity
            {
                BetRecordNoPGS = requiredInfo.BetRecordNoPGS,
                CreateTime = DateTimeHelper.GetCurrentTime(),
                TagID = SystemTagEnum.BetRecord_PlaceBet_RecoverFailInsert_Record.ToInteger()
            });
        }


    }
}
