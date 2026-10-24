using Lck.Utility.Extensions.Helper;
using Lck.Utility.Helper;
using target.Models.Attributes;
using target.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lck.Utility.Extensions.Helper
{
    public class CacheKey
    {
        public static string GetAllGameBriefByGameVendor(int gameVendor)
        {
            return $"GetAllGameBriefByGameVendor_{gameVendor}";
        }
        
        public static string GetAliveMerchants()
        {
            return $"GetAliveMerchants";
        }

        public static string GlobalAccountingIgnorePlayerIDs()
        {
            return $"GlobalAccountingIgnorePlayerIDs";
        }

        public static string GetAllGameNameLanguageText()
        {
            return $"GetAllGameNameLanguageText";
        }
        public static string Lobby_PlayerJwtTokenByGuid(Guid guid)
        {
            return $"Lobby_PlayerJwtToken_v3_{guid.ToString()}";
        }
        public static string Lobby_InvisibleGames_Merchant(int merchantID)
        {
            return $"Lobby_InvisibleGames_v4_Merchant{merchantID}";
        }
        public static string Lobby_DisabledGames()
        {
            return $"Lobby_DisabledGames";
        }

        public static string Lobby_PlatformTransfer_Counter(int playerID , int type , DateTime utcNow)
        {
            return $"Lobby_PlatformTransfer_Counter_{playerID}_{type}_{utcNow.ToString("yyyyMMddHH")}";
        }

        public static string Lobby_BetRankingGames(int merchantID)
        {
            return $"LobbyBetRankingGames{merchantID}";
        }

        public static string Skype_LastJwtToken(string appID)
        {
            return $"Skype_LastJwtToken_{appID}";
        }

        public static string MyEazyCounter(string key)
        {
            return $"MyEazyCounter_{key}";
        }

        public static string ExternalAPI_NoneCompletedWager_PayoutAmountCounter(int gameVendorID , int playerID)
        {
            return $"ExternalAPI_NoneCompletedWager_PayoutAmountCounter{gameVendorID}_{playerID}";
        }

        public static string ExternalAPI_MerchantValidateInfo(int merchantID, int gameVendorID)
        {
            return $"ExternalAPI_MerchantValidateInfo_{merchantID}_{gameVendorID}";
        }

        /// <summary>
        /// VendorAPI game list API
        /// </summary>
        public static string VendorAPI_GetMerchantEnableGames(int merchantID)
        {
            string suffix = merchantID == 0 ? "" : merchantID.ToString();
            return $"VendorAPI_GetMerchantEnableGames_v3_{suffix}";
        }

        public static string Entity_GetMerchantEnableGameIDs(int merchantID)
        {
            string suffix = merchantID == 0 ? "" : merchantID.ToString();
            return $"Entity_GetMerchantEnableGameIDs_{suffix}";
        }


        public static string AdminSys_OrganizationsList()
        {
            return $"AdminSys_OrganizationsList";
        }

        public static string AdminSys_UserOwnPermission(int userID)
        {
            return $"AdminSys_v4_UserOwnPermission_{userID}";
        }

        public static string AdminSys_TestRedisKey()
        {
            return $"AdminSys_TestRedisKey";
        }

        public static string AdminSys_RolePermission(string roleIdsText)
        {
            return $"AdminSys_RolePermission{roleIdsText}";
        }

        public static string AdminSys_UserInfo(int userID)
        {
            return $"AdminSys_UserInfo{userID}";
        }

        public static string AdminSys_UserToken_ByHash(string tokenHash)
        {
            return $"AdminSys_UserToken_ByHash{tokenHash}";
        }

        public static string AdminSys_MerchantInfo(int merchantId)
        {
            return $"AdminSys_MerchantInfo{merchantId}";
        }
        public static string AdminSys_CreateDevMerchantCounter(int userID)
        {
            return $"AdminSys_CreateDevMerchantCounter_{userID}";
        }

        public static string AdminSys_FailLoginCounter(string ipOrDevice)
        {
            return $"AdminSys_FailLoginCounter_{ipOrDevice}";
        }

        public static string AdminSys_FailLoginCounter_Method(string methodName, string ipOrDevice)
        {
            return $"AdminSys_FailLoginCounter_Method{ipOrDevice}";
        }


        public static string AdminSys_HomePageReport_YesterdayInfo(DateTime begin, DateTime end)
        {
            return $"AdminSys_HomePageReport_YesterdayInfo_{begin.ToString("yyyyMMddHHmmss")}_{end.ToString("yyyyMMddHHmmss")}";
        }

        public static string AdminSys_HomePageReport_EachMerchantSummary(DateTime begin , DateTime end)
        {
            return $"AdminSys_HomePageReport_v2_EachMerchantSummary_{begin.ToString("yyyyMMddHHmmss")}_{end.ToString("yyyyMMddHHmmss")}";
        }

        public static string AdminSys_HomePageReport_EachMerchantTotalRound(DateTime begin, DateTime end)
        {
            return $"AdminSys_HomePageReport_EachMerchantTotalRound{begin.ToString("yyyyMMddHHmmss")}_{end.ToString("yyyyMMddHHmmss")}";
        }

        public static string AdminSys_FailLoginLock(string ipOrDevice)
        {
            return $"AdminSys_FailLoginLock_{ipOrDevice}";
        }

        public static string AdminSys_GamesDropdownFullTextForKeywordSearch()
        {
            return $"AdminSys_GamesDropdownFullTextForKeywordSearch";
        }

        public static string AdminSys_EventSendPrizeLocker(int eventID)
        {
            return $"AdminSys_EventSendPrizeLocker_{eventID}";
        }

        /// <summary>
        /// 會開放編輯一些平時不開放編輯的東西
        /// </summary>
        public static string AdminSys_SuperAdminEableEditEverything()
        {
            return $"AdminSys_SuperAdminEableEditEverything";
        }


        public static string AdminSys_PlayerBriefInfo(int playerID)
        {
            return $"AdminSys_PlayerBrief{playerID}";
        }

        public static string AdminSys_ServerBriefInfo(int serverID)
        {
            return $"AdminSys_ServerBriefInfo{serverID}";
        }

        public static string PublicAPI_EventSupportGamesViewModel(int merchantID)
        {
            string text = merchantID == 0 ? "" : merchantID.ToString();
            return $"PublicAPI_EventSupportGamesViewModel{text}";
        }

        public static string BetRecordScreenShotDetailInfo(string betRecordNoPGS)
        {
            return $"BetRecordScreenShotDetailInfo_{betRecordNoPGS}";
        }


        /// <summary>
        /// 還剩幾個紅包
        /// </summary>
        public static string Event_PlayerEventTicket(int player, int eventID)
        {
            return $"Event_PlayerEventTicket{player}_{eventID}";
        }

        public static string Event_RankingEvent_TopPlayers(int eventID)
        {
            return $"Event_RankingEvent_TopPlayers_v4{eventID}";
        }

        public static string FlagExistValue_PlayerTag(int playerID, int tagID)
        {
            return $"FlagExistValue_PlayerTag_{playerID}_{tagID}";
        }


        [SettingDefaultValue((int)CacheInstanceCategory.Redis002)]
        public static string ExternalAPI_BetRecordExistTempCache(GameVendorTypeEnum gameVendorID, string thirdPartyWagerID, int pgsGameID)
        {
            return $"ExternalAPI_BetRecordExistTempInfo_V3_{(int)gameVendorID}_{thirdPartyWagerID}_{pgsGameID}";
        }


        [SettingDefaultValue((int)CacheInstanceCategory.Redis002)]
        public static string ExternalAPI_BetRecordExistTempInfo(GameVendorTypeEnum gameVendorID , string thirdPartyWagerID, string gameCode)
        {
            return $"ExternalAPI_BetRecordExistTempInfo_V3_{(int)gameVendorID}_{thirdPartyWagerID}_{gameCode}";
        }

        public static string ExternalAPI_BetRecordSettle_LockByRound(string aggRoundID)
        {
            return $"ExternalAPI_BetRecordSettle_ShortTermLock{aggRoundID}";
        }


        public static string EazyLocker_LockShortTerm_Player(int playerID)
        {
            return EazyLocker_LockShortTerm($"_player_{playerID}");
        }

        public static string EazyLocker_LockShortTerm(string lockerKey)
        {
            return $"EazyLocker_LockShortTerm_{lockerKey}";
        }

        public static string ExternalAPI_BetRecord_LockShortTermForCancelAPI(string transactionNo)
        {
            return $"ExternalAPI_BetRecord_LockShortTermForCancelAPI_{transactionNo}";
        }

        public static string ExternalAPI_GetApiKey_ByAssignVendorID(string assignVendorID)
        {
            return $"ExternalAPI_GetApiKey_ByAssignVendorID{assignVendorID}";
        }

     
        public static string ExternalAPI_RiskControlRecordCount_ByConfigID(int riskControlConfigID)
        {
            return $"ExternalAPI_RiskControlRecordCount_ByConfigID{riskControlConfigID}";
        }

        [SettingDefaultValue((int)CacheInstanceCategory.Redis003)]
        public static string VendorAPI_CheckRequestTooManyTimes(int merchantID , string dateText , string parameterHash , string apiName)
        {
            return $"VendorAPI_CheckRequestTooManyTimes{merchantID}_{dateText}_{parameterHash}_{apiName}";
        }

        public static string VendorAPI_IpForbidden(int merchantID)
        {
            return $"VendorAPI_IpForbidden_v2_{merchantID}";
        }


        public static string VendorAPI_RequestTimesErrorTempCache(int merchantID , string apiName)
        {
            return $"VendorAPI_RequestTimesError{merchantID}_{apiName}";
        }


        public static string Events_PrizeRecord_GameID_ExistCheck(int playerID, int prizeBundleID, int gameID , string targetDateText = "")
        {
            string gameIDText = gameID == 0 ? "" : gameID.ToString();
            return $"Events_PrizeRecord_GameID_ExistCheck{playerID}_{prizeBundleID}_{gameIDText}_{targetDateText}";
        }

        public static string Events_EventContent_ContentBlocks(int eventID , EventContentLayoutTypeEnum type)
        {
            return $"Events_EventContent_ContentBlocks{eventID}_{(int)type}";
        }

        public static string Events_Bundle_MerchantEventAndSupportGame(int merchantID)
        {
            string merchantIDText = merchantID == 0 ? "" : merchantID.ToString();
            return $"Events_Bundle_MerchantEventAndSupportGame_V2_{merchantIDText}";
        }

        public static string DbEntity_VendorSessionToken(string token)
        {
            return $"DbEntity_VendorSessionToken_{token}";
        }

        public static string DbEntity_Player_ByPlayerNameMerchantID(string playerName, int merchantID , bool isLockQuery)
        {
            string lockQuery = isLockQuery ? "1" : "0";
            return $"DbEntity_Player_ByPlayerNameMerchantID_{merchantID}_{playerName}_{lockQuery}";
        }

        public static string DbEntity_Player_ByToken(string token)
        {
            return $"DbEntity_Player_ByToken{token}";
        }
        public static string DbEntity_PlayerGameToken_ByTokenHash(string tokenHash)
        {
            return $"DbEntity_PlayerGameToken_ByToken{tokenHash}";
        }

        public static string DbEntity_Player(int playerID)
        {
            return $"DbEntity_Player{playerID}";
        }

        public static string DbEntity_SingleWalletMerchantSessionToken(string sessionTokenPGS)
        {
            return $"DbEntity_SingleWalletMerchantSessionToken{sessionTokenPGS}";
        }

        public static string DbEntity_MerchantHierarchy(int merchantID)
        {
            return $"DbEntity_MerchantHierarchy_{merchantID}";
        }

        public static string DbEntity_Event(int eventID)
        {
            return $"DbEntity_Event{eventID}";
        }
        
        public static string DbEntity_Brief_IPAddressListTemplate_Id(int id)
        {
            return $"DbEntity_Brief_IPAddressListTemplate_Id{id}";
        }

        public static string DbEntity_RiskControlConfig_MerchantID_GameID(int merchantID, int gameID)
        {
            return $"DbEntity_RiskControlConfig_MerchantID_GameID{merchantID}_{gameID}";
        }

        public static string DbEntity_VendorConfig(int vendorConfigID)
        {
            return $"DbEntity_VendorConfig{vendorConfigID}";
        }

        public static string DbEntity_VendorConfig_By_Merchant_GameVendor(int merchantID, int gameVendorType)
        {
            return $"DbEntity_VendorConfig_By_Merchant_GameVendor{merchantID}_{gameVendorType}";
        }

        public static string DbEntity_ParseModel_Multiple_MerchantCustomizedSetting(int merchantID, List<int> settingKeys)
        {
            string keys_text = string.Empty;

            if(settingKeys != null && settingKeys.Count() > 0)
            {
                keys_text = string.Join("_", settingKeys.OrderBy(x => x).Distinct().ToList());
            }

            return $"DbEntity_ParseModel_MerchantCustomizedSetting{merchantID}_{keys_text}";
        }

        public static string DbEntity_ParseModel_MerchantCustomizedSetting(int merchantID, int settingKey)
        {
            return $"DbEntity_ParseModel_MerchantCustomizedSetting{merchantID}_{settingKey}";
        }

        public static string DbEntity_ParseModel_EventCustomizedSetting(int eventID, int settingKey)
        {
            return $"DbEntity_ParseModel_EventCustomizedSetting{eventID}_{settingKey}";
        }

        public static string DbEntity_PlayerTag(int playerID, int tagID)
        {
            return $"DbEntity_PlayerTag{playerID}_{tagID}";
        }

        public static string DbEntity_GameListCustomizedSetting(int gameID , int settingKey)
        {
            return $"DbEntity_GameListCustomizedSetting{gameID}_{settingKey}";
        }

        public static string DbEntity_GameList(int gameID)
        {
            return $"DbEntity_GameList{gameID}";
        }

        public static string DbEntity_GameList_GameVendorCode(int vendorID, string thirdGameCode)
        {
            return $"DbEntity_GameList_GameVendorCode{vendorID}_{thirdGameCode}";
        }

        public static string DbEntity_GameList_GPIGameCode(string gpiGameCode)
        {
            return $"DbEntity_GameList_GPIGameCode_{gpiGameCode}";
        }

        public static string DbEntity_ParseModel_SystemConfigSetting(int settingID)
        {
            return $"DbEntity_SystemConfigSetting{settingID}";
        }

        public static string DbEntity_LanguageTexts(string languageCode)
        {
            return $"DbEntity_LanguageTexts{languageCode}";
        }

        public static string DbEntity_VendorMaintain_VendorID(int vendorID)
        {
            return $"DbEntity_v3_VendorMaintain_VendorID{vendorID}";
        }

        public static string MemoryCache_SystemMaintainInfo(int gameVendorID)
        {
            return $"MemoryCache_SystemMaintainInfo{gameVendorID}";
        }


        public static string ExternalVendor_VendorConfigSettings(int merchantID, int vendorID)
        {
            if (merchantID == 0 && vendorID == 0)
                return "ExternalVendor_VendorConfigSettings_";

            return $"ExternalVendor_VendorConfigSettings_{merchantID}_{vendorID}";
        }

        public static string Event_FullInformation(int eventID)
        {
            return $"Event_FullInformation_{eventID}";
        }

        public static string Event_EventFinish(int eventID, int playerID)
        {
            return $"Event_EventFinish{eventID}_{playerID}";
        }


        public static string Job_JobSettingControl(int jobType)
        {
            return $"Job_JobSettingControl_{jobType}";
        }

        public static string Job_FunkyBackOfficeApiReport(string rootAgentCategory, string beginDateText)
        {
            return $"Job_FunkyBackOfficeApiReport_{rootAgentCategory}_{beginDateText}";
        }

        public static string Job_FunkyBackOfficeLoginToken(string rootAgentCategory)
        {
            return $"Job_FunkyBackOfficeLoginToken_{rootAgentCategory}";
        }


    }


}
