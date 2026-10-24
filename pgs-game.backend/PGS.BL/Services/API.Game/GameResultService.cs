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
using target.Utility.Helper;
using target.Models;
using target.Models.Attributes;
using target.Utility;
using game.Entity.Game;
using target.Utility.Utility.ThirdParty;
using target.Utility.IOC;

namespace target.BL.Services
{

    public class GameResultService : BaseGameService, IGameResultService
    {
        public BetRecordRepository BetRecordRepository { get; set; }

        public BetRecordExtraInfoRepository BetRecordExtraInfoRepository { get; set; }

        public BetRecordCustomizedSettingRepository BetRecordCustomizedSettingRepository { get; set; }

        public BetRecordCustomizedSettingSWSessionTokenRepository BetRecordCustomizedSettingSWSessionTokenRepository { get; set; }

        public PerGameLogRepository PerGameLogRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public PerGameDetailLogZeroRepository PerGameDetailLogZeroRepository { get; set; }

        public SingleWalletMerchantSessionTokenRepository SingleWalletMerchantSessionTokenRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public ServerListRepository ServerListRepository { get; set; }

        public EazyTempValueRepository EazyTempValueRepository { get; set; }

        public PlayerTagMappingRepository PlayerTagMappingRepository { get; set; }

        public PerGameDetailLogAccountingIgnoreRepository PerGameDetailLogAccountingIgnoreRepository { get; set; }

        public IPlayerWalletRefService PlayerWalletRefService { get; set; }

        public ISingleWalletCallbackNotifyProcessRefService SingleWalletMerchantBetProcessRefService { get; set; }


        /// <summary>
        /// 若判斷機器人 會將merchantID改成 -1 並且不寫入資料
        /// </summary>
        public int _robotSkipInsertMerchantID = -1;


        public GamePlayResultViewModel UpdateSettlementAmount(GamePlayResultRequestModel model)
        {
            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.ServerID).Rule_NotZeroNotMinus();

            var queryPlayer = GetValidPlayerAndMerchant(model.PlayerID);

            var queryServer = ServerListRepository.GetBriefByCache(model.ServerID);

            if (queryServer == null)
                throw new TerproException(ApiErrorCode.NotFoundServer);

            var queryGame = GameListRepository.GetSingleByCache(queryServer.GameID);

            //目前暫時沒有試玩模式
            WalletTypeEnum walletType = WalletTypeEnum.GameWallet;

            decimal updateGamePoint = model.UpdateAmount;
            decimal updateAmount = ConvertGamePointToMoney(queryPlayer.MerchantID, updateGamePoint);

            var updateModel = model.ReturnInjecteObject<WriteGameResulSPParameterModel>();
            updateModel.WalletType = walletType.ToInteger();
            updateModel.WalletMode = WalletTypeEnum.GameWallet.ToInteger();
            updateModel.GameID = queryServer.GameID;
            updateModel.UpdateAmount = updateAmount;

            bool isSingleWallet = CommonLogicHelper.IsSingleWalletMode(queryPlayer.WalletMode);
            bool isTransferMode = isSingleWallet == false;

            bool is_V1_Process = !model.IsPlaceBetProcess;

            //轉帳模式 可以記錄到更新錢金額
            if (isTransferMode)
            {
                var walletAmount = PlayerWalletRefService.GetWalletAmountByMerchantWalletMode(model.PlayerID, "");
                updateModel.CurrentAmount = walletAmount.Amount;
            }

            //舊版模式(v1)
            if (is_V1_Process)
            {
                //更新Db錢包金額 並結束
                PlayerRepository.UpdatePlayerWalletAfterGameSettle(updateModel);

                decimal afterAmount = updateModel.CurrentAmount + updateAmount;
                decimal afterGamePoint = ConvertGamePointToMoney(queryPlayer.MerchantID, afterAmount);

                return new GamePlayResultViewModel
                {
                    AfterAmount = afterGamePoint
                };
            }

            //如果是v2版的結算邏輯 (單錢包模式的邏輯)
            var betInfo = model.BetInfo;

