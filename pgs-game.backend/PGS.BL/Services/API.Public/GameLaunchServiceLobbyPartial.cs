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
using target.Utility.Helper.Dependency;
using target.Models.ViewModels.Base;
using target.Utility.Extensions.Web;
using System.Reflection;

namespace target.BL.Services
{

    public partial class GameLaunchService : BaseVendorService, IGameLaunchService
    {

        public LobbyGenerateLinkResultModel GenerateGameLinkForLobby(LobbyApiGenerateLinkParameterModel model)
        {
            model.RuleFor(z => z.GameID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.Ts).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.PlayerName).Rule_NotEmptyText();
            model.RuleFor(z => z.SignKey).Rule_NotEmptyText();

            SignKeyRuleHelper.CheckIsValidPublicApiLobbySignkey(model.Ts, model.PlayerName, model.SignKey);

            string token = [REDACTED];

            var lobbyTokenResult = GetLobbyTokenAndCheckValid(token);

            GameLaunchRequestModel launchParameter = InstanceLaunchParameter(model, lobbyTokenResult);
            launchParameter.IsFromLobbyPage = true;

            VerifyCheckCodeIsValid(model, launchParameter, lobbyTokenResult);

            var result = LaunchGameAndGetGamePageUrl(model, launchParameter);
            return result;
        }

        public LobbyGenerateLinkResultModel GenerateGameLinkForIframe(LobbyApiGenerateLinkParameterModel model)
        {
            model.RuleFor(z => z.GameID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.Ts).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.PlayerName).Rule_NotEmptyText();
            model.RuleFor(z => z.SignKey).Rule_NotEmptyText();

            SignKeyRuleHelper.CheckIsValidPublicApiLobbySignkey(model.Ts, model.PlayerName, model.SignKey);

            string token = [REDACTED];

            var lobbyTokenResult = GetLobbyTokenAndCheckValid(token);

            GameLaunchRequestModel launchParameter = InstanceLaunchParameter(model, lobbyTokenResult);
            launchParameter.IsFromIframe = true;

            VerifyCheckCodeIsValid(model, launchParameter, lobbyTokenResult);

