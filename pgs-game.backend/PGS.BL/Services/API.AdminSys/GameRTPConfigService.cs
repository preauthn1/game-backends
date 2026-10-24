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
using target.Models.ViewModels.API.AdminSys;
using Microsoft.Extensions.Caching.Memory;
using target.Models.Attributes;
using target.Utility.Utility.ThirdParty;
using target.Models.ViewModels.GameVendor;
using System.Threading;
using target.Utility;
using target.Utility.Extensions.Web;

namespace target.BL.Services
{

    public class GameRTPConfigService : BaseAdminSysService, IGameRTPConfigService
    {

        public GameVendorRTPChangeRecordRepository GameVendorRTPChangeRecordRepository { get; set; }

        public GameRTPConfigRepository GameRTPConfigRepository { get; set; }

        public PlayerRTPConfigRepository PlayerRTPConfigRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public UserOperateRecordRepository UserOperateRecordRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public PlayerGameSummaryRepository PlayerGameSummaryRepository { get; set; }

        public EventRepository EventRepository { get; set; }

        public IPlayerRTPChangeRefService PlayerRTPChangeRefService { get; set; }


        public AGGIntegrationService AGGIntegrationService = new AGGIntegrationService();


        public PagedList<GameRTPConfigViewModel> GetGameRTPByPagingFilter(int userID, QueryGameRTPConfigRequestParameterModel filter)
        {
            filter.GameIDs = filter.GameID.ToIntegerListCollection();
            filter.VendorConfigIDs = filter.VendorConfigID.ToIntegerListCollection();

            var query = GameRTPConfigRepository.GetPagingByFilter(filter);

            var viewModels = ConvertToNewTypePagingObject<GameRTPConfigEntity, GameRTPConfigViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = ConvertToViewModels(x);
                    return convert;
                })
                .ToList();

