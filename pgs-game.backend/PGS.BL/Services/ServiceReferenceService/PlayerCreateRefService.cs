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
using target.Utility;

namespace target.BL.Services
{

    public class PlayerCreateRefService : BaseService, IPlayerCreateRefService
    {
        public PlayerRepository PlayerRepository { get; set; }

        public PlayerFakeCreateRecordRepository PlayerFakeCreateRecordRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }


        public int CreatePlayerOrReturnPlayerID(PlayerCreateParameterModel model)
        {
            var queryPlayer = PlayerRepository.GetByPlayerNameAndMerchantID(model.PlayerName, model.MerchantID);

            //資料已存在 回應playerID
            if (queryPlayer != null)
            {
                if(model.IsCheckAccountStatus && queryPlayer.Status != NormalActiveStatus.Enable.ToInteger())
                {
                    throw new TerproException(ApiErrorCode.InvalidStatus_AccountStatus);
                }

                return queryPlayer.PlayerID;
            }

            //資料不存在 呼叫 create
            var result = CreatePlayer(model);
            return result.PlayerID;
        }


        /// <summary>
        /// 商戶主動請求新建帳號
        /// </summary>
        public int CreatePlayerForMerchantRequest(PlayerCreateParameterModel model)
        {
            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.PlayerName).Rule_IsValidAccountFormat();

            string selectColumns = " PlayerID , PlayerName , Status "; //輕量化搜尋
            var queryPlayer = PlayerRepository.GetByPlayerNameAndMerchantID(model.PlayerName, model.MerchantID, selectColumns);

            //資料已存在
            if (queryPlayer != null)
            {
                if(model.IsSkipExceptionWhenDataDuplicate)
                {
                    return queryPlayer.PlayerID;
                }
                else
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);
                }
            }

            //針對濫用API的商戶(或大量幽靈帳號) 會標記並採用此流程 減少players垃圾資料
            if (IsForbiddenToCreateAccountByVendorAPI(model.MerchantID))
            {
                PlayerFakeCreateRecordRepository.CreateTryCatch(new PlayerFakeCreateRecordEntity
                {
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    MerchantID = model.MerchantID,
                    PlayerName = model.PlayerName
                });

                return -1;
            }

            var result = CreatePlayer(model);
            return result.PlayerID;
        }


        public PlayerCreateResultModel CreatePlayer(PlayerCreateParameterModel model)
        {
            DateTime now = DateTimeHelper.GetCurrentTime();

            var newPlayer = new PlayerEntity
            {
                PlayerName = model.PlayerName,
                MerchantID = model.MerchantID,
                CreateTime = now,
                PlayerType = PlayerTypeEnum.Normal.ToInteger(),
                Currency = model.Currency,
                Status = NormalActiveStatus.Enable.ToInteger(),
                FaceID = RandomHelper.GetRandIntegerNumbers(1, 16), //隨機頭像
            };

            decimal defaultAmount = 0;

            //查出是否有設置 預設金幣(基本上是for Dev、QA環境用的)
            var queryDefaultBalanceSetting = MerchantCustomizedSettingRepository.GetMerchantSettingKeyWithoutCache(model.MerchantID, MerchantCustomizedSettingKeyEnum.DefaultWalletBalance);

            if (queryDefaultBalanceSetting != null)
            {
                defaultAmount = queryDefaultBalanceSetting.SettingValueOfDecimal;
            }

            try
            {
                var result = PlayerRepository.CreatePlayerAndWallet(newPlayer, defaultAmount);
                return new PlayerCreateResultModel
                {
                    PlayerID = result.ResultId,
                    DefaultBalance = defaultAmount
                };
            }
            catch (Exception ex)
            {
                if (ex.IsSqlUniqueException())
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);
                }
                else
                {
                    throw;
                }
            }
        }

        private bool IsForbiddenToCreateAccountByVendorAPI(int merchantID)
        {
            var forbiddenSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.System_GameVendorForbiddenUseApiToCreateAccount);

            if (forbiddenSetting.SettingValueOfInt == NormalActiveStatus.Enable.ToInteger())
            {
                return true;
            }

            return false;
        }

    }
}
