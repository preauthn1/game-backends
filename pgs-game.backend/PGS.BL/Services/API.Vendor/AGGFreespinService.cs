using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using target.BL.Interface;
using target.DA.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.BL.Services
{

    /// <summary>
    /// AGG/IDG API轉換層
    /// </summary>
    public class AGGFreespinService : BaseVendorService, IAGGFreespinService
    {

        public AGGIntegrationService AGGIntegrationService = new AGGIntegrationService();

        public GameListRepository GameListRepository { get; set; }

        public AGGFreespinCampaignRepository AGGFreespinCampaignRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public AGGFreespinAMPromotionConfigRepository AGGFreespinAMPromotionConfigRepository { get; set; }


        public static int _GameVendor = GameVendorTypeEnum.AGG.ToInteger();

        public IPlayerCreateRefService PlayerCreateRefService { get; set; }



        public AggGameInfoResponseViewModel FreespinSpin_GetBetSetting(AggGameInfoReqeustParameter param)
        {
            var queryMerchant = GetMerchantAndCheckIsValid(param.MerchantID, param.MerchantSecretKey);

            CheckIsVendorAPIMaintain();

            var queryMyGames = GetSupportFreespinAGGGames(param.MerchantID);

            var apiResult = AGGIntegrationService.GetGameBettingInfo(new AGGGameBettingInfoReqeustParameter
            {
                MerchantID = param.MerchantID,
            });

            if (apiResult == null || apiResult.integratorBets.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError);

            List<GameBettingInfoDetailItem> gameItems = new List<GameBettingInfoDetailItem>();

            var apiGames = apiResult.integratorBets.SelectMany(z => z.gameBetRangeList).ToList();

            foreach (var item in queryMyGames)
            {
                var matchSetting = apiGames.FirstOrDefault(z => z.gameId == item.ThirdPartyGameCode);

                var instance = new GameBettingInfoDetailItem
                {
                    GameID = item.GameID,
                    GameName = matchSetting.gameName,
                    BetRanges = new List<decimal>(),
                };

                if (matchSetting != null)
                {
                    instance.BetRanges = matchSetting.betRange;
                }

                gameItems.Add(instance);
            }

            return new AggGameInfoResponseViewModel
            {
                MerchantID = queryMerchant.MerchantID,
                Currency = queryMerchant.Currency,
                GameInfoList = gameItems
            };
        }


        public PagedList<AggFreespinListDetailItemModel> FreespinSpin_List(VendorAPIFreespinListReqeustParameter model)
        {
            model.RuleFor(z => z.StartTime).Rule_NotNullDateTime();
            model.RuleFor(z => z.EndTime).Rule_NotNullDateTime();
            model.RuleFor(z => z.StartTime).Rule_StartTimeMustSmallThanEndTime(model.EndTime);
            model.RuleFor(z => z.PageIndex).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.PageSize).Rule_NotZeroNotMinus();

            var queryMerchant = GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            CheckIsVendorAPIMaintain();

            var apiResult = AGGIntegrationService.Freespin_GetList(new AGGGameFreespinListReqeustParameter
            {
                MerchantID = model.MerchantID,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                FreespinID = model.FreespinID,
                PageIndex = model.PageIndex,
                PageSize = model.PageSize
            });

            var detailItems = new List<AggFreespinListDetailItemModel>();

            if (apiResult.freeSpinCampaigns.IsNotEmptyNull())
            {
                var queryAGGGames = GetSupportFreespinAGGGames();

                foreach (var item in apiResult.freeSpinCampaigns)
                {
                    var instance = new AggFreespinListDetailItemModel
                    {
                        FreeSpinId = item.freeSpinId,
                        Rounds = item.rounds,
                        GameSettings = new List<AGGFreespinGameListItemViewModel>(),
                        CreateTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(item.createDate),
                        StartTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(item.startDate),
                        EndTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(item.endDate),
                        UseEndTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(item.useEndDate),
                        ModifyTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(item.modifyDate),
                    };

                    foreach (var eachGame in item.gameList)
                    {
                        var matchGame = queryAGGGames.FirstOrDefault(z => z.ThirdPartyGameCode == eachGame.GameID);

                        if (matchGame == null)
                            continue;

                        if (eachGame.Bet == 0)
                            continue;

                        instance.GameSettings.Add(new AGGFreespinGameListItemViewModel
                        {
                            GameID = matchGame?.GameID ?? 0,
                            Bet = eachGame.Bet,
                        });
                    }

                    detailItems.Add(instance);
                }
            }

            var resultViewModel = new PagedList<AggFreespinListDetailItemModel>
            {
                Items = detailItems,
                PageIndex = model.PageIndex,
                PageSize = model.PageSize,
                TotalCount = apiResult.total,
                TotalPage = (int)Math.Ceiling((decimal)apiResult.total / model.PageSize),
            };

            return resultViewModel;
        }


        public AggFreespinSetPromotionQuotaResponseViewModel FreespinSpin_Create_ByFreeQuota(VendorAPIFreespinSetPromotionQuotaReqeustParameter model)
        {
            model.RuleFor(z => z.Players).Rule_NotEmptyNull();
            model.RuleFor(z => z.GameIDs).Rule_NotEmptyNull();
            model.RuleFor(z => z.Rounds).Rule_NotZeroNotMinus();

            var queryMerchant = GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            CheckIsVendorAPIMaintain();

            bool isDevMerchant = queryMerchant.Type != (int)MerchantTypeEnum.Plaform_ProductionEnvironment;

            var gameSettingsByAPI = FreespinSpin_GetBetSetting(new AggGameInfoReqeustParameter
            {
                MerchantID = model.MerchantID,
                MerchantSecretKey = model.MerchantSecretKey
            });
          
            VendorAPIFreespinSetReqeustParameter param = new VendorAPIFreespinSetReqeustParameter
            {
                MerchantID = model.MerchantID,
                MerchantSecretKey = model.MerchantSecretKey,
                Players = model.Players,
                Rounds = model.Rounds,
                GameSettings = new List<AGGFreespinGameListItemViewModel>(),
                IsAutoCreateNonExistPlayer = model.IsAutoCreateNonExistPlayer
            };

            foreach (var gameID in model.GameIDs)
            {
                var matchSetting = gameSettingsByAPI.GameInfoList.Where(x => x.GameID == gameID).FirstOrDefault();

                if (matchSetting == null)
                {
                    throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel($"GameID: {gameID} no support"));
                }

                param.GameSettings.Add(new AGGFreespinGameListItemViewModel
                {
                    GameID = gameID,
                    Bet = matchSetting.BetRanges
                                .Select(x => x)
                                .OrderBy(x => x)
                                .FirstOrDefault()
                });
            }

            //測試環境直接寫死某些設定
            if (isDevMerchant)
            {
                var now = DateTimeHelper.GetCurrentTime();

                param.StartTime = new DateTime(2026, 4, 1);
                param.EndTime = now.AddDays(30);
                param.UseEndTime = now.AddDays(30);               

                var resultOfDev = FreespinSpin_Set(param);
                return new AggFreespinSetPromotionQuotaResponseViewModel
                {
                    FreespinID = resultOfDev.FreespinID,
                    RemainAmount = 9999999999,
                    RemainRounds = 9999999999,
                    UseEndTime = param.UseEndTime
                };
            }

            var querySetting = AGGFreespinAMPromotionConfigRepository.GetByFilter(new QueryAGGFreespinAMPromotionConfigRequestViewModel
            {
                Type = FreespinAMPromotionType.AMEvent_1st.ToInteger(),
                MerchantIDs = model.MerchantID.ToListCollection()
            })
            .FirstOrDefault();

            if (querySetting == null)
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("PromotionConfig not found"));
            }

            var myBudgetFilter = new VendorAPIFreespinBudgetListReqeustParameter
            {
                MerchantID = model.MerchantID,
                MerchantSecretKey = model.MerchantSecretKey,
                BudgetId = querySetting.ReferBudgetID
            };

            //檢查是否有對應的配置
            var queryExistBudget = FreespinSpin_Budget_List(myBudgetFilter).Items.FirstOrDefault();

            if (queryExistBudget == null)
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("Budget not setup"));
            }

            if (queryExistBudget.RemainRounds < model.Rounds)
            {
                throw new TerproException(ApiErrorCode.Forbidden_AmountNotEnough, new ErrorDetailModel("RemainRounds"));
            }

            if (queryExistBudget.RemainAmount <= 0)
            {
                throw new TerproException(ApiErrorCode.Forbidden_AmountNotEnough, new ErrorDetailModel("RemainAmount"));
            }

            var now_2 = DateTimeHelper.GetCurrentTime();

            if (querySetting.EventEndTime < now_2)
            {
                throw new TerproException(ApiErrorCode.Forbidden_EventHasEnd);
            }

            param.BudgetId = queryExistBudget?.BudgetId;
            param.StartTime = querySetting.EventStartTime;
            param.EndTime = querySetting.EventEndTime;
            param.UseEndTime = querySetting.EventEndTime;

            //AGG那邊有限制startTime要大於現在 笑死
            if(querySetting.EventStartTime < now_2)
            {
                param.StartTime = now_2.AddMinutes(1);
            }
        
            //if (model.GameSettings.IsEmptyOrNull())
            //{
            //    param.GameSettings = gameSettingsByAPI
            //        .GameInfoList.Select(x =>
            //        {
            //            var instance = new AGGFreespinGameListItemViewModel
            //            {
            //                GameID = x.GameID,
            //                Bet = x.BetRanges
            //                    .Select(value => new
            //                    {
            //                        diff = (value - querySetting.DefaultBetAmount).ToAbs(),
            //                        amount = value,
            //                    })
            //                    .OrderBy(x => x.diff)
            //                    .Select(x => x.amount)
            //                    .FirstOrDefault()
            //            };

            //            return instance;
            //        })
            //        .ToList();
            //}

            //創建freespin
            var result = FreespinSpin_Set(param);

            //檢查是否有對應的配置
            var queryExistBudgetAgain = FreespinSpin_Budget_List(myBudgetFilter).Items.FirstOrDefault();

            return new AggFreespinSetPromotionQuotaResponseViewModel
            {
                FreespinID = result.FreespinID,
                RemainAmount = queryExistBudgetAgain.RemainAmount,
                RemainRounds = queryExistBudgetAgain.RemainRounds,
                UseEndTime = param.UseEndTime
            };
        }


        public AggFreespinSetResponseViewModel FreespinSpin_Set(VendorAPIFreespinSetReqeustParameter model)
        {
            model.RuleFor(z => z.StartTime).Rule_NotEmptyDateTime();
            model.RuleFor(z => z.EndTime).Rule_NotEmptyDateTime();
            model.RuleFor(z => z.StartTime).Rule_StartTimeMustSmallThanEndTime(model.EndTime);
            model.RuleFor(z => z.UseEndTime).Rule_NotEmptyDateTime();
            model.RuleFor(z => z.Players).Rule_NotEmptyNull();
            model.RuleFor(z => z.Rounds).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.GameSettings).Rule_NotEmptyNull();

            int maxArraySize = 1000;

            if (model.Players.CountOrZero() > maxArraySize)
            {
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel($"Players max size {maxArraySize}"));
            }

            var queryMerchant = GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            CheckIsVendorAPIMaintain();

            if (model.BudgetId.IsNotEmptyTextNotNull())
            {
                CheckBudgetViewPermissionByAPI(model.MerchantID, model.BudgetId);
            }

            //if(model.FreeSpinId.IsNotEmptyTextNotNull())
            //{
            //    QueryFreespinAndCheckOwner(model.FreeSpinId, model.MerchantID);
            //}

            List<int> gameIDs = model.GameSettings.Select(x => x.GameID).ToList();

            var queryGames = GetSupportFreespinAGGGames();

            var queryGameSettings = FreespinSpin_GetBetSetting(new AggGameInfoReqeustParameter
            {
                MerchantID = model.MerchantID,
                MerchantSecretKey = model.MerchantSecretKey
            });

            foreach (var item in model.GameSettings)
            {
                int gameID = item.GameID;

                var matchGame = queryGames.FirstOrDefault(z => z.GameID == gameID);

                if (matchGame == null)
                {
                    throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel($"GameID: {gameID}"));
                }

                var matchSetting = queryGameSettings.GameInfoList.Where(x => x.GameID == item.GameID).FirstOrDefault();

                if (matchSetting == null)
                {
                    throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel($"GameID: {gameID} no match bet-settings"));
                }

                if (!matchSetting.BetRanges.Contains(item.Bet))
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel($"GameID: {gameID} Bet amount invalid"));
                }

                item.ThirdGameID = matchGame.ThirdPartyGameCode;
            }

            List<int> playerIds = new List<int>();

            var queryPlayers = PlayerRepository.GetByLargeAccounts(model.MerchantID, model.Players);

            playerIds = queryPlayers.Select(z => z.PlayerID).ToList();


            if(playerIds.CountOrZero() != model.Players.CountOrZero())
            {
                List<int> newPlayerIDs = new List<int>();

                foreach(string p_account in model.Players)
                {
                    var match = queryPlayers.FirstOrDefault(z => z.PlayerName == p_account);

                    //如果有指定自動創建不存在的玩家
                    if (match == null && model.IsAutoCreateNonExistPlayer)
                    {
                        int newPlayerID = PlayerCreateRefService.CreatePlayerOrReturnPlayerID(new PlayerCreateParameterModel
                        {
                            Currency = queryMerchant.Currency,
                            MerchantID = queryMerchant.MerchantID,
                            PlayerName = p_account,
                        });

                        newPlayerIDs.Add(newPlayerID);
                    }
                    else
                    {
                        throw new TerproException(ApiErrorCode.NotFoundPlayer , new ErrorDetailModel($"playerAccount: {p_account}"));
                    }
                }

                if(newPlayerIDs.IsNotEmptyNull())
                {
                    playerIds.AddRange(newPlayerIDs);
                }
            }

            if (playerIds.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            AGGFreespinSetAPIResponseModel apiResult = null;

            var apiParam = new AGGGameFreespinSetReqeustParameter
            {
                MerchantID = model.MerchantID,
                PlayerIDs = playerIds,
                VendorAPIParam = model,
            };

            string newCreatedFreespinID = string.Empty;

            try
            {
                apiResult = AGGIntegrationService.Freespin_Set(apiParam);
                newCreatedFreespinID = apiResult.freeSpinCampaign?.freeSpinId;
            }
            catch (Exception ex)
            {
                newCreatedFreespinID = RecheckFailResponseFreespin(apiParam);

                if (string.IsNullOrWhiteSpace(newCreatedFreespinID))
                    throw;
            }

            if (string.IsNullOrWhiteSpace(newCreatedFreespinID))
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError);

            AGGFreespinCampaignRepository.CreateOrUpdate(new AGGFreespinCampaignEntity
            {
                FreespinID = apiResult.freeSpinCampaign?.freeSpinId,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                CreateTime = DateTimeHelper.GetCurrentTime(),
                MerchantID = model.MerchantID,
            });

            return new AggFreespinSetResponseViewModel
            {
                FreespinID = apiResult.freeSpinCampaign?.freeSpinId
            };
        }

        private string RecheckFailResponseFreespin(AGGGameFreespinSetReqeustParameter apiParam)
        {
            var queryExist = AGGIntegrationService.Freespin_GetList(new AGGGameFreespinListReqeustParameter
            {
                MerchantID = apiParam.MerchantID,
                StartTime = apiParam.VendorAPIParam.StartTime.AddMinutes(-1),
                EndTime = apiParam.VendorAPIParam.EndTime.AddMinutes(1),
                PageIndex = 1,
                PageSize = 5000,
            });

            var top3 = queryExist.freeSpinCampaigns
                .OrderByDescending(x => ExtensionOfDatetime.ISO8601_To_UtcDateTime(x.createDate))
                .Take(3)
                .ToList();

            List<string> freespinIds = top3.Select(x => x.freeSpinId).ToList();

            var queryMyDB = AGGFreespinCampaignRepository.GetByFilter(new QueryAGGFreespinCampaignRequestViewModel
            {
                FreespinIDList = freespinIds
            });

            string newCreatedFreespinID = string.Empty;

            int merchantID = apiParam.VendorAPIParam.MerchantID;

            foreach (var api_item in top3)
            {
                var matchDB = queryMyDB.FirstOrDefault(z => z.FreespinID == api_item.freeSpinId);

                //資料補救
                if (matchDB == null && api_item.clientId == merchantID.ToString())
                {
                    newCreatedFreespinID = api_item.freeSpinId;
                    AGGFreespinCampaignRepository.CreateOrUpdate(new AGGFreespinCampaignEntity
                    {
                        FreespinID = api_item.freeSpinId,
                        StartTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(api_item.startDate),
                        EndTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(api_item.endDate),
                        CreateTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(api_item.createDate),
                        MerchantID = apiParam.VendorAPIParam.MerchantID,
                    });
                }
            }

            return newCreatedFreespinID;
        }

        public AggFreespinSetResponseViewModel FreespinSpin_Disable(VendorAPIFreespinDisableReqeustParameter model)
        {
            model.RuleFor(x => x.FreeSpinId).Rule_NotEmptyText();

            var queryMerchant = GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            CheckIsVendorAPIMaintain();

            QueryFreespinAndCheckOwner(model.FreeSpinId, model.MerchantID);

            var result = AGGIntegrationService.Freespin_Disable(new AGGAPIFreespinDisableReqeustParameter
            {
                FreeSpinID = model.FreeSpinId,
                MerchantID = model.MerchantID
            });

            return new AggFreespinSetResponseViewModel
            {
                FreespinID = model.FreeSpinId
            };
        }

        public PagedList<AggFreespinStatusItemViewModel> FreespinSpin_Status(VendorAPIFreespinStatusReqeustParameter model)
        {
            model.RuleFor(z => z.StartTime).Rule_NotEmptyDateTime();
            model.RuleFor(z => z.EndTime).Rule_NotEmptyDateTime();
            model.RuleFor(z => z.StartTime).Rule_StartTimeMustSmallThanEndTime(model.EndTime);
            model.RuleFor(z => z.PageIndex).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.PageSize).Rule_NotZeroNotMinus();


            var queryMerchant = GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            CheckIsVendorAPIMaintain();

            if (model.FreeSpinID.IsNotEmptyTextNotNull())
            {
                QueryFreespinAndCheckOwner(model.FreeSpinID, model.MerchantID);
            }

            var result = AGGIntegrationService.Freespin_Status(new AGGAPIFreespinStatusReqeustParameter
            {
                VendorAPIParam = model,
                MerchantID = model.MerchantID,
            });

            var viewModel = new PagedList<AggFreespinStatusItemViewModel>
            {
                Items = new List<AggFreespinStatusItemViewModel>(),
                PageIndex = model.PageIndex,
                PageSize = model.PageSize,
                TotalCount = result.total,
                TotalPage = (int)Math.Ceiling((decimal)result.total / model.PageSize),
            };

            if (result.freeSpinStatuses.IsNotEmptyNull())
            {
                List<int> playerIDs = result.freeSpinStatuses.Select(z => Int32.Parse(z.UserId)).ToList();

                var playerDic = PlayerRepository.GetByIds(playerIDs).ToDictionary(z => z.PlayerID.ToString(), z => z);

                foreach (var item in result.freeSpinStatuses)
                {
                    string playerIDText = item.UserId;
                    var instance = item.ReturnInjecteObject<AggFreespinStatusItemViewModel>();
                    instance.CreateTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(item.CreateDate);
                    instance.ModifyTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(item.ModifyDate);
                    instance.TotalPayout = item.TotalWin;

                    if (playerDic.ContainsKey(playerIDText))
                    {
                        instance.PlayerID = playerDic[playerIDText].PlayerID;
                        instance.PlayerName = playerDic[playerIDText].PlayerName;
                    }

                    viewModel.Items.Add(instance);
                }
            }

            return viewModel;
        }

        public AggFreespinRecordResultViewModel FreespinSpin_Record(VendorAPIFreespinRecordReqeustParameter model)
        {
            model.RuleFor(z => z.StartTime).Rule_NotEmptyDateTime();
            model.RuleFor(z => z.EndTime).Rule_NotEmptyDateTime();
            model.RuleFor(z => z.StartTime).Rule_StartTimeMustSmallThanEndTime(model.EndTime);
            model.RuleFor(z => z.PageIndex).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.PageSize).Rule_NotZeroNotMinus();

            if (model.FreeSpinID.IsNotEmptyTextNotNull())
            {
                QueryFreespinAndCheckOwner(model.FreeSpinID, model.MerchantID);
            }

            var queryMerchant = GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            CheckIsVendorAPIMaintain();

            var result = AGGIntegrationService.Freespin_Record(new AGGAPIFreespinRecordRequestParameter
            {
                MerchantID = model.MerchantID,
                VendorAPIParam = model,
            });

            return new AggFreespinRecordResultViewModel
            {

            };
        }

        public AggFreespinSummaryResultViewModel FreespinSpin_Summary(VendorAPIFreespinSummaryReqeustParameter model)
        {
            model.RuleFor(z => z.StartTime).Rule_NotEmptyDateTime();
            model.RuleFor(z => z.EndTime).Rule_NotEmptyDateTime();
            model.RuleFor(z => z.StartTime).Rule_StartTimeMustSmallThanEndTime(model.EndTime);

            var queryMerchant = GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            CheckIsVendorAPIMaintain();

            var result = AGGIntegrationService.Freespin_Summary(new AGGAPIFreespinSummaryRequestParameter
            {
                MerchantID = model.MerchantID,
                VendorAPIParam = model
            });

            var viewModel = new AggFreespinSummaryResultViewModel
            {
                Items = new List<AggFreespinSummaryDetailItemViewModel>(),
            };

            if (result.freeSpinStatisticals.IsNotEmptyNull())
            {
                var queryAGGGames = GetSupportFreespinAGGGames();

                foreach (var item in result.freeSpinStatisticals)
                {
                    var instance = item.ReturnInjecteObject<AggFreespinSummaryDetailItemViewModel>();
                    instance.TotalPayout = item.TotalWin;

                    var matchGame = queryAGGGames.FirstOrDefault(z => z.ThirdPartyGameCode == item.GameId);
                    instance.GameID = matchGame.GameID;

                    viewModel.Items.Add(instance);
                }
            }

            return viewModel;
        }


        public VendorAPIFreespinBudgetListResultViewModel FreespinSpin_Budget_List([FromBody] VendorAPIFreespinBudgetListReqeustParameter model)
        {
            var queryMerchant = GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            CheckIsVendorAPIMaintain();

            var result = AGGIntegrationService.FreespinSpin_Budget_List(new AGGAPIFreespinBudgetListRequestParameter
            {
                MerchantID = model.MerchantID,
                VendorAPIParam = model
            });

            var viewModel = new VendorAPIFreespinBudgetListResultViewModel
            {
                Items = new List<AGGAPIFreespinBudgetDetailItemModel>(),
            };

            if (result.FreeSpinBudgets.IsNotEmptyNull())
            {
                string myMerchantIDText = model.MerchantID.ToString();

                foreach (var item in result.FreeSpinBudgets)
                {
                    if (item.ClientId != myMerchantIDText)
                        continue;

                    var instance = item.ReturnInjecteObject<AGGAPIFreespinBudgetDetailItemModel>();
                    instance.CreateTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(item.CreatedTime);

                    viewModel.Items.Add(instance);
                }
            }

            return viewModel;
        }

        public VendorAPIFreespinBudgetSetResultViewModel FreespinSpin_BudgetCreate_ByAdmin([FromBody] VendorAPIFreespinBudgetAdminCreateReqeustParameter model)
        {
            model.RuleFor(x => x.EventStartTime).Rule_NotEmptyDateTime();
            model.RuleFor(x => x.EventEndTime).Rule_NotEmptyDateTime();
            model.RuleFor(x => x.DefaultBetAmount).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.TotalBudgetAmount).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.TotalBudgetRounds).Rule_NotZeroNotMinus();

            var queryMerchant = GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            CheckIsVendorAPIMaintain();

            if (SettingHelper.IsProduction)
            {
                if (!IPHelper.IsCompanyIP())
                {
                    throw new TerproException(ApiErrorCode.Forbidden_IPAddressNotAllow);
                }
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                model.Name = $"第一波_{model.MerchantID}_{DateTimeHelper.GetCurrentTime().ToString("yyyyMMdd")}";
            }

            var queryExistConfig = AGGFreespinAMPromotionConfigRepository.GetByFilter(new QueryAGGFreespinAMPromotionConfigRequestViewModel
            {
                MerchantIDs = model.MerchantID.ToListCollection(),
                Type = FreespinAMPromotionType.AMEvent_1st.ToInteger(),
            })
            .FirstOrDefault();

            if (queryExistConfig != null)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);

            var result = FreespinSpin_Budget_Set(new VendorAPIFreespinBudgetSetReqeustParameter
            {
                MerchantID = model.MerchantID,
                MerchantSecretKey = model.MerchantSecretKey,
                Name = model.Name,
                Enable = true,
            });

            if (string.IsNullOrWhiteSpace(result.BudgetId))
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_SeamlessAPIResponse_ParsingFail, new ErrorDetailModel("BudgetId null"));

            AGGFreespinAMPromotionConfigRepository.Create(new AGGFreespinAMPromotionConfigEntity
            {
                MerchantID = model.MerchantID,
                Name = model.Name,
                DefaultBetAmount = model.DefaultBetAmount,
                EventStartTime = model.EventStartTime,
                EventEndTime = model.EventEndTime,
                Type = FreespinAMPromotionType.AMEvent_1st.ToInteger(),
                ReferBudgetID = result.BudgetId,
            });

            if (model.TotalBudgetAmount > 0)
            {
                FreespinSpin_Budget_Deposit(new VendorAPIFreespinBudgetDepositReqeustParameter
                {
                    MerchantID = model.MerchantID,
                    MerchantSecretKey = model.MerchantSecretKey,
                    BudgetId = result.BudgetId,
                    Amount = model.TotalBudgetAmount,
                    Rounds = model.TotalBudgetRounds
                });
            }

            return result;
        }


        public VendorAPIFreespinBudgetSetResultViewModel FreespinSpin_Budget_Set([FromBody] VendorAPIFreespinBudgetSetReqeustParameter model)
        {
            model.RuleFor(x => x.Name).Rule_NotEmptyText();

            var queryMerchant = GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            CheckIsVendorAPIMaintain();

            CheckBudgetViewPermissionByAPI(model.MerchantID, model.BudgetId);

            var result = AGGIntegrationService.FreespinSpin_Budget_Set(new AGGAPIFreespinBudgetSetRequestParameter
            {
                MerchantID = model.MerchantID,
                VendorAPIParam = model
            });

            return new VendorAPIFreespinBudgetSetResultViewModel
            {
                BudgetId = result.FreeSpinBudget?.BudgetId
            };
        }

        public VendorAPIFreespinBudgetWalletUpdateResultViewModel FreespinSpin_Budget_Deposit([FromBody] VendorAPIFreespinBudgetDepositReqeustParameter model)
        {
            model.RuleFor(x => x.BudgetId).Rule_NotEmptyText();
            model.RuleFor(x => x.Amount).Rule_NotMinus();
            model.RuleFor(x => x.Rounds).Rule_NotMinus();

            if (model.Amount == 0 && model.Rounds == 0)
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("Amount or rounds should not 0"));

            var queryMerchant = GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            CheckIsVendorAPIMaintain();

            CheckBudgetViewPermissionByAPI(model.MerchantID, model.BudgetId);

            var result = AGGIntegrationService.FreespinSpin_Budget_Deposit(new AGGAPIFreespinBudgetDepositRequestParameter
            {
                MerchantID = model.MerchantID,
                VendorAPIParam = model
            });

            return new VendorAPIFreespinBudgetWalletUpdateResultViewModel
            {
                RemainAmount = result.FreeSpinBudgetWallet.remainAmount,
                RemainRounds = result.FreeSpinBudgetWallet.remainRounds,
            };
        }

        public VendorAPIFreespinBudgetWalletUpdateResultViewModel FreespinSpin_Budget_Withdrawal([FromBody] VendorAPIFreespinBudgetWithdrawalReqeustParameter model)
        {
            model.RuleFor(x => x.BudgetId).Rule_NotEmptyText();
            model.RuleFor(x => x.Amount).Rule_NotMinus();
            model.RuleFor(x => x.Rounds).Rule_NotMinus();

            if (model.Amount == 0 && model.Rounds == 0)
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("Amount or rounds should not 0"));

            var queryMerchant = GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            CheckIsVendorAPIMaintain();

            CheckBudgetViewPermissionByAPI(model.MerchantID, model.BudgetId);

            var result = AGGIntegrationService.FreespinSpin_Budget_Withdrawal(new AGGAPIFreespinBudgetWithdrawalRequestParameter
            {
                MerchantID = model.MerchantID,
                VendorAPIParam = model
            });

            return new VendorAPIFreespinBudgetWalletUpdateResultViewModel
            {
                RemainAmount = result.FreeSpinBudgetWallet.remainAmount,
                RemainRounds = result.FreeSpinBudgetWallet.remainRounds,
            };
        }


        private AGGFreespinCampaignEntity QueryFreespinAndCheckOwner(string freespinID, int myMerchantID)
        {
            if (string.IsNullOrWhiteSpace(freespinID))
                return null;

            var query = AGGFreespinCampaignRepository.GetByFilter(new QueryAGGFreespinCampaignRequestViewModel
            {
                FreespinID = freespinID
            })
            .FirstOrDefault();

            if (query == null)
            {
                int days = 14;

                var queryByAPI = AGGIntegrationService.Freespin_GetList(new AGGGameFreespinListReqeustParameter
                {
                    MerchantID = myMerchantID,
                    FreespinID = freespinID,
                    StartTime = DateTimeHelper.GetCurrentTime().AddDays(-1 * days),
                    EndTime = DateTimeHelper.GetCurrentTime().AddDays(days),
                    PageIndex = 1,
                    PageSize = 5
                })
                .freeSpinCampaigns.FirstOrDefault();

                if (queryByAPI == null)
                {
                    throw new TerproException(ApiErrorCode.NotFoundData, new ErrorDetailModel($"FreespinID: {freespinID}"));
                }

                query = new AGGFreespinCampaignEntity
                {
                    MerchantID = Int32.Parse(queryByAPI.clientId),
                    FreespinID = queryByAPI.freeSpinId
                };
            }

            if (query.MerchantID != myMerchantID)
                throw new TerproException(ApiErrorCode.Forbidden_WatchDataPermissionNotAllow);

            return query;
        }


        public List<GameListEntity> GetSupportFreespinAGGGames(int? merchantID = null)
        {
            var myFilter = new QueryGameListRequestViewModel
            {
                IsNotDelete = true,
                GameVendorTypes = _GameVendor.ToListCollection(),
                Status = NormalActiveStatus.Enable.ToInteger(),
                FilterGameCutomizedSettingKey = GameListCustomizedSettingKeyEnum.IsSupportAGGFreespin.ToInteger(),
                FilterGameCutomizedSettingValue = "1",
            };

            if (merchantID.HasValue)
            {
                myFilter.AssignMerchantIDs = merchantID.Value.ToListCollection();
            }

            var queryAGGGames = GameListRepository.GetByFilter(myFilter);
            return queryAGGGames;
        }

        private void CheckBudgetViewPermissionByAPI(int merchantID, string budgetID)
        {
            if (string.IsNullOrWhiteSpace(budgetID))
                return;

            var result = AGGIntegrationService.FreespinSpin_Budget_List(new AGGAPIFreespinBudgetListRequestParameter
            {
                MerchantID = merchantID,
                VendorAPIParam = new VendorAPIFreespinBudgetListReqeustParameter
                {
                    BudgetId = budgetID
                }
            });

            var match = result.FreeSpinBudgets.FirstOrDefault();

            if (match == null || match.ClientId != merchantID.ToString())
                throw new TerproException(ApiErrorCode.NotFoundData, new ErrorDetailModel($"BudgetId: {budgetID}"));
        }

    }

}