            return viewModels;
        }


        public PagedList<PlayerRTPConfigViewModel> GetPlayerRTPByPagingFilter(int userID, QueryPlayerRTPConfigRequestParameterModel filter)
        {
            var query = PlayerRTPConfigRepository.GetPagingByFilter(filter);

            var viewModels = ConvertToNewTypePagingObject<PlayerRTPConfigEntity, PlayerRTPConfigViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = ConvertToPlayerRTPViewModels(x);
                    return convert;
                })
                .ToList();

            return viewModels;
        }

        public void BatchUpdatePlayerRTP(int userID, UpdatePlayerRTPRequestModel model)
        {
            model.RuleFor(z => z.GameIDs).Rule_IsNotEmptyCollection();

            CheckViewPlayerPermission(userID, model.PlayerID);

            foreach (var gameID in model.GameIDs)
            {
                Thread.Sleep(100);

                model.GameID = gameID;

                PlayerGameSummaryRepository.CreateDefaultGameSummaryData(model.PlayerID, model.GameID);

                UpdatePlayerRTP(userID , model);
            }
        }

        public void UpdatePlayerRTP(int userID, UpdatePlayerRTPRequestModel model)
        {
            model.RuleFor(z => z.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.GameID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.RTP).Rule_NotMinus();

            CheckViewPlayerPermission(userID, model.PlayerID);

            var queryPlayer = PlayerRepository.GetSingleByCache(model.PlayerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            var queryGame = GameListRepository.GetSingleByCache(model.GameID);

            if (queryGame == null)
                throw new TerproException(ApiErrorCode.NotFoundGame);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            DateTime now = DateTimeHelper.GetCurrentTime();
            string debug = $"userID:{userID}_PlayerID:{model.PlayerID}_GameID:{model.GameID}_{now.ToString("yyyyMMddHHmmss")}";

            if (model.IsReset)
            {
                PlayerRTPChangeRefService.ResetPlayerGameRTP(new PlayerRTPResetRequestModel
                {
                    GameID = model.GameID,
                    PlayerID = model.PlayerID,
                    Operator = currentUser.UserName,
                    DebugReferenceUUID = debug
                });
            }
            else
            {
                var queryVendorConfig = VendorConfigRepository.GetByFilter(new QueryVendorConfigRequestViewModel
                {
                    MerchantIDs = queryPlayer.MerchantID.ToListCollection(),
                    VendorIDs = queryGame.GameVendorType.ToListCollection()
                })
                    .FirstOrDefault();

                if (queryVendorConfig == null)
                {
                    throw new TerproException(ApiErrorCode.NotFoundData);
                }

                //檢查RTP合法
                CheckIsValidRTPValue(queryVendorConfig.VendorConfigID, model.GameID, (int)model.RTP);

                PlayerRTPChangeRefService.UpdatePlayerGameRTP(new PlayerRTPChangeRequestModel
                {
                    GameID = model.GameID,
                    PlayerID = model.PlayerID,
                    RTP = model.RTP,
                    Operator = currentUser.UserName,
                    DebugReferenceUUID = debug
                });
            }


            //寫一筆操作紀錄
            var log = InstanceOperateLog(userID, UserOperateTypeEnum.UpdatePlayerRTP, model.PlayerID);
            log.DebugInfo = $"{model.ConvertToJsonString()}";

            WriteOperateLog(log);


            //TODO 需要驗證下 如果本來已經是99了 可以直接改98嗎 還是說必須要先reset
            //TODO 需要驗證下 如果本來已經是99了 可以直接改98嗎 還是說必須要先reset
            //TODO 需要驗證下 如果本來已經是99了 可以直接改98嗎 還是說必須要先reset
            //TODO 需要驗證下 如果本來已經是99了 可以直接改98嗎 還是說必須要先reset
            //TODO 需要驗證下 如果本來已經是99了 可以直接改98嗎 還是說必須要先reset
            //TODO 需要驗證下 如果本來已經是99了 可以直接改98嗎 還是說必須要先reset


            return;
        }


        public void ResetPlayerRTPByTargetValue(int userID, ResetAllPlayerRTPRequestModel model)
        {
            model.RuleFor(z => z.TargetRTPValue).Rule_NotZeroNotMinus();

            var user = GetCurrentOwnPermissionByCache(userID);

            var query = PlayerRTPConfigRepository.GetByFilter(new QueryPlayerRTPConfigRequestViewModel
            {
                RTPValue = model.TargetRTPValue
            });

            if (query.IsEmptyOrNull())
                return;

            var now = DateTimeHelper.GetCurrentTime();
            string debug = $"{userID}_{now.ToString("yyyyMMddHHmmss")}";

            //寫一筆操作紀錄
            var log = InstanceOperateLog(userID, UserOperateTypeEnum.ResetLargePlayersRTP, 0);
            log.DebugInfo = $"{model.ConvertToJsonString()}";

            WriteOperateLog(log);


            foreach (var item in query)
            {
                PlayerRTPChangeRefService.ResetPlayerGameRTP(new PlayerRTPResetRequestModel
                {
                    PlayerID = item.PlayerID,
                    GameID = item.GameID,
                    Operator = user.UserName,
                    DebugReferenceUUID = debug,
                });
            }

            return;
        }


        private ThirdPartySlotsGameVendorComplexService InstanceGameVendorExtraService(int gameID)
        {
            var gameVendor = AppHelper.GetGameVendorByGameID(gameID);
            var service = ServiceInstanceHelper.InstanceSlotsGameVendorService((GameVendorTypeEnum)gameVendor);
            return service;
        }


        private void CheckIsValidRTPValue(int vendorConfigID, int gameID, int targetRTPValue)
        {
            var gameVendorService = InstanceGameVendorExtraService(gameID);
            var apiResultRTPList = gameVendorService.GetAvailableRTPSettings(new Models.ViewModels.GameVendor.AGGGetGameRTPSettingReqeustParameter
            {
                VendorConfigID = vendorConfigID,
                GameID = gameID
            });

            if (apiResultRTPList.ResultObject == null)
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError);
            }

            if (!apiResultRTPList.ResultObject.AvailableRTPs.Contains(targetRTPValue))
            {
                throw new TerproException(ApiErrorCode.Forbidden_NotValidDefineValue, new ErrorDetailModel("RTP"));
            }
        }




        public void UpdateGameRTP(int userID, GameVendorRTPUpdateRequestModel model)
        {
            model.RuleFor(z => z.VendorConfigID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.GameID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.RTP).Rule_NotZeroNotMinus();

            var queryUser = GetCurrentOwnPermissionByCache(userID);

            //檢查RTP合法
            CheckIsValidRTPValue(model.VendorConfigID, model.GameID, model.RTP);

            var gameVendorService = InstanceGameVendorExtraService(model.GameID);
            var updateRsponse = gameVendorService.UpdateRTPSettings(new AGGUpdateRTPSettingReqeustParameter
            {
                VendorConfigID = model.VendorConfigID,
                GameID = model.GameID,
                RTP = model.RTP,
                IsAutoThrowException = true
            });

            //稍微等待一下對方的API
            Thread.Sleep(200);

            //重新讀一次API設定 (理論上數字就會變動了)
            GetRTPByApiAndUpdateDBValue(model.VendorConfigID, model.GameID);


            //寫異動紀錄
            GameVendorRTPChangeRecordRepository.Create(new GameVendorRTPChangeRecordEntity
            {
                CreateTime = DateTimeHelper.GetCurrentTime(),
                RTP = model.RTP,
                GameID = model.GameID,
                Type = GameVendorRTPChangeTypeEnum.UpdateGameRTP.ToInteger(),
                VendorConfigID = model.VendorConfigID,
                CreateBy = queryUser.UserName,
                DebugInfo = updateRsponse.ResponseText,
            });


            //寫操作紀錄
            var log = InstanceOperateLog(userID, UserOperateTypeEnum.UpdateGameRTP, model.GameID);
            log.DebugInfo = $"model:{model.ConvertToJsonString()}";
            log.Memo = model.Memo;

            WriteOperateLog(log);

            return;
        }


        public void UpdateGameWinLimit(int userID, GameVendorRTPUpdateRequestModel model)
        {
            model.RuleFor(z => z.VendorConfigID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.GameID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.WinLimit).Rule_NotNull();

            if (model.WinLimit < -1)
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberValueTooSmall);

            var queryUser = GetCurrentOwnPermissionByCache(userID);

            var gameVendorService = InstanceGameVendorExtraService(model.GameID);
            var updateRsponse = gameVendorService.UpdateRTPSettings(new AGGUpdateRTPSettingReqeustParameter
            {
                VendorConfigID = model.VendorConfigID,
                GameID = model.GameID,
                WinLimit = model.WinLimit,
                IsAutoThrowException = true
            });

            //稍微等待一下對方的API
            Thread.Sleep(200);

            //重新讀一次API設定 (理論上數字就會變動了)
            GetRTPByApiAndUpdateDBValue(model.VendorConfigID, model.GameID);

            //寫操作紀錄
            var log = InstanceOperateLog(userID, UserOperateTypeEnum.UpdateGameRTP, model.GameID);
            log.DebugInfo = $"model:{model.ConvertToJsonString()}";
            log.Memo = model.Memo;

            WriteOperateLog(log);

            return;
        }


        public string FetchGameOptionRawResponse(int userID , GameVendorGameOptionInfoRequestModel model)
        {
            var gameVendorService = InstanceGameVendorExtraService(model.GameID);
            var updateRsponse = gameVendorService.GetRTPSettings(new AGGGetGameRTPSettingReqeustParameter
            {
                VendorConfigID = model.VendorConfigID,
                GameID = model.GameID,
                IsAutoThrowException = true
            });

            return updateRsponse.ResponseText;
        }


        public void SyncAGGAllRTPSettings(int userID , LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.VendorConfigIDs).Rule_IsNotEmptyCollection();

            var queryVendorConfigs = VendorConfigRepository.GetByFilter(new QueryVendorConfigRequestViewModel
            {
                VendorIDs = SystemConstant.SlotsExtensionApiSupportVendors,
                VendorConfigIDs = model.VendorConfigIDs,
            });

            if(queryVendorConfigs.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.NotFoundVendorConfig);
            }

            foreach (var item in queryVendorConfigs)
            {
                SyncRTPSettingByApi(userID, new GameVendorRTPSyncRequestModel
                {
                    VendorConfigID = item.VendorConfigID,
                    GameVendorID = item.VendorID,
                    SpecificGameIDs = model.GameIDs
                });
            }
        }


        public void SyncRTPSettingByApi(int userID, GameVendorRTPSyncRequestModel model)
        {
            model.RuleFor(z => z.VendorConfigID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.GameVendorID).Rule_NotZeroNotMinus();

            int vendorConfigID = model.VendorConfigID;

            var myFilter = new QueryGameListRequestViewModel
            {
                GameVendorTypes = model.GameVendorID.ToListCollection(),
                Status = NormalActiveStatus.Enable.ToInteger()
            };

            var queryTargetGames = GameListRepository.GetByFilter(myFilter);

            queryTargetGames = queryTargetGames
                .Where(x => !AppHelper.IsLobbyGameID(x.GameID))
                .ToList();

            if (model.SpecificGameIDs.IsNotEmptyNull())
            {
                queryTargetGames = queryTargetGames
                    .Where(x => model.SpecificGameIDs.Contains(x.GameID))
                    .ToList();
            }

            foreach (var item in queryTargetGames)
            {
                try
                {
                    GetRTPByApiAndUpdateDBValue(vendorConfigID, item.GameID);
                }
                catch(Exception ex)
                {
                    string msg = $"vendorCofigID: {vendorConfigID} {item.GameID} {ex.GetMessageAndErrorLine()}";
                    SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_adminSys_api);
                }
            }

            return;
        }


        private void GetRTPByApiAndUpdateDBValue(int vendorConfigID, int gameID)
        {
            var gameVendorService = InstanceGameVendorExtraService(gameID);

            var apiResult = gameVendorService.GetRTPSettings(new Models.ViewModels.GameVendor.AGGGetGameRTPSettingReqeustParameter
            {
                VendorConfigID = vendorConfigID,
                GameID = gameID
            });

            if (apiResult.ResultObject == null || apiResult.ResultObject.rtp <= 0)
            {
                var queryGame = GameListRepository.GetSingleByCache(gameID);

                string msg = $"{gameID} {queryGame.ThirdPartyGameCode} vendorConfigID:`{vendorConfigID}` GetRTPSettings 異常 {Environment.NewLine}{apiResult.ResponseText}";
                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Message = msg,
                    Channel = SlackChannelEnum.n_error_adminsys
                });

                return;
            }

            var apiResultRTPList = gameVendorService.GetAvailableRTPSettings(new Models.ViewModels.GameVendor.AGGGetGameRTPSettingReqeustParameter
            {
                VendorConfigID = vendorConfigID,
                GameID = gameID
            });

            var queryExistConfig = GameRTPConfigRepository.GetByFilter(new QueryGameRTPConfigRequestViewModel
            {
                GameIDs = gameID.ToListCollection(),
                VendorConfigIDs = vendorConfigID.ToListCollection()

            }).FirstOrDefault();

            DateTime now = DateTimeHelper.GetCurrentTime();

            string availableRTPs = string.Join(",", apiResultRTPList.ResultObject?.AvailableRTPs);

            if (queryExistConfig == null)
            {
                GameRTPConfigRepository.Create(new GameRTPConfigEntity
                {
                    CreateTime = now,
                    LastUpdateTime = now,
                    GameID = gameID,
                    VendorConfigID = vendorConfigID,
                    WinLimit = apiResult.ResultObject.winLimit,
                    DebugInfo = apiResult.ResponseText,
                    RTP = apiResult.ResultObject.rtp,
                    AvailableRTP = availableRTPs
                });
            }
            else
            {
                queryExistConfig.RTP = apiResult.ResultObject.rtp;
                queryExistConfig.WinLimit = apiResult.ResultObject.winLimit;
                queryExistConfig.AvailableRTP = availableRTPs;
                queryExistConfig.LastUpdateTime = now;
                queryExistConfig.DebugInfo = apiResult.ResponseText;

                GameRTPConfigRepository.Update(queryExistConfig);
            }
        }

        private GameRTPConfigViewModel ConvertToViewModels(GameRTPConfigEntity query)
        {
            var convert = query.ReturnInjecteObject<GameRTPConfigViewModel>();
            convert.GameName = DbMultiLanguageTextHelper.GetGameDisplayName(query.GameID);
            convert.VendorConfigName = VendorConfigRepository.GetSingleByCache(query.VendorConfigID)?.Name;

            // -1 代表默認不限制
            if (!query.WinLimit.HasValue)
            {
                convert.WinLimit = -1;
            }

            return convert;
        }


        private PlayerRTPConfigViewModel ConvertToPlayerRTPViewModels(PlayerRTPConfigEntity query)
        {
            var convert = query.ReturnInjecteObject<PlayerRTPConfigViewModel>();
            convert.GameName = DbMultiLanguageTextHelper.GetGameDisplayName(query.GameID);
            return convert;
        }

    }
}
