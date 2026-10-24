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
using target.Utility.Helper;
using target.Models;
using target.DA.Repository;
using game.Entity.Game;
using target.Utility;
using Lck.Utility.Extensions.Helper;
using target.Utility.Extensions.Web;
using target.Utility.Utility.ThirdParty;
using Microsoft.AspNetCore.Mvc;

namespace target.BL.Services
{

    public class PlayerSuspiciousCheckService : BaseAdminSysService, IPlayerSuspiciousCheckService
    {

        public PlayerRepository PlayerRepository { get; set; }
     
        public ServerMerchantMappingRepository ServerMerchantMappingRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public MerchantGameSummaryDataRepository MerchantGameSummaryDataRepository { get; set; }
      
        public PlayerGameDataRepository PlayerGameDataRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public CurrencyRepository CurrencyRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }

        public VendorConfigSettingRepository VendorConfigSettingRepository { get; set; }

        public BetRecordAGGLiveRepository BetRecordAGGLiveRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public IMChannelRepository IMChannelRepository { get; set; }

        public PlayerTagMappingRepository PlayerTagMappingRepository { get; set; }



        public PlayerSuspiciousCheckResultViewModel CheckPlayerDataIsSuspicious(int userID, LazyCommonRequestModel filter)
        {
            filter.RuleFor(x => x.Begin).Rule_NotNullDateTime().Rule_StartTimeMustSmallThanEndTime(filter.End);
            filter.RuleFor(x => x.End).Rule_NotNullDateTime();

            DateTime beginTW = filter.Begin.Value.AddHours(8);
            DateTime beginUTC = filter.Begin.Value;
            DateTime endUTC = filter.End.Value;
            DateTime now = DateTimeHelper.GetCurrentTime();

            if (endUTC > now)
            {
                endUTC = DateTimeHelper.GetCurrentTime(8).AddDays(1).To_00_00_00().AddHours(-8);
            }

            int minDays = 5;
            int searchRangeDays = (int)(endUTC - beginUTC).TotalDays;

            if (HasCustomerServicePermission(userID) && searchRangeDays <= minDays)
            {
                string dateRangeText = $"{beginUTC.AddHours(8).ToString("yyyy-MM-dd")} - {endUTC.AddHours(8).ToString("yyyy-MM-dd")}";
                string msg = $"The search time range must be at least {minDays} days (excluding future time). 您搜寻的时间区间至少要拉{minDays}天以上 (不含未來時間)。时间太短数据不够精准 ({dateRangeText})";
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, msg);
            }

            int targetPlayerID = GetTargetPlayerIDByParameter(userID, filter);

