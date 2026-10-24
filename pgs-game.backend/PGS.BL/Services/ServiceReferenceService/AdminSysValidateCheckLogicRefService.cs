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
using Lck.Utility.Extensions.Helper;
using target.Web.Modules.Filter;
using target.Utility;
using target.Utility.Extensions.Web;

namespace target.BL.Services
{

    public class AdminSysValidateCheckLogicRefService : BaseAdminSysService, IAdminSysValidateCheckLogicRefService
    {
        public GameListRepository GameListRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public UserCustomizedSettingRepository UserCustomizedSettingRepository { get; set; }



        public void CheckSingleWalletMerchantNotSupprtGame(int merchantID, List<int> gameIDs)
        {
            var queryMerchant = MerchantRepository.GetSingleByCache(merchantID);

            if (!CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode))
                return;

            this.CheckSingleWalletMerchantNotSupprtGame(merchantID.ToListCollection(), gameIDs);
        }


        public void CheckSingleWalletMerchantNotSupprtGame(List<int> merchantIDs, List<int> gameIDs)
        {
            if (merchantIDs.IsEmptyOrNull())
                return;

            if (gameIDs.IsEmptyOrNull())
                return;

            var queryGames = GameListRepository.GetByIds(gameIDs);

            bool isAllSupportSingleWallet = queryGames.All(x => x.IsSupportSingleWallet);

            if (isAllSupportSingleWallet)
                return;

            var queryMerchants = MerchantRepository.GetByIds(merchantIDs);

            if (queryMerchants.Any(x => CommonLogicHelper.IsSingleWalletMode(x.WalletMode)))
            {
                throw new TerproException(ApiErrorCode.Forbidden_SingleWalletMerchantDoNotSupportGame);
            }
        }


        public void CheckMerchantCurrencyNotSupprtGameAndThrow(List<int> merchantIDs, List<int> gameIDs)
        {
            var gameVendors = gameIDs.Select(gameID => AppHelper.GetGameVendorByGameID(gameID)).Distinct().ToList();

            var queryMerchants = MerchantRepository.GetByIds(merchantIDs);

            List<string> myCurrencyList = queryMerchants.Select(x => x.Currency).Distinct().ToList();

            foreach (var gameVendor in gameVendors)
            {
                foreach (var merchantCurrency in myCurrencyList)
                {
                    CheckGameVendorNotSupprtCurrencyAndThrow(merchantCurrency, gameVendor);
                }
            }
        }

        /// <summary>
        /// 部分商戶不想要有JP的遊戲 關閉
        /// </summary>
        public void CheckMerchantJackpotGameAndThrow(List<int> myMerchantIDs, List<int> gameIDs)
        {
            var queryNoJpMerchants = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
            {
                MatchGeneralTagAndIMChannel = GeneralTagEnum.DoNotEnableJackpotGame.ToInteger()
            })
                .Select(z => z.MerchantID)
                .ToList();