            var result = LaunchGameAndGetGamePageUrl(model, launchParameter);
            return result;
        }


        private GameLaunchRequestModel InstanceLaunchParameter(
            LobbyApiGenerateLinkParameterModel model,
            LobbyTokenParsingResultModel lobbyTokenResult)
        {
            var parseModel = lobbyTokenResult.ParsingTokenModel;

            if (parseModel.LoginName != model.PlayerName)
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_NotValid);

            var launch = new GameLaunchRequestModel
            {
                GameID = model.GameID,
                MerchantID = parseModel.MerchantID,
                PlayerName = parseModel.LoginName,
                SessionToken = [REDACTED],
                IsAllowDuplicateSessionToken = [REDACTED], //使用同一組sessionToken
            };

            var locale = HeaderValueHelper.GetLocaleHeader();

            if (string.IsNullOrWhiteSpace(locale))
            {
                launch.Lang = parseModel.Lang;
            }

            //部分數值要保留第一次launch帶入的
            CopyValueFromOriginalRequestModel(launch, lobbyTokenResult.ParsingRawRequest);

            //先指定進lobby 再進去特定遊戲、可能部分參數會遺失要再塞回去
            if (lobbyTokenResult.ParsingRawRequest?.ExtraLaunchSetting != null)
            {
                launch.ExtraLaunchSetting = lobbyTokenResult.ParsingRawRequest.ExtraLaunchSetting;
            }

            return launch;
        }


        private LobbyGenerateLinkResultModel LaunchGameAndGetGamePageUrl(LobbyApiGenerateLinkParameterModel model, GameLaunchRequestModel param)
        {
            bool isMobileDevice = HeaderValueHelper.IsMobileDevice();

            param.DeviceType = (isMobileDevice ? HDTypeEnum.Mobile : HDTypeEnum.PC).ToInteger();

            var linkResult = LaunchGameAndGetGamePageUrl(param);

            var merchantSettings = MerchantCustomizedSettingRepository.GetMultipleMerchantSettingByCache(param.MerchantID, new List<MerchantCustomizedSettingKeyEnum>
            {
                MerchantCustomizedSettingKeyEnum.Common_LobbyBrandingName,
                MerchantCustomizedSettingKeyEnum.Common_MerchantTag_BrandingCategory,
                MerchantCustomizedSettingKeyEnum.Common_DisableBrandingLogoAndLoading,
                MerchantCustomizedSettingKeyEnum.GameSetting_IsShowBackbuttonInIframeGame
            });

            MerchantCustomizedSettingModel brandingName = merchantSettings.GetBySettingKey(MerchantCustomizedSettingKeyEnum.Common_LobbyBrandingName);

            MerchantCustomizedSettingModel brandingTag = merchantSettings.GetBySettingKey(MerchantCustomizedSettingKeyEnum.Common_MerchantTag_BrandingCategory);

            MerchantCustomizedSettingModel disableLoadingSetting = merchantSettings.GetBySettingKey(MerchantCustomizedSettingKeyEnum.Common_DisableBrandingLogoAndLoading);

            var queryMerchant = MerchantRepository.GetSingleByCache(param.MerchantID);

            string backUrlLobby = string.Empty;
            bool isLandscapeRequired = false;

            int gameVendor = AppHelper.GetGameVendorByGameID(model.GameID);
            bool isShowBackButtonInIframeGamePage = IsShowBackButtonInIframeGamePage(param.MerchantID, gameVendor);

            if (isShowBackButtonInIframeGamePage)
            {
                backUrlLobby = GenerateLobbyUrl();

                List<int> landscapeGames = new List<int>
                {
                    (int)GameListEnum.DFDC_多福多财,
                    (int)GameListEnum.TGW_糖果屋,
                    (int)GameListEnum.JJBX_金吉报喜_无尽宝藏,
                    (int)GameListEnum.CSDJP_財神到_彩金版,
                    (int)GameListEnum.ALD_阿拉丁,
                    (int)GameListEnum.WLYC_五龍運財,
                    (int)GameListEnum.hljbp_欢乐聚宝盆,
                    (int)GameListEnum.LHD_龙虎斗,
                    (int)GameListEnum.BJL_百家乐,
                    (int)GameListEnum.HHDZ_红黑大战,
                };

                //若遊戲需要以橫屏模式 會告知前端調整樣式
                isLandscapeRequired = landscapeGames.Contains(model.GameID);
            }


            bool isIONSlotBranding = brandingTag.SettingValueOfInt == (int)SystemTagEnum.MerchantTag_Branding_ION;

            var result = new LobbyGenerateLinkResultModel
            {
                SiteDisplayName = brandingName?.SettingValue,
                GameName = DbMultiLanguageTextHelper.GetGameDisplayName(model.GameID),
                Url = linkResult.Link,
                BackUrlLobby = backUrlLobby,
                IsLandscapeRequired = isLandscapeRequired,
                IsIONSlot = isIONSlotBranding,
                ExtraInfo = new GameLaunchExtraInfoModel
                {

                }
            };

            string externalUrlHost = GetDomainByUrl(param.BackUrl);

            if (!string.IsNullOrWhiteSpace(externalUrlHost))
            {
                string lobbyUrlHost = GetDomainByUrl(backUrlLobby);

                //如果是商戶端自訂的網址 就丟給前端做一些判斷使用
                if (lobbyUrlHost != externalUrlHost)
                {
                    result.ExternalBackUrlKeyword = externalUrlHost;
                }
            }

            result.ExtraInfo.IsShowExchangeRateHint = IsShowMYR100Hint(queryMerchant.Currency, linkResult.PlayerID);
            result.ExtraInfo.IsIONSlot = isIONSlotBranding;

            if (disableLoadingSetting.SettingValueOfBoolean)
            {
                result.ExtraInfo.IsDisableLogoAndLoading = true;
            }

            result.ExtraInfo.IsShowGamePageHeader = linkResult.IsShowGamePageHeader;
            return result;
        }


        private bool IsShowMYR100Hint(string merchantCurrency , int playerID)
        {
            bool isMYR100 = merchantCurrency == "MYR100";

            if (!isMYR100)
                return false;

            var isReadTagExist = AnnouncementReadRepository.IsReadAlready(playerID, DefinedAnnouncementEnum.CurrencyExchangeHint.ToInteger());

            if (isReadTagExist)
                return false;

            return true;

        }



        private string GetDomainByUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            try
            {
                var parseUrl = new Uri(url);
                return parseUrl.Host;
            }
            catch
            {

            }

            return null;
        }


        private void VerifyCheckCodeIsValid(LobbyApiGenerateLinkParameterModel apiParam, GameLaunchRequestModel launch, LobbyTokenParsingResultModel tokenParse)
        {
            int merchantID = launch.MerchantID;

            var restrictMode = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.GameSetting_IsRestrictTokenMode);

            if (!restrictMode.SettingValueOfBoolean)
                return;

            if (string.IsNullOrWhiteSpace(apiParam.CheckCode))
                throw new TerproException(ApiErrorCode.Authorize_TokenAvailableGameNotMatch);

            var expectedCode = AppHelper.GenerateGameIdCheckCode(launch.GameID, tokenParse.ParsingTokenModel.LoginUserID);

            if (expectedCode != apiParam.CheckCode)
                throw new TerproException(ApiErrorCode.Authorize_TokenAvailableGameNotMatch);
        }


        private string GenerateLobbyUrl()
        {
            var headerToken = [REDACTED];

            if (string.IsNullOrWhiteSpace(headerToken))
                return string.Empty;

            var lobbyTokenResult = GetLobbyTokenAndCheckValid(headerToken);
            var parseModel = lobbyTokenResult.ParsingTokenModel;

            if (parseModel == null)
                return string.Empty;

            string base64Token = [REDACTED];

            var lobbyPageUrl = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.LobbyPageUrl);

            if (lobbyPageUrl == null || lobbyPageUrl.SettingValue.IsNullOrWhiteSpace())
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel { ColumnName = "LobbyPageDomain" });

            //pgsToken是舊版寫法 暫時保留。新板是使用 v3Token=
            [REDACTED] $"{lobbyPageUrl.SettingValue}?playerName={parseModel.LoginName}&pgsToken=[REDACTED]";
        }


        public void CopyValueFromOriginalRequestModel(GameLaunchRequestModel model, GameLaunchRequestModel copyValueFromThis)
        {
            if (copyValueFromThis == null)
                return;

            foreach (PropertyInfo item in model.GetType().GetProperties())
            {
                //如果有定義attribute的 從Lobby進入遊戲會保留原值
                var hasAttribute = item.HasAttribute<KeepValueWhenLobbyLaunch>();

                if (hasAttribute)
                {
                    var originalValue = copyValueFromThis.GetType().GetProperty(item.Name).GetValue(copyValueFromThis);

                    if (originalValue != null)
                    {
                        item.SetValue(model, originalValue);
                    }
                }
            }
        }


    }
}