            var queryPlayer = PlayerRepository.GetSingleByCache(targetPlayerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            var queryMerchant = MerchantRepository.GetSingleByCache(queryPlayer.MerchantID);

            CheckViewMerchantPermission(userID, queryPlayer.MerchantID);

            var result = new PlayerSuspiciousCheckResultViewModel
            {
                PlayerID = queryPlayer.PlayerID,
                PlayerName = queryPlayer.PlayerName,
                MerchantID = queryMerchant.MerchantID,
                MerchantName = queryMerchant.Name,
                Currency = queryMerchant.Currency,
                IsSingleWallet = AppHelper.IsSingleWalletMode(queryMerchant.WalletMode),
                Memo = "",
                BeginText = beginUTC.AddHours(8).ToString("MM-dd HH:mm"),
                EndText = endUTC.AddHours(8).ToString("MM-dd HH:mm")
            };

            var enableGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                AssignMerchantIDs = queryMerchant.MerchantID.ToListCollection(),
                Status = NormalActiveStatus.Enable.ToInteger(),
            });

            List<int> enableVendorIDs = enableGames.Select(x => AppHelper.ConvertSlot88ToPgsVendorID(x.GameVendorType)).Distinct().ToList();

            var queryVendorConfig = VendorConfigRepository.GetByFilter(new QueryVendorConfigRequestViewModel
            {
                VendorIDs = enableVendorIDs,
                MerchantIDs = queryMerchant.MerchantID.ToListCollection()
            });

            if (queryVendorConfig.IsNotEmptyNull())
            {
                result.GameServerMapping = queryVendorConfig.Select(x => new PlayerGameServerMappingModel
                    {
                        VendorConfigID = x.VendorConfigID,
                        VendorConfigName = x.Name,
                        GameVendorID = x.VendorID
                    })
                    .ToList();
            }

            var vendorAssignIdInfo = queryVendorConfig.Select(x =>
                {
                    var querySetting = VendorConfigSettingRepository.GetAssignVendorIdByVendorConfigID(x.VendorConfigID);
                    return new
                    {
                        AssignVendorId = querySetting.Value,
                        VendorID = x.VendorID,
                        VendorConfigID = x.VendorConfigID
                    };
                })
                .ToList();

            var queryGameSummary = PlayerGameDataRepository.GetEachGameSummary(queryPlayer.PlayerID.ToListCollection(), beginUTC, endUTC);

            foreach (var item in queryGameSummary)
            {
                item.GameVendorID = AppHelper.GetGameVendorByGameID(item.GameID);
                item.GameName = DbMultiLanguageTextHelper.GetGameDisplayName(item.GameID);

                var matchVendorConfig = queryVendorConfig.FirstOrDefault(x => x.VendorID == item.GameVendorID);

                if (matchVendorConfig != null)
                {
                    item.VendorConfigID = matchVendorConfig.VendorConfigID;
                    item.VendorConfigName = matchVendorConfig.Name;

                    var matchAssignID = vendorAssignIdInfo.FirstOrDefault(x => x.VendorConfigID == matchVendorConfig.VendorConfigID)?.AssignVendorId;
                    var matchGameCode = enableGames.FirstOrDefault(z => z.GameID == item.GameID)?.ThirdPartyGameCode;

                    item.ExtraMemo = @$"服務器：{matchAssignID}{Environment.NewLine}" +
                                      $"玩家：{queryPlayer.PlayerID}{Environment.NewLine}" +
                                      $"遊戲：{item.GameName} ({matchGameCode}){Environment.NewLine}" +
                                      $"時間：{filter.Begin.Value.AddHours(8).ToString("yyyy-MM-dd HH:mm")} ~ {filter.Begin.Value.AddHours(8).ToString("yyyy-MM-dd HH:mm")} (GMT + 8)" + Environment.NewLine +
                                      $"RTP: {item.RTP}{Environment.NewLine}{Environment.NewLine}" +
                                      $"您好，客戶想確認此玩家是否有異常";
                }
            }

            result.GameSummary = queryGameSummary.OrderByDescending(x => x.PlayerWinLoss).ToList();

            var bigWinGameSummary = queryGameSummary
                .Where(x => x.PlayerWinLoss > 0)
                .OrderByDescending(x => x.PlayerWinLoss)
                .ToList();

            //如果沒資料了。改成顯示相對贏最多的遊戲 (可能是負盈利)
            if(bigWinGameSummary.IsEmptyOrNull())
            {
                bigWinGameSummary = queryGameSummary
                  .OrderByDescending(x => x.PlayerWinLoss)
                  .ToList();
            }


            decimal totalBet = queryGameSummary.Select(x => x.BetAmount).Sum();
            decimal totalPayout = queryGameSummary.Select(x => x.PayoutAmount).Sum();
            decimal totalWinloss = queryGameSummary.Select(x => x.PlayerWinLoss).Sum();
            decimal totalRound = queryGameSummary.Select(x => x.TotalRound).Sum();
            int totalRound100X = ((int)totalRound / 100) * 100;
            decimal playerRTP = CommonLogicHelper.CalculateRTP_Percentage(totalBet, totalPayout);

            List<int> bigWinGames = new List<int>();

            int top1GameID = -1;
            int top1GameVendor = -1;

            if (bigWinGameSummary.IsNotEmptyNull())
            {
                decimal top1_WinLoss = bigWinGameSummary[0].PlayerWinLoss;
                decimal top2_WinLoss = bigWinGameSummary.Count() >= 2 ? bigWinGameSummary[1].PlayerWinLoss : 1;

                bigWinGames.Add(bigWinGameSummary[0].GameID);

                //如果第一名是第二名的二倍以下
                if (top2_WinLoss != 1 && (top1_WinLoss / top2_WinLoss) < 2)
                {
                    bigWinGames.Add(bigWinGameSummary[1].GameID);
                }
            }
            string bigWinGameMsgCN = string.Empty;
            string bigWinGameMsgEN = string.Empty;
            decimal gameServerRTP_Top1Game = 999;
            decimal gameServerRTP = 999;

            int targetVendorConfigID = 0;

            if (bigWinGames.IsNotEmptyNull())
            {
                bigWinGameMsgCN = string.Join("、", bigWinGames.Select(gameID =>
                {
                    var gameName = DbMultiLanguageTextHelper.GetGameDisplayName(gameID, LanguageEnum.zh_cn);
                    return $"【{gameID}｜{gameName}】";
                }));

                bigWinGameMsgEN = string.Join("、", bigWinGames.Select(gameID =>
                {
                    var gameName = DbMultiLanguageTextHelper.GetGameDisplayName(gameID, LanguageEnum.en_us);
                    return $"【{gameID}｜{gameName}】";
                }));

                top1GameID = bigWinGames[0];
                top1GameVendor = AppHelper.GetGameVendorByGameID(top1GameID);

                if (AppHelper.IsPgsSelfGame(top1GameID))
                {
                    var queryServer = ServerMerchantMappingRepository.GetByFilter(new QueryServerMerchantRelationRequestViewModel
                    {
                        MerchantIDs = queryPlayer.MerchantID.ToListCollection(),
                        GameIDs = top1GameID.ToListCollection()
                    });

                    targetVendorConfigID = queryServer.FirstOrDefault().ServerID;
                }
                else
                {
                    targetVendorConfigID = result.GameServerMapping.FirstOrDefault(x => x.GameVendorID == top1GameVendor).VendorConfigID;
                }

                var queryReportSummary = MerchantGameSummaryDataRepository
                    .GetVendorConfigStatReportGroupByTargetCondition(targetVendorConfigID, beginUTC, endUTC, TableColumnEnum.GameID);

                int top1GamVendor = AppHelper.GetGameVendorByGameID(top1GameID);

                var myGameServerSummary = queryReportSummary
                    .Where(x => AppHelper.GetGameVendorByGameID(x.GameID) == top1GamVendor)
                    .CalculateTotalSumMergeToOneObject();

                var myGameServerSummary_top1 = queryReportSummary.FirstOrDefault(x => x.GameID == top1GameID);

                gameServerRTP = CommonLogicHelper.CalculateRTP_Percentage(myGameServerSummary.BetAmount, myGameServerSummary.PayoutAmount);

                if(myGameServerSummary_top1 != null)
                {
                    gameServerRTP_Top1Game = CommonLogicHelper.CalculateRTP_Percentage(myGameServerSummary_top1.BetAmount, myGameServerSummary_top1.PayoutAmount);
                }
            }

            string columns = " TOP 20 * ";
            var queryBigWinPerGame = PerGameDetailLogRepository.GetByFilter(new QueryDetailLogRequestViewModel
            {
                TagID = SystemTagEnum.PerGameDetailLog_PayoutRate_OverThan50.ToInteger(),
                MerchantID = queryMerchant.MerchantID,
                PlayerIDs = queryPlayer.PlayerID.ToListCollection(),
                GameStartTimeBegin = beginUTC,
                GameStartTimeEnd = endUTC
            }, columns)
                .OrderByDescending(x => x.UpdateAmount)
                .ToList();

            result.BigWinPerGameLogs = queryBigWinPerGame;
            string bigWinRecordsText = string.Empty;

            if (queryBigWinPerGame.IsNotEmptyNull())
            {
                bigWinRecordsText = string.Join("、", queryBigWinPerGame.Select(x =>
                    {
                        return $"{x.TransactionNo} (WinLoss: {x.UpdateAmount})";
                    })
                   .Take(2)
                   .ToList());
            }

            var queryMerchantSummary = MerchantGameSummaryDataRepository.GetEachMerchantTotalSumOnDuration(queryMerchant.MerchantID.ToListCollection(), beginUTC, endUTC).FirstOrDefault();

            int totalRoundOfMerchant = 0;
            decimal merchantRTP = 0;

            if (queryMerchantSummary != null)
            {
                totalRoundOfMerchant = queryMerchantSummary?.TotalRound ?? 0;
                merchantRTP = CommonLogicHelper.CalculateRTP_Percentage(queryMerchantSummary.BetAmount, queryMerchantSummary.PayoutAmount);
            }

            bool isLiveCasinoPlayer = false;

            if (top1GameVendor == (int)GameVendorTypeEnum.AGGLive ||
               top1GameVendor == (int)GameVendorTypeEnum.Galaxy)
            {
                isLiveCasinoPlayer = true;
            }

            //產生訊息文案 (中文/英文)
            //產生訊息文案 (中文/英文)
            //產生訊息文案 (中文/英文)
            #region 產生訊息文案 (中文/英文)

            string msgCN = "";
            msgCN += $"玩家: {queryPlayer.PlayerName} ({queryPlayer.PlayerID}) 商戶: {queryMerchant.MerchantID} ({queryMerchant.Currency})" + Environment.NewLine;
            msgCN += $"时间：{beginUTC.AddHours(8).ToString("MM/dd")} ~ {endUTC.AddHours(8).ToString("MM/dd")}" + Environment.NewLine;
            msgCN += $"玩家这段时间整体RTP为 {playerRTP}" + Environment.NewLine;

            if (!string.IsNullOrWhiteSpace(bigWinGameMsgCN))
            {
                msgCN += $"赢钱较多的是以下游戏 {bigWinGameMsgCN} 。" + Environment.NewLine;
            }

            if (!string.IsNullOrWhiteSpace(bigWinRecordsText))
            {
                msgCN += $"近期输赢较高的注单为 {bigWinRecordsText} " + Environment.NewLine;
            }

            msgCN += Environment.NewLine;


            if (gameServerRTP_Top1Game < 100)
            {
                string top1GameName = bigWinGameMsgCN.IsNotEmptyNull() ? $"({bigWinGameMsgCN[0]})" : "";

                msgCN += $"另外有额外确认 游戏目前整体RTP为 {gameServerRTP_Top1Game} ({top1GameName}) 目前尚未出现负营利。" + Environment.NewLine;

                if (gameServerRTP < 100)
                {
                    msgCN += $"游戏服务器目前整体RTP为 {gameServerRTP_Top1Game} 也尚未出现负营利。" + Environment.NewLine;
                }
            }
            else
            {
                if (merchantRTP < 100)
                {
                    msgCN += $"另外有额外确认 商户目前整体RTP为 {merchantRTP} 目前尚未出现负营利。" + Environment.NewLine;
                }
            }

            if (totalRound <= 2000)
            {
                msgCN += $"玩家目前整体注单数只有 {totalRound100X}。量体不够大、数字可能较为极端。" + Environment.NewLine;
            }

            msgCN += $"少数玩家中奖赢钱是正常现象、目前判断数据暂无异常。谢谢您" + Environment.NewLine;


            string msgEN = "";
            msgEN += $"Player: {queryPlayer.PlayerName} ({queryPlayer.PlayerID}) Merchant: {queryMerchant.MerchantID} ({queryMerchant.Currency})" + Environment.NewLine;
            msgEN += $"Date: {beginUTC.AddHours(8).ToString("MM/dd")} to {endUTC.AddHours(8).ToString("MM/dd")}." + Environment.NewLine;
            msgEN += $"For this player. the overall RTP is {playerRTP}." + Environment.NewLine;

            if (!string.IsNullOrWhiteSpace(bigWinGameMsgEN))
            {
                msgEN += $"Player got the most of win from those games: {bigWinGameMsgEN}" + Environment.NewLine;
            }

            if (!string.IsNullOrWhiteSpace(bigWinRecordsText))
            {
                msgEN += $"big-win order numbers: {bigWinRecordsText} " + Environment.NewLine;
            }

            msgCN += Environment.NewLine;

            if (gameServerRTP_Top1Game < 100)
            {
                string top1GameName = bigWinGameMsgCN.IsNotEmptyNull() ? $"({bigWinGameMsgEN[0]})" : "";

                msgEN += $"BTW, We also confirm the game rtp ({queryMerchant.Currency}). The overall RTP is {gameServerRTP_Top1Game}, negative profitability has not yet appeared." + Environment.NewLine;

                if (gameServerRTP < 100)
                {
                    msgCN += $"for the game server. the current RTP is {gameServerRTP_Top1Game}, negative profitability has not yet appeared too。" + Environment.NewLine;
                }
            }
            else
            {
                if (merchantRTP < 100)
                {
                    msgEN += $"BTW, We also confirm the merchant rtp. The overall RTP is {merchantRTP}, negative profitability has not yet appeared." + Environment.NewLine;
                }
            }

            if (totalRound <= 5000)
            {
                msgEN += $"The player's total round is only {totalRound100X}+. the wager quantity is not enough,so the win-lose gap might be unstable. need more wager quantity to make the win-lose converge to the expected RTP." + Environment.NewLine;
            }

            msgEN += $"It's normal for a few players win money. We consider the data is normal. Thank you." + Environment.NewLine;

            string msgShortCN = "";
            string msgShortEN = "";

            msgShortCN += $"玩家: {queryPlayer.PlayerName} ({queryPlayer.PlayerID})" + Environment.NewLine;
            msgShortCN += $"您好，我方团队查看后无发现异常。{(isLiveCasinoPlayer ? "" : "玩家是正常投注。謝謝您")}";

            msgShortEN += $"Player: {queryPlayer.PlayerName} ({queryPlayer.PlayerID})" + Environment.NewLine;
            msgShortEN += $"Hi, team, after our team checked, we didn't find any abnormal. it's normal bet.";

            #endregion

            result.CopyMsgCN = msgCN;
            result.CopyMsgEN = msgEN;

            result.CopyShortMsgCN = msgShortCN;
            result.CopyShortMsgEN = msgShortEN;
            result.IMChannelName = IMChannelRepository.GetMerchantIMChannel(queryPlayer.MerchantID)?.IMChannelName;

            if(targetVendorConfigID > 0)
            {
                var queryTopVendorConfig = VendorConfigRepository.GetById(targetVendorConfigID);

                if (queryTopVendorConfig != null)
                {
                    var assignAgentID = VendorConfigSettingRepository.GetAssignVendorIdByVendorConfigID(targetVendorConfigID)?.Value;

                    result.VendorConfigInfo = $"服務器：{assignAgentID} (*{targetVendorConfigID})" + Environment.NewLine;
                }
            }

            result.Tags = new List<string>();

            bool isMerchantAvgRoundNotEnough2000 = false;
            bool isMerchantAvgRoundNotEnough5000 = false;

            int avgRoundPerDay = (int)(totalRoundOfMerchant / searchRangeDays);

            if (avgRoundPerDay < 5000)
            {
                if (avgRoundPerDay < 2000)
                {
                    isMerchantAvgRoundNotEnough2000 = true;
                }
                else
                {
                    isMerchantAvgRoundNotEnough5000 = true;
                }

                //只顯示百位數
                var avgRoundPerDayInt = avgRoundPerDay <= 100 ? avgRoundPerDay : ((avgRoundPerDay / 100) * 100);

                result.Tags.Add($"商戶日均注单 {avgRoundPerDayInt}");
            }

            int validateScore = 0;

            if (playerRTP < 100)
            {
                validateScore++;
            }

            if (gameServerRTP_Top1Game < 100)
            {
                validateScore++;
            }

            if (merchantRTP < 100)
            {
                validateScore++;
            }

            //目前判定方式:
            // 1. 玩家局數算多、RTP過高、且遊戲RTP也不佳 > 異常
            // 2. 其中兩個RTP小於100 就認定無異常
            // 3. 越來越多客製...
            if (totalRound > 2000 && playerRTP > 200 && gameServerRTP_Top1Game > 98 && searchRangeDays > 7)
            {
                result.IsValidateOK = false;
            }
            else
            {
                result.IsValidateOK = validateScore >= 2;

                if (!result.IsValidateOK)
                {
                    //商戶RTP無異常、遊戲服務器整體無異常。純粹單一遊戲RTP較高
                    if (merchantRTP < 98 && gameServerRTP < 98 && gameServerRTP_Top1Game < 150)
                    {
                        result.IsValidateOK = true;
                    }

                    //兩者
                    decimal avgRtpOfGame = (gameServerRTP + gameServerRTP_Top1Game) / 2;

                    //商戶注單太少、遊戲服務器整體無異常
                    if (isMerchantAvgRoundNotEnough2000 && playerRTP < 130 && gameServerRTP < 98 && gameServerRTP_Top1Game < 98 && avgRtpOfGame < 97)
                    {
                        result.IsValidateOK = true;
                    }
                    else if (isMerchantAvgRoundNotEnough5000 && playerRTP < 115 && gameServerRTP < 98 && gameServerRTP_Top1Game < 98 && avgRtpOfGame < 97)
                    {
                        result.IsValidateOK = true;
                    }
                    else
                    {
                        if (avgRoundPerDay < 500 && playerRTP < 300 && (gameServerRTP < 99 || gameServerRTP_Top1Game < 99))
                        {
                            result.IsValidateOK = true;
                        }
                        else if (avgRoundPerDay < 1000 && playerRTP < 200 && (gameServerRTP < 98.5M || gameServerRTP_Top1Game < 98.5M))
                        {
                            result.IsValidateOK = true;
                        }
                    }
                }
            }


            var begin_24hour = DateTimeHelper.GetCurrentTime().AddHours(-1 * 24);
            var end_24hour = DateTimeHelper.GetCurrentTime();

            var querySummary_24Hour = PlayerGameDataRepository.GetEachGameSummary(queryPlayer.PlayerID.ToListCollection(), begin_24hour, end_24hour);

            if(querySummary_24Hour.IsNotEmptyNull())
            {
                decimal totalBet_24hour = querySummary_24Hour.Select(x => x.BetAmount).Sum();
                decimal totalPay_24hour = querySummary_24Hour.Select(x => x.PayoutAmount).Sum();

                decimal rtp_24hour = CommonLogicHelper.CalculateRTP_Percentage(totalBet_24hour, totalPay_24hour);
                result.Tags.Add($"今日RTP: {rtp_24hour}");
            }

            if (isLiveCasinoPlayer)
            {
                result.Tags.Add($"【視訊遊戲】需確認對賭");

                //視訊遊戲RTP超過一定值。還是人工確認下是否有對賭
                if(playerRTP > 97)
                {
                    result.IsValidateOK = false;
                }
            }

            result.RTP_Game = gameServerRTP_Top1Game;
            result.RTP_GameServerAll = gameServerRTP;
            result.RTP_Player = playerRTP;
            result.RTP_Merchant = merchantRTP;

            var log = InstanceOperateLog(userID, UserOperateTypeEnum.SearchPlayerSuspicoiusCheck, queryPlayer.PlayerID);
            log.Memo = $"isOK:{result.IsValidateOK} , {filter.Begin}-{filter.End}";

            WriteOperateLog(log);

            //如果是視訊遊戲會額外用另一種邏輯查
            if(isLiveCasinoPlayer)
            {
                result.IsLiveCasinoPlayer = isLiveCasinoPlayer;
                OverrideCheckResultByLiveCasinoLogic(userID, filter, result);
            }
            else
            {
                SetPlayerTags(queryPlayer.PlayerID, result);
            }

            return result;
        }


        private void SetPlayerTags<T>(int playerID , T result) where T: TagsModel
        {
            var queryTags = PlayerTagMappingRepository.GetPlayerTagsAndName(playerID);

            //部分tag隱藏 不需要在這邊顯示
            queryTags = queryTags
                .Where(x => x.TagID != (int)SystemTagEnum.Player_PlayerBehaviorEvent)
                .ToList();

            if (queryTags.IsNotEmptyNull())
            {
                result.Tags.AddRange(queryTags.Select(x => x.TagName));
            }
        }


        private void OverrideCheckResultByLiveCasinoLogic(int userID, LazyCommonRequestModel filter, PlayerSuspiciousCheckResultViewModel viewModel)
        {
            var liveCheck = CheckPlayerLiveCasinoGameSuspicious(userID, filter);

            bool isLargerBetAmount = false;
            bool isSmallBetAmount = false;
            bool isSmallWinLossAmount = false;

            var queryCurrency = CurrencyRepository.GetById(viewModel.Currency);

            //超過一定金額才視為大額玩家
            if (queryCurrency != null)
            {
                decimal convertToUSD = liveCheck.TotalBetAmount * queryCurrency.ExchangeRateToUSDT;
                decimal convertToUSD_winLoss = liveCheck.TotalWinLossAmount * queryCurrency.ExchangeRateToUSDT;

                isLargerBetAmount = convertToUSD > 1000;
                isSmallBetAmount = convertToUSD < 200;
                isSmallWinLossAmount = convertToUSD_winLoss < 150;
            }
            else
            {
                isLargerBetAmount = true;
            }
                      
            if(liveCheck.Tags.IsNotEmptyNull())
            {
                foreach (var tag in liveCheck.Tags)
                {
                    if (tag == "Galaxy")
                        continue;

                    viewModel.Tags.Add(tag);
                }
            }

            bool isNoSuspicoiusSameRound = liveCheck.PlayerBaseAndEachRoundCompare.All(x => x.SuspiciousRatePercentage < 30);

            var trustCustomerTag = PlayerTagMappingRepository.GetPlayerTag(viewModel.PlayerID, SystemTagEnum.Player_OldTrustCustomer);

            bool isTrustCustomers = trustCustomerTag != null;

            //總之就是小額或注單數不多 RTP合理。就直接視為正常            
            if (isLargerBetAmount && liveCheck.RTP_Player >= 97.5M)
            {
                //忠實老玩家 可以判定寬鬆一點
                if(isTrustCustomers && isNoSuspicoiusSameRound && liveCheck.RTP_Player <= 105)
                {
                    viewModel.IsValidateOK = true;
                    return;
                }
                else
                {
                    //大額玩家 一律交給技術確認
                    viewModel.IsValidateOK = false;
                    viewModel.Tags.Add("大額投注玩家");
                    return;
                }
            }
            else if(isNoSuspicoiusSameRound)
            {
                decimal extraRTPMinus = 0;
                decimal extraRTPMultiple = 0;

                //小額投注 真的沒啥好檢查的...
                if (isSmallBetAmount)
                {
                    extraRTPMultiple += 0.15M;   
                    extraRTPMinus = extraRTPMinus + 1;                    
                }
                if(isSmallWinLossAmount)
                {
                    extraRTPMultiple += 0.15M;
                    extraRTPMinus = extraRTPMinus + 1;
                }

                if (isTrustCustomers)
                {
                    extraRTPMinus = extraRTPMinus + 2;
                }

                //根據局數多寡，會有個指數型的RTP預期值 (局數少、容許較極端的RTP)
                decimal expectRTP = ExponentialHelper.DetermineCasinoGame_SuspiciousRTP_ByRound(liveCheck.TotalRoundOfMe);

                //如果商戶RTP正盈利。允許調高一點判斷值
                if(liveCheck.RTP_Merchant < 98)
                {
                    expectRTP = expectRTP + 0.5M;
                }

                decimal finalCompareRTP = (expectRTP + extraRTPMinus + (expectRTP - 95) * extraRTPMultiple);

                viewModel.Tags.Add("預期合理RTP:" + Math.Round(finalCompareRTP, 1));

                if (liveCheck.RTP_Player <= finalCompareRTP)
                {
                    viewModel.IsValidateOK = true;
                }
            }

            return;
        }


        public PlayerLiveCasinoSuspiciousCheckResultViewModel CheckPlayerLiveCasinoGameSuspicious(int userID, LazyCommonRequestModel filter)
        {
            filter.RuleFor(x => x.Begin).Rule_NotNullDateTime().Rule_StartTimeMustSmallThanEndTime(filter.End);
            filter.RuleFor(x => x.End).Rule_NotNullDateTime();

            DateTime beginTW = filter.Begin.Value.AddHours(8);
            DateTime beginUTC = filter.Begin.Value;
            DateTime endUTC = filter.End.Value;
            DateTime now = DateTimeHelper.GetCurrentTime();
            DateTime endTW = endUTC.AddHours(8);

            if (endUTC > now)
            {
                endUTC = DateTimeHelper.GetCurrentTime(8).AddDays(1).To_00_00_00().AddHours(-8);
            }

            int minDays = 2;
            int maxDays = 14;
            int searchRangeDays = (int)(endUTC - beginUTC).TotalDays;

            //if (IsCustomerServicePermission(userID) && searchRangeDays <= minDays)
            //{
            //    string dateRangeText = $"{beginUTC.AddHours(8).ToString("yyyy-MM-dd")} - {endUTC.AddHours(8).ToString("yyyy-MM-dd")}";
            //    string msg = $"The search time range must be at least {minDays} days (excluding future time). 您搜寻的时间区间至少要拉{minDays}天以上 (不含未來時間)。时间太短数据不够精准 ({dateRangeText})";
            //    throw new TerproException(ApiErrorCode.BusinessLogicDeny, msg);
            //}

            if (searchRangeDays > maxDays)
            {
                string dateRangeText = $"{beginUTC.AddHours(8).ToString("yyyy-MM-dd")} - {endUTC.AddHours(8).ToString("yyyy-MM-dd")}";
                string msg = $"The search time range must be less than {maxDays} days (excluding future time). 您搜寻的时间区间至少要小於{maxDays}天。時間太長會造成系統負擔 ({dateRangeText})";
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, msg);
            }

            int targetPlayerID = GetTargetPlayerIDByParameter(userID, filter);

            var queryPlayer = PlayerRepository.GetSingleByCache(targetPlayerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            var queryMerchant = MerchantRepository.GetSingleByCache(queryPlayer.MerchantID);

            CheckViewMerchantPermission(userID, queryPlayer.MerchantID);

            //用單號查詢是否有其他玩家 好幾局都一起 而且投注金額比對
            //用單號查詢是否有其他玩家 好幾局都一起 而且投注金額比對
            //用單號查詢是否有其他玩家 好幾局都一起 而且投注金額比對
            //用單號查詢是否有其他玩家 好幾局都一起 而且投注金額比對
            //用單號查詢是否有其他玩家 好幾局都一起 而且投注金額比對


            int gameVendor_AGG = (int)GameVendorTypeEnum.AGGLive;
            int gameVendor_Galaxy = (int)GameVendorTypeEnum.Galaxy;

            var queryStatisticSummary = PlayerGameDataRepository.GetEachGameSummary(queryPlayer.PlayerID.ToListCollection(), beginUTC, endUTC);

            int totalSumOfAGGLive = queryStatisticSummary.Where(x => AppHelper.GetGameVendorByGameID(x.GameID) == gameVendor_AGG).Select(x => x.TotalRound).Sum();
            int totalSumOfGalaxy = queryStatisticSummary.Where(x => AppHelper.GetGameVendorByGameID(x.GameID) == gameVendor_Galaxy).Select(x => x.TotalRound).Sum();

            var queryStatisticSummaryOfLive = queryStatisticSummary
                .Where(x => AppHelper.GetGameVendorByGameID(x.GameID) == gameVendor_AGG || AppHelper.GetGameVendorByGameID(x.GameID) == gameVendor_Galaxy)
                .ToList();

            var totalSumOfLiveGame = queryStatisticSummary.CalculateTotalSumMergeToOneObject();

            int? top1GameID = null;
            string top1Game = string.Empty;

            var targetGameVendor = GameVendorTypeEnum.AGGLive;

            bool isMajorOnAGGLive = true;

            if(totalSumOfGalaxy > totalSumOfAGGLive)
            {
                //galaxy為主力
                top1GameID = queryStatisticSummary
                    .Where(x => AppHelper.GetGameVendorByGameID(x.GameID) == (int)GameVendorTypeEnum.Galaxy)
                    .OrderByDescending(x => x.BetAmount)
                    .FirstOrDefault()
                    .GameID;

                targetGameVendor = GameVendorTypeEnum.Galaxy;
                isMajorOnAGGLive = false;
            }
            else if(totalSumOfAGGLive > 0)
            {
                top1GameID = queryStatisticSummary
                  .Where(x => AppHelper.GetGameVendorByGameID(x.GameID) == (int)GameVendorTypeEnum.AGGLive)
                  .OrderByDescending(x => x.BetAmount)
                  .FirstOrDefault()
                  .GameID;
            }

            int targetVendorConfigID = VendorConfigRepository.GetCacheByVendorTypeAndMerchant(queryPlayer.MerchantID, targetGameVendor.ToInteger())?.VendorConfigID ?? 0;
           
            List<SameRoundIdPlayersModel> querySameRoundPlayers = BetRecordAGGLiveRepository.GetSameRoundSummaryByPlayerID(queryPlayer.PlayerID, beginUTC, endUTC , targetGameVendor);

            int totalRoundOfMe = querySameRoundPlayers.Where(x => x.PlayerID == queryPlayer.PlayerID).Select(x => x.RoundID).Distinct().Count();
            int totalRound = queryStatisticSummary
                  .Where(x => x.GameID == top1GameID)
                  .Select(x => x.TotalRound)
                  .Sum();

            var groupByPlayerRound = querySameRoundPlayers
                .GroupBy(x => x.PlayerID)
                .Select(x => new
                {
                    PlayerID = x.Key,
                    RoundSummaryItems = x.ToList(),
                    TotalRound = x.Select(x => x.RoundID).Distinct().Count(),
                })
                .ToList();

            //假如100局 至少要10局相遇 才會視為可疑玩家
            int minCompareValue = (int)(totalRoundOfMe / 10);

            if (minCompareValue <= 2)
                minCompareValue = 2;

            groupByPlayerRound = groupByPlayerRound
                .Where(x => x.TotalRound > minCompareValue || x.TotalRound > 20) //排除局數太少的
                .ToList();

            List<int> otherSuspiciousPlayers = groupByPlayerRound.Select(x => x.PlayerID).ToList();

            querySameRoundPlayers = querySameRoundPlayers              
                .Where(x => otherSuspiciousPlayers.Contains(x.PlayerID))
                .ToList();

            List<PlayerRoundSummaryInfoItemModel> SameRoundMapping = new List<PlayerRoundSummaryInfoItemModel>();

            List<PlayerRoundSummaryInfoItemModel> PlayerBaseAndEachRoundCompare = new List<PlayerRoundSummaryInfoItemModel>();

            //玩家001 總局數: 50
            //A 同一局數: 35
            //B 同一局數: 20

            var playerIDs = querySameRoundPlayers.Select(x => x.PlayerID).ToList();

            var queryPlayers = PlayerRepository.GetByIds(playerIDs);

            var myViewModel = new PlayerLiveCasinoSuspiciousCheckResultViewModel
            {
                PlayerID = queryPlayer.PlayerID,
                PlayerName = queryPlayer.PlayerName,
                MerchantID = queryMerchant.MerchantID,
                MerchantName = queryMerchant.Name,
                Currency = queryMerchant.Currency,
                BeginText = beginTW.ToString("yyyy-MM-dd"),
                EndText = endTW.ToString("yyyy-MM-dd"),
                Tags = new List<string>(),
            };

            if(isMajorOnAGGLive && totalRoundOfMe > 0)
            {
                myViewModel.Tags.Add("AGGLive");
            }
            else
            {
                myViewModel.Tags.Add("Galaxy");
            }

            myViewModel.TotalBetAmount = queryStatisticSummaryOfLive.Select(x => x.BetAmount).Sum();
            myViewModel.TotalWinLossAmount = queryStatisticSummaryOfLive.Select(x => x.PlayerWinLoss).Sum();
            myViewModel.TotalRoundOfMe = totalRoundOfMe;
            myViewModel.TotalRound = totalRound;
            myViewModel.PlayerBaseAndEachRoundCompare = new List<PlayerBaseAndEachRoundCompare>();
            myViewModel.SameRoundPlayers = groupByPlayerRound
                .Where(x=> x.PlayerID != queryPlayer.PlayerID)
                .OrderByDescending(x=> x.TotalRound)
                .Select(x => new PlayerRoundSummaryInfoItemModel
                {
                    PlayerID = x.PlayerID,
                    PlayerName = queryPlayers.FirstOrDefault(z=> z.PlayerID == x.PlayerID)?.PlayerName,
                    TotalRound = x.TotalRound,
                    SameRoomRatePercentage = Math.Round(AppHelper.CalculateRate(x.TotalRound , totalRoundOfMe) * 100 , 2),
                })
                .ToList();

            List<string> roundIDs = querySameRoundPlayers.Select(x => x.RoundID).Distinct().ToList();

            var roundsOfSuspiciousPlayer = groupByPlayerRound
                .Where(x => x.PlayerID == queryPlayer.PlayerID)
                .FirstOrDefault();

            var roundSummaryOfOtherPlayer = groupByPlayerRound
                .Where(x => x.PlayerID != queryPlayer.PlayerID)
                .ToList();


            string top1GameNameCN = "";

            if(top1GameID.HasValue)
            {
                var queryGameName = DbMultiLanguageTextHelper.GetGameDisplayName(top1GameID.Value , LanguageEnum.zh_cn);
                top1GameNameCN = $"主要游玩: {queryGameName} ({top1GameID.Value})";
            }

            foreach (var item in roundSummaryOfOtherPlayer)
            {
                var roundsOfThisPlayer = item.RoundSummaryItems;

                var myInstance = new PlayerBaseAndEachRoundCompare
                {
                    PlayerID = item.PlayerID,
                    PlayerName = queryPlayers.FirstOrDefault(z => z.PlayerID == item.PlayerID)?.PlayerName,
                    EachRoundCompareItems = new List<PlayerRoundBetAmountCompareModel>()
                };

                foreach(var otherPlayer in roundsOfThisPlayer)
                {
                    var compareBasePlayer = roundsOfSuspiciousPlayer.RoundSummaryItems.FirstOrDefault(x => x.RoundID == otherPlayer.RoundID);

                    var roundModel = new PlayerRoundBetAmountCompareModel
                    {
                        RoundID = otherPlayer.RoundID,

                        //主要玩家
                        TotalBetAmountOfSuspiciousPlayer = compareBasePlayer.TotalBetAmount,
                        TotalWinLossOfSuspiciousPlayer = compareBasePlayer.TotalWinLossAmount,

                        TotalBetAmountOfOtherPlayer = otherPlayer.TotalBetAmount,
                        TotalWinLossOfOtherPlayer = otherPlayer.TotalWinLossAmount,
                    };

                    //總之就是兩個對賭 且金額差距在10%內
                    if(((roundModel.TotalBetAmountOfOtherPlayer - roundModel.TotalBetAmountOfSuspiciousPlayer).ToAbs() / roundModel.TotalBetAmountOfSuspiciousPlayer) < 0.1M)
                    {
                        bool isMinus = (roundModel.TotalWinLossOfOtherPlayer * roundModel.TotalWinLossOfSuspiciousPlayer) < 0;

                        if (isMinus)
                        {
                            decimal diff = (roundModel.TotalWinLossOfOtherPlayer + roundModel.TotalWinLossOfSuspiciousPlayer).ToAbs() / roundModel.TotalWinLossOfSuspiciousPlayer.ToAbs();
                        
                            if(diff < 0.1M)
                            {
                                roundModel.IsSuspicious = true;
                            }                        
                        }
                    }

                    myInstance.EachRoundCompareItems.Add(roundModel);
                }

                myInstance.EachRoundCompareItems = myInstance.EachRoundCompareItems.OrderBy(x => x.IsSuspicious ? 0 : 999).ToList();
                myInstance.TotalRoundOfSuspicious = myInstance.EachRoundCompareItems.Where(x => x.IsSuspicious).Count();
                myInstance.TotalRound = roundsOfThisPlayer.Count();

                if (myInstance.TotalRound > 0)
                {
                    myInstance.SuspiciousRatePercentage = Math.Round(AppHelper.CalculateRate(myInstance.TotalRoundOfSuspicious , myInstance.TotalRound) * 100 , 2);
                }

                myInstance.CopyText_CN = $"玩家:{queryPlayer.PlayerName}({queryPlayer.PlayerID}) 近期视讯游戏总局数:{totalRoundOfMe} ,{top1GameNameCN}{Environment.NewLine}" + 
                                         $"与另名玩家:{myInstance.PlayerName} 共有{ myInstance.TotalRound}局出现在同一局中。其中{myInstance.TotalRoundOfSuspicious}局疑似有对赌现象或异常{Environment.NewLine}" +
                                         $"技术单位这边为您查询到这些资讯、供您参考下";

                myViewModel.PlayerBaseAndEachRoundCompare.Add(myInstance);
            }

            myViewModel.RTP_Game = AppHelper.CalculateRTP_Percentage(totalSumOfLiveGame.BetAmount, totalSumOfLiveGame.PayoutAmount);

            var queryReportSummaryOfGame = MerchantGameSummaryDataRepository.GetEachVendorConfigGameDailyReportGroupByGame(beginUTC, endUTC)
                .Where(z => z.VendorConfigID == targetVendorConfigID)
                .Where(z => z.GameID == top1GameID)
                .FirstOrDefault();

            if(queryReportSummaryOfGame != null)
            {
                myViewModel.RTP_Game = AppHelper.CalculateRTP_Percentage(queryReportSummaryOfGame.BetAmount, queryReportSummaryOfGame.PayoutAmount);
            }

            myViewModel.RTP_Player = AppHelper.CalculateRTP_Percentage(totalSumOfLiveGame.BetAmount, totalSumOfLiveGame.PayoutAmount);
            myViewModel.PlayerBaseAndEachRoundCompare = myViewModel.PlayerBaseAndEachRoundCompare
                .OrderByDescending(x => x.TotalRoundOfSuspicious)
                .ToList();

            SetPlayerTags(queryPlayer.PlayerID, myViewModel);

            return myViewModel;
        }



        private int GetTargetPlayerIDByParameter(int userID, LazyCommonRequestModel filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.PlayerName))
            {
                var playerFindResult = GetPlayerIDsByPlayerName(userID, filter.PlayerName, new List<int>());

                if (playerFindResult.IsPlayerNotFound)
                {
                    throw new TerproException(ApiErrorCode.NotFoundPlayer);
                }

                if (playerFindResult.PlayerIDs.IsNotEmptyNull() && playerFindResult.PlayerIDs.Count() > 1)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_MultiplePlayerNameMatch_PleaseUsePlayerID);
                }

                return playerFindResult.PlayerIDs.FirstOrDefault();
            }
            else
            {
                filter.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();
                return filter.PlayerID;
            }

        }


    }
}
