using DocumentFormat.OpenXml.EMMA;
using Lck.Utility.Extensions.Helper;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace target.BL.Services
{

    public class PlayerRTPChangeRefService : BaseEventService, IPlayerRTPChangeRefService
    {


        public GroupRTPConfigRepository GroupRTPConfigRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public PlayerRTPConfigRepository PlayerRTPConfigRepository { get; set; }

        public GameVendorRTPChangeRecordRepository GameVendorRTPChangeRecordRepository { get; set; }


        /// <summary>
        /// 根據商戶配置決定是否調整RTP
        /// </summary>
        public PlayerRTPChangeResultModel WhenEnterGameChangeRTP(PlayerRTPChangeWhenEnterGameParameter model)
        {
            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();

            int gameVendorID = AppHelper.GetGameVendorByGameID(model.GameID);

            var defaultResult = new PlayerRTPChangeResultModel();

            if (gameVendorID != (int)GameVendorTypeEnum.AGG)
                return defaultResult;

            var querySetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(model.MerchantID, MerchantCustomizedSettingKeyEnum.GameSetting_RTPConfig_GroupID);

            if (querySetting.SettingValueOfInt == 0)
                return defaultResult;

            int groupID = querySetting.SettingValueOfInt;

            var queryRTPSettings = GroupRTPConfigRepository.GetByFilter(new QueryGroupRTPConfigRequestViewModel
            {
                SettingGroupIDs = groupID.ToListCollection(),
            });

            if (queryRTPSettings.IsEmptyOrNull())
                return defaultResult;

            //優先使用指定gameID的配置
            var matchGameSetting = queryRTPSettings.Where(x=> x.GameID == model.GameID).FirstOrDefault();

            if(matchGameSetting == null)
            {
                // -1 代表預設RTP配置
                matchGameSetting = queryRTPSettings.Where(x => x.GameID == -1).FirstOrDefault();
            }

            //如果還是查無資料
            if (matchGameSetting == null)
                return defaultResult;

            var rtpResult = new PlayerRTPChangeResultModel
            {
                NewRTP = matchGameSetting.RTP
            };

            if (IsRTPAlreadyChanged(model.PlayerID, model.GameID, matchGameSetting.RTP))
                return rtpResult;

            UpdatePlayerGameRTP(new PlayerRTPChangeRequestModel
            {
                GameID = model.GameID,
                PlayerID = model.PlayerID,
                RTP = matchGameSetting.RTP,
                Operator = "system",
            });

            return rtpResult;
        }




        public void UpdatePlayerGameRTP(PlayerRTPChangeRequestModel model)
        {
            model.RuleFor(z => z.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.GameID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.RTP).Rule_NotZeroNotMinus();

            if (!IsSupportRTPGame(model.GameID))
                return;

            model.Operator = model.Operator ?? "system";
            model.DebugReferenceUUID = model.DebugReferenceUUID ?? Guid.NewGuid().ToString();

            DateTime now = DateTimeHelper.GetCurrentTime();

            var loopParam = new RetryLoopParameter
            {
                MaxTimes = 3,  //最多retry三次
                DelayMilliseconds = 5 * 1000,
                Tag = "UpdatePlayerGameRTP"
            };

            AppHelper.ExecuteMethodAndRetryMaxTimes(loopParam, (param) =>
            {
                ApiResultCheckModel apiResult = CallApiToUpdateRTP(model);

                if (apiResult.IsSuccess)
                {
                    var queryExist = PlayerRTPConfigRepository.GetByPlayerAndGameID(model.PlayerID, model.GameID);

                    string beforeValueText = queryExist == null ? "NULL" : queryExist.RTP.ToString();

                    if (queryExist == null)
                    {
                        PlayerRTPConfigRepository.Create(new PlayerRTPConfigEntity
                        {
                            PlayerID = model.PlayerID,
                            GameID = model.GameID,
                            IsRiskControl = false,
                            RTP = model.RTP,
                            UpdateBy = model.Operator,
                            UpdateTime = now
                        });
                    }
                    else
                    {
                        queryExist.RTP = model.RTP;
                        queryExist.UpdateTime = now;
                        queryExist.UpdateBy = model.Operator;

                        PlayerRTPConfigRepository.Update(queryExist);
                    }

                    GameVendorRTPChangeRecordRepository.Create(new GameVendorRTPChangeRecordEntity
                    {
                        CreateTime = now,
                        CreateBy = model.Operator,
                        GameID = model.GameID,
                        PlayerID = model.PlayerID,
                        RTP = model.RTP,
                        VendorConfigID = 0,
                        DebugInfo = "DebugReferenceUUID:" + model.DebugReferenceUUID + ", BeforeValue:" + beforeValueText,
                        Type = GameVendorRTPChangeTypeEnum.UpdatePlayerRTP.ToInteger()
                    });
                    return true;
                }
                else
                {
                    //第N次retry還是不成功的話..
                    if(param.LoopTimes >= loopParam.MaxTimes)
                    {
                        string msg = $"UpdatePlayerRTPSettings似乎失敗 {param.LoopTimes} 次了" + apiResult.Message + $"Player:{model.PlayerID}, 請求識別碼DebugUUID: {model.DebugReferenceUUID}";
                        SendMessageToMultipleChannel(msg);
                    }

                    return false;
                }
            });
        }



        public void ResetPlayerAllGameRTP(PlayerRTPResetAllGameRequestModel model)
        {
            model.RuleFor(z => z.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.GameIDs).Rule_IsNotEmptyCollection();

            var filter = new QueryPlayerRTPConfigRequestViewModel
            {
                PlayerIDs = model.PlayerID.ToListCollection(),
                GameIDs = model.GameIDs,
                IsRiskControl = false
            };

            //如果有指定要刪除風控 就不篩選此欄位
            if (model.IsDeleteRiskControl)
            {
                filter.IsRiskControl = null;
            }

            //先查出db有異動記錄的
            var queryExistPlayerRTPConfig = PlayerRTPConfigRepository.GetByFilter(filter);


            if (queryExistPlayerRTPConfig.IsEmptyOrNull())
                return;

            foreach (var item in queryExistPlayerRTPConfig)
            {
                ResetPlayerGameRTP(new PlayerRTPResetRequestModel
                {
                    PlayerID = model.PlayerID,
                    GameID = item.GameID,
                    DebugReferenceUUID = model.DebugReferenceUUID
                });
            }
        }



        public void ResetPlayerGameRTP(PlayerRTPResetRequestModel model)
        {
            model.RuleFor(z => z.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.GameID).Rule_NotZeroNotMinus();

            if (!IsSupportRTPGame(model.GameID))
                return;

            model.Operator = model.Operator ?? "system";
            model.DebugReferenceUUID = model.DebugReferenceUUID ?? $"{Guid.NewGuid().ToString()}_{model.PlayerID}";

            var queryPlayerRTPConfig = PlayerRTPConfigRepository.GetByPlayerAndGameID(model.PlayerID, model.GameID);

            if (queryPlayerRTPConfig == null)
                return;

            DateTime now = DateTimeHelper.GetCurrentTime();

            model.Memo = model.Memo.Truncate(50);
            model.Operator = model.Operator.Truncate(50);

            var loopParam = new RetryLoopParameter
            {
                MaxTimes = 3,  //最多retry三次
                DelayMilliseconds = 5 * 1000,
                Tag = "ResetPlayerGameRTP"
            };

            AppHelper.ExecuteMethodAndRetryMaxTimes(loopParam, (param) =>
            {
                ApiResultCheckModel apiResult = CallApiToResetRTP(model);

                if (apiResult.IsSuccess)
                {
                    //清除資料
                    if (queryPlayerRTPConfig != null)
                    {
                        PlayerRTPConfigRepository.Delete(queryPlayerRTPConfig.Id);
                    }

                    GameVendorRTPChangeRecordRepository.Create(new GameVendorRTPChangeRecordEntity
                    {
                        CreateTime = now,
                        CreateBy = model.Operator,
                        GameID = model.GameID,
                        PlayerID = model.PlayerID,
                        RTP = -1,
                        VendorConfigID = 0,
                        Memo = model.Memo,
                        DebugInfo = "DebugReferenceUUID:" + model.DebugReferenceUUID,
                        Type = GameVendorRTPChangeTypeEnum.ResetPlayerRTP.ToInteger()
                    });

                    return true;
                }
                else
                {
                    if(param.LoopTimes >= loopParam.MaxTimes)
                    {
                        string msg = $"ResetPlayerGameRTP 似乎失敗 {param.LoopTimes} 次了" + apiResult.Message + $",請求識別碼DebugUUID: {model.DebugReferenceUUID}";
                        SendMessageToMultipleChannel(msg);
                    }

                    GameVendorRTPChangeRecordRepository.Create(new GameVendorRTPChangeRecordEntity
                    {
                        CreateTime = now,
                        CreateBy = model.Operator,
                        GameID = model.GameID,
                        PlayerID = model.PlayerID,
                        RTP = -1,
                        VendorConfigID = 0,
                        Memo = model.Memo,
                        DebugInfo = "DebugReferenceUUID:" + model.DebugReferenceUUID,
                        Type = GameVendorRTPChangeTypeEnum.CallApiFail.ToInteger()
                    });

                    return false;
                }
            });
        }


        private bool IsSupportRTPGame(int gameID)
        {
            int gameVendor = AppHelper.GetGameVendorByGameID(gameID);

            return GameVendorDefinitionHelper.IsSupportRTPConfig(gameVendor);
        }


        protected virtual ApiResultCheckModel CallApiToUpdateRTP(PlayerRTPChangeRequestModel model)
        {
            var gameVendorService = InstanceGameVendorExtraService(model.GameID);
            return gameVendorService.UpdatePlayerRTPSettings(new AGGUpdatePlayerRTPSettingReqeustParameter
            {
                PlayerID = model.PlayerID,
                GameID = model.GameID,
                RTP = (int)model.RTP,
                DebugReferenceUUID = model.DebugReferenceUUID,
                IsAutoThrowException = false
            });
        }

        protected virtual ApiResultCheckModel CallApiToResetRTP(PlayerRTPResetRequestModel model)
        {
            var gameVendorService = InstanceGameVendorExtraService(model.GameID);
            return gameVendorService.ResetPlayerRTPSettings(new AGGGetPlayerRTPSettingReqeustParameter
            {
                PlayerID = model.PlayerID,
                GameID = model.GameID,
                DebugReferenceUUID = model.DebugReferenceUUID,
                IsAutoThrowException = false
            });
        }

        private ThirdPartySlotsGameVendorComplexService InstanceGameVendorExtraService(int gameID)
        {
            var gameVendor = AppHelper.GetGameVendorByGameID(gameID);
            var service = ServiceInstanceHelper.InstanceSlotsGameVendorService((GameVendorTypeEnum)gameVendor);
            return service;
        }


        /// <summary>
        /// 檢查是否已經改過RTP了 (數值需相同)
        /// </summary>
        private bool IsRTPAlreadyChanged(int playerID, int gameID, decimal rtp)
        {
            var queryExist = PlayerRTPConfigRepository.GetByPlayerAndGameID(playerID, gameID);

            if (queryExist == null)
                return false;

            if (queryExist.RTP == rtp)
                return true;

            return false;
        }

    }
}