            betInfo.RuleFor(z => z.BetAmount).Rule_NotMinus();
            betInfo.RuleFor(z => z.TransactionNo).Rule_NotEmptyText();

            string reqeustUUID = Guid.NewGuid().ToString();
            betInfo.RuleFor(z => z.TransactionNo).Rule_NotEmptyText();

            var queryBetRecord = BetRecordRepository.GetByTransactionNo(betInfo.TransactionNo);

            if (queryBetRecord == null)
            {
                throw new TerproException(ApiErrorCode.NotFoundRecord, new ErrorDetailModel
                {
                    IsAlwaysNotify = true,
                    LogExtraInfo = $"TransactionNo: {betInfo.TransactionNo}"
                });
            }

            if (queryBetRecord.Status != BetRecordStatusEnum.Pending.ToInteger() &&
               model.UpdateAmount > 0 && !SettingHelper.IsMSTest)
            {
                var queryMerchant = MerchantRepository.GetSingleByCache(queryPlayer.MerchantID);
                if (CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode))
                {
                    string msg = $"GameID: {queryGame.GameID} 出現二次結算，單號：{model.BetInfo?.TransactionNo}、派獎金額：{model.BetInfo?.PayoutAmount}";
                    SendNotify(msg);
                }
            }

            decimal betGamePoint = betInfo.BetAmount;
            decimal payoutGamePoint = betInfo.PayoutAmount;
            decimal validGamePoint = betInfo.ValidAmount;
            decimal taxGamePoint = betInfo.TaxAmount;

            decimal convertRate = GetMerchantConvertRate_PGSGame(queryPlayer.MerchantID);
            decimal betAmount = ConvertGamePointToMoney(queryPlayer.MerchantID, betGamePoint, convertRate);
            decimal payoutAmount = ConvertGamePointToMoney(queryPlayer.MerchantID, payoutGamePoint, convertRate);
            decimal validAmount = ConvertGamePointToMoney(queryPlayer.MerchantID, validGamePoint, convertRate);
            decimal taxAmount = ConvertGamePointToMoney(queryPlayer.MerchantID, taxGamePoint, convertRate);


            DateTime now = DateTimeHelper.GetCurrentTime();
            queryBetRecord.Status = BetRecordStatusEnum.SettleComplete.ToInteger();
            queryBetRecord.WinLossAmount = AppHelper.CalculateWinLoss(betAmount, payoutAmount);
            queryBetRecord.PayoutAmount = payoutAmount;
            queryBetRecord.ValidAmount = validAmount;
            queryBetRecord.TaxAmount = taxAmount;
            queryBetRecord.UpdateTime = now;
            queryBetRecord.SettlementTime = now;

            //很重要的欄位 (預計2023/06 加入此欄位)
            if (model.SettleTime.HasValue)
            {
                queryBetRecord.SettlementTime = model.SettleTime.Value;
            }

            //若投注金額為0元，會走bonus結算流程
            bool isFreeBonus = queryBetRecord.Type == BetRecordTypeEnum.FreeBonus.ToInteger();

            bool isFreeGameWithoutMoneyChanged = isFreeBonus && model.BetInfo.PayoutAmount == 0;

            try
            {
                // Step 1. 更新betRecord
                BetRecordRepository.Update(queryBetRecord);

                // Step 1-2. 更新debugInfo
                BetRecordExtraInfoRepository.CreateOrUpdateAsync(queryBetRecord.BetRecordNoPGS, "");
                              
                // Case 1-3. FreeGame結算、無中獎
                if (isFreeGameWithoutMoneyChanged)
                {
                    var walletResult = PlayerWalletRefService.GetWalletAmountByLatestToken(model.PlayerID);
                    return new GamePlayResultViewModel
                    {
                        AfterAmount = ConvertMoneyToGamePoint(queryPlayer.MerchantID, walletResult.Amount, convertRate)
                    };
                }               
            }
            catch(Exception ex)
            {
                SendErrorMsgAndNotify(isSingleWallet, queryBetRecord, ex);
                throw;
            }


