using System;
using System.Collections.Generic;
using target.BL.Interface;
using target.BL.Services;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Models.Models;
using System.Linq;
using target.DA.Repository;
using target.Models;
using target.Utility;
using target.Models.Enums;
using target.Models.ViewModels.Base;
using target.Models.ViewModels.GameVendor;
using target.Utility.Utility.ThirdParty;
using Lck.Utility.Extensions.Helper;
using target.Utility.Helper.Dependency;

namespace target.BL.Services
{

    public partial class FakeWalletMockService : BaseEventService, IFakeWalletMockService
    {

        public static Dictionary<int, decimal> FakePlayerWallet = new Dictionary<int, decimal>();

        public SingleWalletMerchantSessionTokenRepository SingleWalletMerchantSessionTokenRepository { get; set; }

        public SingleWalletMerchantSessionTokenExtraInfoRepository SingleWalletMerchantSessionTokenExtraInfoRepository { get; set; }

        public PlayerWalletFakeDataRepository PlayerWalletFakeDataRepository { get; set; }

        private static decimal _defaultWalletAmount = 88888;

        private static FakeWalletTypeEnum _defaultWalletType = FakeWalletTypeEnum.CommonFakeWallet;


        public (int PlayerID, string SessionTokenPGS) GetPlayerIDByPlatfromSessionToken(string sessionToken)
        {
            var query = SingleWalletMerchantSessionTokenRepository.GetByFilter(new QuerySingleWalletMerchantSessionTokenRequestViewModel
                {
                    SessionTokenRaw = sessionToken
                })
                .FirstOrDefault();

            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            return (query.PlayerID, query.SessionTokenPGS);
        }


        public (int PlayerID, string SessionTokenPGS) GetPlayerIDByW88Token(string w88Token)
        {
            var query = SingleWalletMerchantSessionTokenExtraInfoRepository.GetByW88Token(w88Token);

            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            return (query.PlayerID, query.SessionTokenPGS);
        }

        /// <summary>
        /// **重要 目前的做法在loadbalance架構下 會出現有多種錢包金額的問題 (除非想辦法用redis或真實DB解決此問題)
        /// </summary>
        public decimal GetFakeWalletBalance(int playerID)
        {
            var param = new RetryLoopParameter
            {
                DelayMilliseconds = 100,
                ExceptionNotifyLevel = LoopExceptionNotifyLevelEnum.Never,
                MaxTimes = 2,
                Tag = "GetFakeWalletBalance"
            };

            PlayerWalletFakeDataEntity queryWallet = null;

            AppHelper.ExecuteMethodAndRetryMaxTimes(param, (myParam) =>
               {
                   queryWallet = PlayerWalletFakeDataRepository.GetByPlayerIDAndType(playerID, _defaultWalletType);

                   if (queryWallet == null)
                   {
                       queryWallet = new PlayerWalletFakeDataEntity
                       {
                           PlayerID = playerID,
                           Type = _defaultWalletType.ToInteger(),
                           Amount = _defaultWalletAmount,
                       };

                       PlayerWalletFakeDataRepository.Create(queryWallet);
                   }

                   return true;
               });

            return queryWallet.Amount;
        }


        public decimal UpdateFakeWalletAmount(int playerID, decimal updateAmount)
        {
            //呼叫
            var currentAmount = GetFakeWalletBalance(playerID);

            if (updateAmount < 0)
            {
                if (updateAmount.ToAbs() > currentAmount)
                {
                    throw new TerproException(ApiErrorCode.BetFail_WalletAmountNotEnough);
                }
            }

            //更新錢包
            PlayerWalletFakeDataRepository.UpdateWalletAmount(playerID, _defaultWalletType, updateAmount);

            var afterAmount = GetFakeWalletBalance(playerID);
            return afterAmount;
        }
    }
}
