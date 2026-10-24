using target.Core.Repository;
using target.Models.Paging;
using target.Utility.IOC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility.ThirdParty;
using Lck.Utility.Helper;
using target.Utility;
using target.Models.ViewModels;
using target.Models.Enums;
using Lck.Utility.Extensions.Helper;
using target.Models;
using target.Models.Models;
using target.DA.Repository;

namespace target.BL.Services
{
    public class BaseGameVendorCallbackService : BaseService
    {

        public MerchantRepository MerchantRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }

        public MerchantGameSettingRepository MerchantGameSettingRepository { get; set; }

        

        protected MerchantValidateInfoCacheModel GetMerchantValidateInfoByMemoryCache(int merchantID, GameVendorTypeEnum gameVendor)
        {
            int vendorID = gameVendor.ToInteger();

            string cacheKey = CacheKey.ExternalAPI_MerchantValidateInfo(merchantID, vendorID);

            HeaderValueHelper.AppendLogInfoUserIdentity(merchantID);

            int cacheTimeMinutes = 5;

            var queryCacheValue = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                var queryMerchant = MerchantRepository.GetBriefById(merchantID);

                if (queryMerchant == null)
                    return null;

                var internalMerchant = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.IsInternalTestMerchant);

                var restrictMode = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.GameSetting_IsRestrictTokenMode);

                var convertRate = GetMerchantConvertRate(merchantID, gameVendor);

                var queryVendorConfig = VendorConfigRepository.GetByFilter(new QueryVendorConfigRequestViewModel
                {
                    VendorIDs = vendorID.ToListCollection(),
                    MerchantIDs = merchantID.ToListCollection()
                })
                .FirstOrDefault();

                var queryEnableGames = MerchantGameSettingRepository.GetByFilter(new QueryMerchantGameSettingRequestViewModel
                {
                    MerchantIDs = merchantID.ToListCollection(),
                    Status = NormalActiveStatus.Enable.ToInteger(),
                });

                return new MerchantValidateInfoCacheModel
                {
                    //IsUnderMaintain = false, //後面的程式碼另外查詢
                    IsInternalTestMerchant = internalMerchant.SettingValueOfBoolean,
                    IsRestrictGameTokenMode = restrictMode.SettingValueOfBoolean,
                    MerchantID = queryMerchant.MerchantID,
                    MerchantStatus = queryMerchant.Status,
                    WalletMode = queryMerchant.WalletMode,
                    ConvertRate = convertRate.ConvertRate,
                    Currency = queryMerchant.Currency,
                    VendorConfigID = queryVendorConfig.VendorConfigID,
                    EnableGameIDs = queryEnableGames.Select(x => x.GameID).ToList()
                };

            }, cacheTimeMinutes);

            if (queryCacheValue == null)
                throw new TerproException(ApiErrorCode.NotFoundMerchant);

            if (queryCacheValue.MerchantStatus != NormalActiveStatus.Enable.ToInteger())
                throw new TerproException(ApiErrorCode.InvalidStatus_DisableNow);

            queryCacheValue.EnableGameIDs = queryCacheValue.EnableGameIDs ?? new List<int>();
            queryCacheValue.EnableGameIDs.Add(SystemConstant.Mario_GameID_Gameble); //mario一種特殊機制遊戲(非正常遊玩的遊戲、而是一種特殊機制)
            return queryCacheValue;
        }


        protected (bool IsDefine, decimal ConvertRate) GetMerchantConvertRate(int merchantID, GameVendorTypeEnum gameVendor)
        {
            var settingKeyResult = AppHelper.GetConvertRateCustomizedSettingKey(gameVendor);

            if (!settingKeyResult.IsDefine)
                return (false, 1.0M);

            decimal rate = MerchantCustomizedSettingRepository.GetMerchantGamePointConvertRateByCache(merchantID, settingKeyResult.SettingKey);
            return (true, rate);
        }


        protected decimal ConvertMoneyToGamePoint(decimal money, decimal convertRate)
        {
            return money * convertRate;
        }


    }
}