            // Case 2. 轉帳模式
            if (!isSingleWallet)
            {
                // [轉帳模式] 更新Db錢包金額
                PlayerRepository.UpdatePlayerWalletAfterGameSettle(updateModel);
                return new GamePlayResultViewModel
                {
                    AfterAmount = ConvertMoneyToGamePoint(queryPlayer.MerchantID, updateModel.CurrentAmount + updateAmount, convertRate)
                };
            }

            // Case 3. 單錢包模式-一般投注
            if (!isFreeBonus)
            {
                // [單錢包模式] 呼叫對方API
                var swResult = SingleWalletMerchantBetProcessRefService.SettlemenetProcessCallbackMerchantApi(new SingleWalletSettleCallbackParameter
                {
                    PlayerID = model.PlayerID,
                    BetAmount = betAmount,
                    PayoutAmount = payoutAmount,
                    TaxAmount = taxAmount,
                    ValidAmount = validAmount,
                    MerchantID = queryPlayer.MerchantID,
                    GameID = queryServer.GameID,
                    SettlementTime = queryBetRecord.SettlementTime,
                    BetRecordNoPGS = queryBetRecord.BetRecordNoPGS,
                    DebugProcessUUID = queryBetRecord.BetRecordNoPGS,
                });

                return new GamePlayResultViewModel
                {
                    AfterAmount = ConvertMoneyToGamePoint(queryPlayer.MerchantID, swResult.Balance, convertRate)
                };
            }

            var bonusParam = new SingleWalletSettleCallbackParameter
            {
                BetAmount = 0,
                PlayerID = queryBetRecord.PlayerID,
                PayoutAmount = payoutAmount,
                TaxAmount = taxAmount,
                ValidAmount = validAmount,
                MerchantID = queryPlayer.MerchantID,
                GameID = queryServer.GameID,
                BetRecordNoPGS = model.BetInfo.TransactionNo,
                SettlementTime = queryBetRecord.SettlementTime,
                DebugProcessUUID = reqeustUUID
            };

            // 2023/08 北七商戶的需求
            if (isSingleWallet)
            {
                var lastToken = [REDACTED];

                if(lastToken != null)
                {
                    BetRecordCustomizedSettingSWSessionTokenRepository.Create(model.BetInfo.TransactionNo, lastToken.SessionTokenRaw);
                    bonusParam.SessionTokenPGS = lastToken.SessionTokenPGS;
                }
            }
                        
            // Case 4. 單錢包模式-FreeGame/Bonus
            var bonusResult = SingleWalletMerchantBetProcessRefService.BonusProcessCallbackMerchantApi(bonusParam);