            //這邊只定義 無法關閉JP的遊戲 (如果遊戲端可關閉、則不需要阻擋)
            var queryJpGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                FilterGameCutomizedSettingKey = GameListCustomizedSettingKeyEnum.IsSupportUncloseJackpotFeature.ToInteger(),
                FilterGameCutomizedSettingValue = "1"
            }, " GameID ");

            List<int> invalidGameIDs = queryJpGames.Select(x => x.GameID).ToList();

            List<int> mustCheckGameIds = gameIDs.Where(id => invalidGameIDs.Contains(id)).ToList();

            if (mustCheckGameIds.IsEmptyOrNull())
                return;

            if (queryNoJpMerchants.IsEmptyOrNull())
                return;

            var matchMerchants = myMerchantIDs.Where(id => queryNoJpMerchants.Contains(id)).ToList();

            if (matchMerchants.IsEmptyOrNull())
                return;

            int firstMatch = matchMerchants.FirstOrDefault();

            throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel($"Merchant {firstMatch} Not Support Jackpot Game"));
        }


        public void CheckMerchantOnlySupportIDGVisibleGamesAndThrow(List<int> myMerchantIDs, List<int> gameIDs)
        {
            var notSupportGames = gameIDs.Where(id => !AppHelper.IsIDGVisibleGames(id)).ToList();

            if (notSupportGames.IsEmptyOrNull())
            {
                return;
            }

            var queryRestrictMerchants = MerchantCustomizedSettingRepository.GetBriefByFilter(new QueryMerchantCustomizedSettingRequestViewModel
            {
                MerchantIDs = myMerchantIDs,
                CustomizedSettingKeyIDs = MerchantCustomizedSettingKeyEnum.GameSetting_OnlySupportIDGVisibleGames.ToInteger().ToListCollection(),
                SettingValueMatch = "1",
            });

            if (queryRestrictMerchants.IsNotEmptyNull())
            {
                int firstMerchantID = queryRestrictMerchants.FirstOrDefault().MerchantID;
                int firstGameID = notSupportGames.FirstOrDefault();
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportThisGame, new ErrorDetailModel($"{firstMerchantID} visible game not support gameID: {firstGameID}"));
            }

            return;
        }


        public void CheckGameVendorNotSupprtCurrencyAndThrow(string merchantCurrency, int gameVendor)
        {
            var gameEnum = (GameVendorTypeEnum)gameVendor;

            if (!gameEnum.IsDefinedValue())
            {
                throw new TerproException(ApiErrorCode.Forbidden_NotValidDefineValue);
            }

            var setting = GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(gameVendor);

            if (setting.NotSupportCurrency.IsEmptyOrNull())
            {
                return;
            }
            else
            {
                foreach (var currency in setting.NotSupportCurrency)
                {
                    if (merchantCurrency == currency)
                    {
                        throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel($"{gameVendor} Not Support {currency}"));
                    }
                }
            }
        }


        public List<int> ExcludeSingleWalletMerchantNotSupprtGameIDs(List<int> gameIDs)
        {
            if (gameIDs.IsEmptyOrNull())
                return gameIDs;

            var queryGames = GameListRepository.GetByIds(gameIDs);

            return queryGames
                .Where(x => x.IsSupportSingleWallet)
                .Select(x => x.GameID)
                .ToList();
        }

        public TrustUserCheckResult CheckIPAddressAndSourceIsValid(string myIP, string loginUserName, RestrictLevel validateLevel)
        {
            var successCheck = new TrustUserCheckResult
            {
                IsValidateSuccess = true
            };

            if (SettingHelper.IsLocalhostWebSiteName && !SettingHelper.IsMSTest)
                return successCheck;

            if (SettingHelper.IsDev && !SettingHelper.IsMSTest && myIP.StartsWith("193.36"))
                return successCheck;

            if (IsWhiteListIPAddressForAdminSys(myIP, loginUserName))
                return successCheck;

            string myUserNameLower = loginUserName.ToLower();

            //部分功能，二級安全等級、可以僅判斷是否為內部人士
            if (validateLevel == RestrictLevel.AllowTrustUserName ||
                validateLevel == RestrictLevel.AllowTrustUserName_IncludeOtherTeam)
            {
                string availableUsersLevel2 = RestrictLevel.AllowTrustUserName.GetDescription().ToLower();
                var availableUsersList = availableUsersLevel2.Split(',').ToList();

                if (availableUsersList.Any(name => name == myUserNameLower))
                {
                    return successCheck;
                }
            }

            //部分功能，三級安全等級、可以僅判斷是否為財務部門
            if (validateLevel == RestrictLevel.AllowTrustUserName_IncludeOtherTeam)
            {
                string availableUsersLevel3 = RestrictLevel.AllowTrustUserName_IncludeOtherTeam.GetDescription().ToLower();
                var availableUsersList = availableUsersLevel3.Split(',').ToList();

                if (availableUsersList.Any(name => name == myUserNameLower))
                {
                    return successCheck;
                }
            }

            return new TrustUserCheckResult
            {
                IsValidateSuccess = false
            };
        }


        public bool IsWhiteListIPAddressForAdminSys(string myIP, string myUserName)
        {
            if (IPHelper.IsCompanyIP(myIP))
            {
                return true;
            }

            if (SettingHelper.IsDev && myIP.Contains("192.168.5"))
            {
                return true;
            }

            string myUserNameLower = myUserName.ToLower();

            //內部技術人員 (方便在家操作)
            if (myUserNameLower == "marshall")
            {
                if (IPHelper.CheckIsMarshallHomeIPAddress(myIP))
                {
                    return true;
                }
            }

            return false;
        }


        public bool IsTrustInternalUserNameAllowLogin(string myIP, string myUserName, int userID)
        {
            string myUserNameLower = myUserName.ToLower();

            //內部人員 (方便在家操作) 這些名單不限制IP
            if (myUserNameLower == "ion-cs777" ||
                myUserNameLower == "devsuper20240516")
            {
                return true;
            }

            var query = UserCustomizedSettingRepository.GetByUniqueKey(userID, UserCustomizedSettingEnum.IsAllowSkipIPAddressWhen2FA);

            if (query != null && query.SettingValue == "1")
                return true;

            return false;
        }


    }

}