            return new GamePlayResultViewModel
            {
                AfterAmount = ConvertMoneyToGamePoint(queryPlayer.MerchantID, bonusResult.Balance, convertRate)
            };
        }
              
        /// <summary>
        /// 寫每局遊戲紀錄
        /// </summary>
        public PerGameLogViewModel WritePerGameLog(PerGameLogRequestModel model)
        {
            var perGameLogModel = model.PerGameLog;
            model.RuleFor(x => x.PerGameLog).Rule_ModelNotNull();
            perGameLogModel.RuleFor(x => x.ServerID).Rule_NotZeroNotMinus();
            perGameLogModel.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();
            perGameLogModel.RuleFor(x => x.GameData).Rule_NotEmptyText();
            perGameLogModel.RuleFor(x => x.GameBeginTime).Rule_NotEmptyDateTime();
            perGameLogModel.RuleFor(x => x.GameEndTime).Rule_NotEmptyDateTime();

            foreach (var item in model.PerGameLog.UserGameDetailLogs.NullToEmptyCollection())
            {
                item.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();
                item.RuleFor(x => x.TradeID).Rule_NotZeroNotMinus();
            }

            var queryGame = GameListRepository.GetSingleByCache(perGameLogModel.GameID);

            if (queryGame == null)
                throw new TerproException(ApiErrorCode.NotFoundGame);

            decimal totalTaxGamePoint = model.PerGameLog.TaxAmount;
            decimal totalTaxAmount = model.PerGameLog.TaxAmount; //這邊不管了...難以處理

            //step 1. 寫入perGame
            long perGameID = PerGameLogRepository.Create(new PerGameLogEntity
            {
                BeginTime = model.PerGameLog.GameBeginTime,
                EndTime = model.PerGameLog.GameEndTime,
                GameData = model.PerGameLog.GameData,
                ServerID = model.PerGameLog.ServerID,
                GameID = model.PerGameLog.GameID,
                TaxAmount = model.PerGameLog.TaxAmount,
                CreateTime = DateTimeHelper.GetCurrentTime()
            });

            List<PerGameDetailLogEntity> detailLogs = new List<PerGameDetailLogEntity>();
            List<PerGameDetailLogAccountingIgnoreEntity> detailLogsAccountingIgnore = new List<PerGameDetailLogAccountingIgnoreEntity>();
            List<PerGameDetailLogZeroEntity> detailLogsZero = new List<PerGameDetailLogZeroEntity>();
            List<BetRecordCustomizedSettingEntity> insertCustomizedItems = new List<BetRecordCustomizedSettingEntity>();            

            List<string> needEventBonusRecords = new List<string>();

            // v1舊版寫法(理論上已沒用處了) 傳入字串、拆解解析字串 轉成 PerGameDetailLog
            detailLogs = OldVersionAssignPerGameLogsValues(model, perGameID);

            bool isAlwaysWriteLog = !queryGame.IsSkipInsertZeroBet;
            bool IsSkipInserZeroBet = queryGame.IsSkipInsertZeroBet;

            DateTime now = DateTimeHelper.GetCurrentTime();

            //新版寫法 傳入array物件
            if (model.PerGameLog.UserGameDetailLogs.IsNotEmptyNull())
            {
                //若此gameID有定義 IsSkipInserZeroBet 的話，當投注行為(掛網觀局)的資料會寫到另一張表
                var logs = model.PerGameLog.UserGameDetailLogs.Where(x => isAlwaysWriteLog || !(x.BetAmount == 0 && x.UpdateAmount == 0)).ToList();
                var logs_zero = model.PerGameLog.UserGameDetailLogs.Where(x => IsSkipInserZeroBet && x.BetAmount == 0 && x.UpdateAmount == 0).ToList();

                DateTime insertGameTime = model.PerGameLog.GameEndTime;

                //pgs遊戲 會傳入此值(結算時間以此為準、讓perGame、betRecord兩邊的數字一致)
                if (model.PerGameLog.SettleTime.HasValue)
                {
                    insertGameTime = model.PerGameLog.SettleTime.Value;
                }

                var preQueryBeforeWrite = GetPlayersAndMerchantConvertRate(logs.Select(x => x.PlayerID));

                foreach (var item in logs)
                {
                    var queryPlayer = preQueryBeforeWrite.Players.FirstOrDefault(p=> p.PlayerID == item.PlayerID);

                    if (queryPlayer == null)
                        throw new TerproException(ApiErrorCode.NotFoundPlayer , true);

                    bool isSkipAccountingData = PlayerTagMappingRepository.IsPlayerTagExistByCache(queryPlayer.PlayerID, SystemTagEnum.Player_GameVendorAcoountingIgnoreThisPlayer.ToInteger());

                    bool isWriteToAccountingIgnore = isSkipAccountingData && queryGame.IsSupportForAccountingIgnorePlayer;

                    decimal convertRate = preQueryBeforeWrite.MerchantConvertRate.GetDictionaryOrDefaultValue(queryPlayer.MerchantID, 1);

                    decimal betAmount = ConvertGamePointToMoney(queryPlayer.MerchantID, item.BetAmount , convertRate);
                    decimal taxAmount = ConvertGamePointToMoney(queryPlayer.MerchantID, item.TaxAmount, convertRate);
                    decimal validAmount = ConvertGamePointToMoney(queryPlayer.MerchantID, item.ValidAmount, convertRate);
                    decimal currentAmount = ConvertGamePointToMoney(queryPlayer.MerchantID, item.CurrentAmount, convertRate);
                    decimal updateAmount = ConvertGamePointToMoney(queryPlayer.MerchantID, item.UpdateAmount, convertRate);
                    decimal payoutAmount = ConvertGamePointToMoney(queryPlayer.MerchantID, item.PayoutAmount, convertRate);

                    //以往pgs自製遊戲傳過來的payoutAmount是尚未扣除taxAmount。
                    //為了讓pgs遊戲跟aggLive有一致邏輯的taxAmount。調整為DB紀錄的payoutAmount是已經扣除taxAmount了
                    //但需要把關pgs遊戲服務器 不要再調整此邏輯
                    if (taxAmount > 0 && payoutAmount > 0 && model.PerGameLog.GameID >= 2000 && model.PerGameLog.GameID < 9999)
                    {
                        payoutAmount = payoutAmount - taxAmount.ToAbs();
                    }

                    var insertEntity = new PerGameDetailLogEntity
                    {
                        GameID = model.PerGameLog.GameID,
                        ServerID = model.PerGameLog.ServerID,
                        PlayerID = item.PlayerID,
                        ValidAmount = validAmount,
                        CurrentAmount = currentAmount,
                        UpdateAmount = updateAmount,
                        BetAmount = betAmount,
                        PayoutAmount = payoutAmount,
                        TaxAmount = taxAmount,
                        GameTime = insertGameTime,
                        CreateTime = now,
                        PlayerName = queryPlayer.PlayerName,
                        MerchantID = queryPlayer.PlayerType == (int)PlayerTypeEnum.Robot ? _robotSkipInsertMerchantID: queryPlayer.MerchantID,
                        
                        //有些pgs遊戲 freeSpin的時候 畫面中要要先選一些選項 這種情況會記錄一筆perGame 0元，但是transactionNo會空值
                        TransactionNo = item.TransactionNo,
                    };

                    //根據要不要計算帳務的標籤決定要寫入哪一張表
                    if (isWriteToAccountingIgnore)
                    {
                        detailLogsAccountingIgnore.Add(insertEntity.ReturnInjecteObject<PerGameDetailLogAccountingIgnoreEntity>());
                    }
                    else
                    {
                        detailLogs.Add(insertEntity);
                    }

                    //活動獎勵 (後面處理)
                    if (!isWriteToAccountingIgnore && queryPlayer.PlayerType == PlayerTypeEnum.Normal.ToInteger())
                    {
                        needEventBonusRecords.Add(insertEntity.TransactionNo);
                    }

                    //根據上方的註解 有些情況 transactionNo會空值...
                    var analyzeText = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(item.TransactionNo);

                    if (analyzeText.IsPgsOrderNo)
                    {
                        insertCustomizedItems.Add(new BetRecordCustomizedSettingEntity
                        {
                            SettingKey = BetRecordCustomizedSettingEnum.PgsGame_PerGameLogID.ToInteger(),
                            SettingValue = perGameID.ToString(),
                            CreateTime = now,
                            BetRecordNoPGS = item.TransactionNo,
                            PartitionTime = analyzeText.Time.Value,
                        });

                        insertCustomizedItems.Add(new BetRecordCustomizedSettingEntity
                        {
                            SettingKey = BetRecordCustomizedSettingEnum.PgsGame_TradeID.ToInteger(),
                            SettingValue = item.TradeID.ToString(),
                            CreateTime = now,
                            BetRecordNoPGS = item.TransactionNo,
                            PartitionTime = analyzeText.Time.Value,
                        });

                        if(convertRate != 1)
                        {
                            insertCustomizedItems.Add(new BetRecordCustomizedSettingEntity
                            {
                                SettingKey = BetRecordCustomizedSettingEnum.ConvertRate.ToInteger(),
                                SettingValue = convertRate.ToString(),
                                CreateTime = now,
                                BetRecordNoPGS = item.TransactionNo,
                                PartitionTime = analyzeText.Time.Value,
                            });
                        }
                    }
                }

                foreach (var item in logs_zero)
                {
                    detailLogsZero.Add(new PerGameDetailLogZeroEntity
                    {
                        GameID = model.PerGameLog.GameID,
                        ServerID = model.PerGameLog.ServerID,
                        CreateTime = now,
                        CurrentAmount = item.CurrentAmount,
                        UpdateAmount = item.UpdateAmount,
                        PlayerID = item.PlayerID,
                        TradeID = item.TradeID,
                        ValidAmount = item.ValidAmount,
                        BetAmount = item.BetAmount,
                        PayoutAmount = item.PayoutAmount,
                        GameTime = insertGameTime,
                        MerchantID = 0,
                        PerGameLogID = perGameID,
                    });
                }
            }

            //補上 merchantIDs & playerName 資訊
            SetMerchantIDAndPlayerName(detailLogsZero);

            // 經討論後決定機器人不寫入detailLog資料 (2020/08/31)
            detailLogs = detailLogs.Where(x => x.MerchantID != _robotSkipInsertMerchantID).ToList();
            detailLogsZero = detailLogsZero.Where(x => x.MerchantID != _robotSkipInsertMerchantID).ToList();
            detailLogsAccountingIgnore = detailLogsAccountingIgnore.Where(x => x.MerchantID != _robotSkipInsertMerchantID).ToList();

            //step 2. 寫入detailLog
            PerGameDetailLogRepository.CreateTransactionScope(detailLogs);

            //step 3. 寫入detailLogAccountingIgnore (不計算帳務)
            PerGameDetailLogAccountingIgnoreRepository.CreateMany(detailLogsAccountingIgnore);

            //step 4. 寫入detailLogZero (觀局資料)
            PerGameDetailLogZeroRepository.CreateMany(detailLogsZero);

            //step 5. 寫入customizedSettings
            BetRecordCustomizedSettingRepository.CreateMany(insertCustomizedItems);


            //step 9. 活動獎勵 (先寫死gameID=2230、2232 不可參加活動...節省點效能)
            if(needEventBonusRecords.IsNotEmptyNull() &&
              (queryGame.GameID != (int)GameListEnum.XLHD_新龍虎鬥 && queryGame.GameID != (int)GameListEnum.JZYQ_決戰羽球))
            {
                var EventProcessService = IOCHelper.Resolve<IEventProcessService>();

                foreach (var insertEntity in detailLogs)
                {
                    if(needEventBonusRecords.Contains(insertEntity.TransactionNo))
                    {
                        var eventProcessParam = AppHelper.PerGameConvertToEventProcessModel(insertEntity, insertEntity.GameTime);
                        EventProcessService.WhenSettlement(eventProcessParam);
                    }
                }
            }

            return new PerGameLogViewModel
            {
                PerGameLogID = perGameID
            };
        }



       
        private List<PerGameDetailLogEntity> OldVersionAssignPerGameLogsValues(PerGameLogRequestModel model, long perGameID)
        {
            List<string> splitEachUser = new List<string>();

            if (!string.IsNullOrWhiteSpace(model.PerGameLog.UserGameDetail))
                splitEachUser = model.PerGameLog.UserGameDetail.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            var list = new List<PerGameDetailLogEntity>();

            foreach (var each in splitEachUser)
            {
                var split = each.Split(':');

                int playerID = Int32.Parse(split[0]);
                decimal currentAmount = decimal.Parse(split[1]);
                decimal updateAmount = decimal.Parse(split[2]);
                long tradeID = Int64.Parse(split[3]);

                list.Add(new PerGameDetailLogEntity
                {
                    GameID = model.PerGameLog.GameID,
                    ServerID = model.PerGameLog.ServerID,
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    CurrentAmount = currentAmount,
                    UpdateAmount = updateAmount,
                    PlayerID = playerID,
                    GameTime = model.PerGameLog.GameEndTime,
                    MerchantID = 0,
                });
            }

            return list;
        }


        private void SetMerchantIDAndPlayerName<T>(List<T> models) where T : IPlayerRowDataColumn
        {
            List<int> playerIDs = models.Select(x => x.PlayerID).ToList();

            if (playerIDs.IsEmptyOrNull())
                return;

            List<PlayerEntity> queryPlayers = GetPlayersByCache(playerIDs);

            //如果完全查不到players 直接拋exception
            if (queryPlayers.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            foreach (var item in models)
            {
                var match = queryPlayers.FirstOrDefault(x => x.PlayerID == item.PlayerID);

                if (match != null)
                {
                    //若判斷此筆資料是機器人
                    if (match.PlayerType == (int)PlayerTypeEnum.Robot)
                    {
                        //這邊填 -1 後面會做些處理
                        item.MerchantID = _robotSkipInsertMerchantID;
                    }
                    else
                    {
                        item.MerchantID = match.MerchantID;
                        item.PlayerName = match.PlayerName;
                    }
                }
            }
        }


        private (List<PlayerEntity> Players, Dictionary<int, decimal> MerchantConvertRate) GetPlayersAndMerchantConvertRate(IEnumerable<int> playerIDs)
        {
            List<int> myPlayerIDs = playerIDs.Distinct().ToList();

            Dictionary<int, decimal> merchantConvertRateDic = new Dictionary<int, decimal>();

            if (myPlayerIDs.IsEmptyOrNull())
                return (new List<PlayerEntity>(), merchantConvertRateDic);

            List<PlayerEntity> queryPlayers = GetPlayersByCache(myPlayerIDs);

            List<int> merchantIDs = queryPlayers.Select(x => x.MerchantID).Distinct().ToList();

            foreach (var merchantID in merchantIDs)
            {
                var myRate = GetMerchantConvertRate_PGSGame(merchantID);
                merchantConvertRateDic.Add(merchantID, myRate);
            }

            return (queryPlayers, merchantConvertRateDic);
        }



        /// <summary>
        /// 如果只有一筆資料要查，使用cache查 (slot應該是這種)
        /// 減少DB存取
        /// </summary>
        private List<PlayerEntity> GetPlayersByCache(List<int> playerIDs)
        {
            if (playerIDs.IsEmptyOrNull())
                return new List<PlayerEntity>();

            playerIDs = playerIDs.Distinct().ToList();

            bool isSingle = playerIDs.Count() == 1;

            if (isSingle)
            {
                var querySingle = PlayerRepository.GetSingleByCache(playerIDs.FirstOrDefault());
                return new List<PlayerEntity>
                {
                    querySingle
                };
            }
            else
            {
                string selectColumns = " PlayerID , PlayerName , PlayerType , MerchantID ";
                var queryPlayers = PlayerRepository.GetByIds(playerIDs , selectColumns);
                return queryPlayers;
            }

        }

        private void SendErrorMsgAndNotify(bool isSingleWallet, BetRecordEntity queryBetRecord, Exception ex)
        {
            string extraMsg = isSingleWallet ? "此商戶為單錢包商戶, 後半段callback可能沒通知到" : "";
            string msg = $"PGS自製遊戲結算更新失敗 單號:{queryBetRecord.BetRecordNoPGS} {queryBetRecord.MerchantID} {extraMsg}" + Environment.NewLine;
            msg += $"投注: {queryBetRecord.BetAmount} 派獎:{queryBetRecord.PayoutAmount} 狀態可能卡在Pending中、建議參考 EazyTempValue 或 執行手動結算" + Environment.NewLine;
            msg += ex.GetMessageAndErrorLine();

            SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_important_error);

            EazyTempValueRepository.CreateOrQueueRetry(new EazyTempValueEntity
            {
                UUID = Guid.NewGuid(),
                BetRecordNoPGS = queryBetRecord.BetRecordNoPGS,
                GameID = queryBetRecord.GameID,
                Amount1 = queryBetRecord.PayoutAmount,
                Memo1 = msg,
                PlayerID = queryBetRecord.PlayerID,
                MerchantID = queryBetRecord.MerchantID,
                Type1 = EazyTempValueTypeEnum.PgsGame_SettlementFail.ToInteger(),
            });
        }


        private void SendNotify(string msg)
        {
            MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
            {
                Channel = SlackChannelEnum.n_api_gameapi,
                Message = msg
            });

            MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
            {
                TelegramChannelEnum = TelegramChannelEnum.pgs_single_wallet_erorr,
                Message = msg
            });
        }



    }
}
