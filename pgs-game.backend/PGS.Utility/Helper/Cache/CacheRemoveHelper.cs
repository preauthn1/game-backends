using Lck.Utility.Extensions.Helper;
using target.Models.Enums;
using target.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lck.Utility.Extensions.Helper
{
    public class CacheRemoveHelper
    {

        /// <summary>
        /// 後台修改用戶狀態
        /// </summary>
        public static void RemoveCache_WhenUpdatePlayer(int playerID)
        {
            CacheHelper.RemoveCacheByKey(CacheKey.AdminSys_PlayerBriefInfo(playerID));
        }


        /// <summary>
        /// 更新系統設置
        /// </summary>
        public static void RemoveCache_WhenUpdateSystemSetting(int settingKey)
        {
            CacheHelper.RemoveCacheByKey(CacheKey.DbEntity_ParseModel_SystemConfigSetting(settingKey));
        }


        /// <summary>
        /// 後台用戶每次登入 刷新某些cache
        /// </summary>
        public static void RemoveCache_WhenUserLoginOrAvailableMerchantChanged(int userID)
        {
            CacheHelper.RemoveCacheByKey(CacheKey.AdminSys_UserOwnPermission(userID));
        }


        /// <summary>
        /// 後台用戶資訊 被修改
        /// </summary>
        public static void RemoveCache_WhenUpdateUser(int userID)
        {
            CacheHelper.RemoveCacheByKey(CacheKey.AdminSys_UserOwnPermission(userID));
            CacheHelper.RemoveCacheByKey(CacheKey.AdminSys_UserInfo(userID));
        }

        /// <summary>
        /// 修改角色權限時
        /// </summary>
        public static void RemoveCache_WhenRolePermissionUpdate(int roleID)
        {
            CacheHelper.RemoveCacheByKey(CacheKey.AdminSys_RolePermission(roleID.ToString()));
        }

        /// <summary>
        /// 修改 多語系文字
        /// </summary>
        public static void RemoveCache_WhenUpdateLanguageCode(string languageCode)
        {
            CacheHelper.RemoveCacheByKey(CacheKey.DbEntity_LanguageTexts(languageCode));
        }


        /// <summary>
        /// 修改伺服器
        /// </summary>
        public static void RemoveCache_WhenUpdateServer(int serverID)
        {
            CacheHelper.RemoveCacheByKey(CacheKey.AdminSys_ServerBriefInfo(serverID));
        }

        /// <summary>
        /// 修改遊戲商配置
        /// </summary>
        public static void RemoveCache_WhenVendorConfigUpdate(int vendorConfigID)
        {
            //關鍵字清除 (清除全部相同關鍵字)
            string keyword = CacheKey.ExternalVendor_VendorConfigSettings(0, 0);

            CacheHelper.RemoveCacheByPrefixKeyword(keyword);
            CacheHelper.RemoveCacheByKey(CacheKey.DbEntity_VendorConfig(vendorConfigID));
        }


        public static void UnlockFailLoginLocker()
        {
            string cacheKey1 = CacheKey.AdminSys_FailLoginCounter("");
            string cacheKey2 = CacheKey.AdminSys_FailLoginLock("");

            CacheHelper.RemoveCacheByPrefixKeyword(cacheKey1);
            CacheHelper.RemoveCacheByPrefixKeyword(cacheKey2);
        }


        public static void RemoveCache_WhenVendorConfigMerchantMappingUpdate(int merchantID)
        {
            RemoveEachGameVendorCacheOfTargetMerchant(merchantID);
        }


        /// <summary>
        /// 遊戲商維護
        /// </summary>
        public static void RemoveCache_WhenVendorMaintainSettingUpdate(int vendorID)
        {
            CacheHelper.RemoveCacheByKey(CacheKey.DbEntity_VendorMaintain_VendorID(vendorID));
        }


        public static void RemoveCache_WhenLobbyTokenReset(Guid tokenGuid)
        {
            CacheHelper.RemoveCacheByKey(CacheKey.Lobby_PlayerJwtTokenByGuid(tokenGuid));
        }

        public static void RemoveCache_WhenUpdateEvent(int eventID)
        {
            CacheHelper.RemoveCacheByKey(CacheKey.Event_FullInformation(eventID));
            CacheHelper.RemoveCacheByKey(CacheKey.Event_RankingEvent_TopPlayers(eventID));

            CacheHelper.RemoveCacheByPrefixKeyword(CacheKey.Events_Bundle_MerchantEventAndSupportGame(0));
            CacheHelper.RemoveCacheByPrefixKeyword(CacheKey.PublicAPI_EventSupportGamesViewModel(0));

            var settingKeys = ExtensionOfEnum.GetEnumAllValue<EventCustomizedSettingKeyEnum>();

            foreach (var key in settingKeys)
            {
                string cacheKey = CacheKey.DbEntity_ParseModel_EventCustomizedSetting(eventID, key.ToInteger());
                CacheHelper.RemoveCacheByKey(cacheKey);
            }
        }


        public static void RemoveCache_WhenUpdateEventContent(int eventID)
        {
            CacheHelper.RemoveCacheByKey(CacheKey.Events_EventContent_ContentBlocks(eventID, EventContentLayoutTypeEnum.TitleAndBanner));
            CacheHelper.RemoveCacheByKey(CacheKey.Events_EventContent_ContentBlocks(eventID, EventContentLayoutTypeEnum.EventRules));
            CacheHelper.RemoveCacheByKey(CacheKey.Events_EventContent_ContentBlocks(eventID, EventContentLayoutTypeEnum.Description));
        }


        public static void RemoveCache_WhenEventSendAllPrize(int eventID)
        {
            CacheHelper.RemoveCacheByKey(CacheKey.Event_FullInformation(eventID));

            string keyword = CacheKey.Events_Bundle_MerchantEventAndSupportGame(0);
            CacheHelper.RemoveCacheByPrefixKeyword(keyword);
        }

        /// <summary>
        /// 更新玩家標籤
        /// </summary>
        public static void RemoveCache_WhenResetPgsPlayerToken(string tokenHash)
        {
            CacheHelper.RemoveCacheByKey(CacheKey.DbEntity_PlayerGameToken_ByTokenHash(tokenHash));
        }

        /// <summary>
        /// 更新玩家標籤
        /// </summary>
        public static void RemoveCache_WhenUpdatePlayerTag(int playerID, int tagID)
        {
            CacheHelper.RemoveCacheByKey(CacheKey.DbEntity_PlayerTag(playerID, tagID));
            CacheHelper.RemoveCacheByKey(CacheKey.FlagExistValue_PlayerTag(playerID, tagID));

            if (tagID == (int)SystemTagEnum.Player_LobbyTransferForbidden)
            {
                CacheHelper.RemoveCacheByPrefixKeyword($"Lobby_PlatformTransfer_Counter_{playerID}");
            }
        }

        /// <summary>
        /// 玩家打開紅包 / 抽到紅包
        /// </summary>
        public static void RemoveCache_WhenReceivePrize(int playerID, int eventID)
        {
            CacheHelper.RemoveCacheByKey(CacheKey.Event_PlayerEventTicket(playerID, eventID));
        }

        /// <summary>
        /// 修改遊戲
        /// </summary>
        public static void RemoveCache_WhenUpdateGame(int gameID, int vendorID, string thirdGameCode)
        {
            CacheHelper.RemoveCacheByKey(CacheKey.DbEntity_GameList(gameID));
            CacheHelper.RemoveCacheByKey(CacheKey.DbEntity_GameList_GameVendorCode(vendorID, thirdGameCode));

            //遊戲開關狀態可能會有有異動 刪除 VendorAPI
            var queryKeys = CacheHelper.GetCacheKeysByKeyword(CacheKey.VendorAPI_GetMerchantEnableGames(0));    
            var queryKeys2 = CacheHelper.GetCacheKeysByKeyword(CacheKey.Entity_GetMerchantEnableGameIDs(0));

            foreach (string key in queryKeys)
            {
                CacheHelper.RemoveCacheByKey(key);
            }

            foreach (string key in queryKeys2)
            {
                CacheHelper.RemoveCacheByKey(key);
            }
        }


        /// <summary>
        /// 修改商戶
        /// </summary>
        public static void RemoveCache_WhenUpdateMerchant(int merchantID)
        {
            CacheHelper.RemoveCacheByKey(CacheKey.AdminSys_MerchantInfo(merchantID));
            CacheHelper.RemoveCacheByKey(CacheKey.AdminSys_MerchantInfo(merchantID));

            RemoveEachGameVendorCacheOfTargetMerchant(merchantID);
        }

        public static void RemoveCache_WhenUpdateIPAddressTemplateID(int templateID)
        {
            CacheHelper.RemoveCacheByKey(CacheKey.DbEntity_Brief_IPAddressListTemplate_Id(templateID));
        }

        /// <summary>
        /// 修改商戶樹狀代理異動
        /// </summary>
        public static void RemoveCache_WhenMultipleCaches_WhenMerchantHierarchyChanged(List<int> merchantIDs)
        {
            //如果筆數太多 就會改用Async
            if(merchantIDs.CountOrZero() <= 500)
            {
                foreach(var merchantID in merchantIDs)
                {
                    RemoveCache_WhenMerchantHierarchyChanged(merchantID);
                }

                return;
            }

            Task.Factory.StartNew(() =>
            {
                foreach (var merchantID in merchantIDs)
                {
                    RemoveCache_WhenMerchantHierarchyChanged(merchantID);
                }
            });
        }


        /// <summary>
        /// 修改商戶樹狀代理異動
        /// </summary>
        public static void RemoveCache_WhenMerchantHierarchyChanged(int merchantID)
        {
            CacheHelper.RemoveCacheByKey(CacheKey.Events_Bundle_MerchantEventAndSupportGame(merchantID));
            CacheHelper.RemoveCacheByKey(CacheKey.DbEntity_MerchantHierarchy(merchantID));
        }


        public static void RemoveCache_WhenMerchantSelectTemplateMappingMerchantChanged(int merchantID)
        {
            CacheHelper.RemoveCacheByKey(CacheKey.Events_Bundle_MerchantEventAndSupportGame(merchantID));
            RemoveEachGameVendorCacheOfTargetMerchant(merchantID);
        }

        /// <summary>
        /// 修改商戶開通遊戲
        /// </summary>
        public static void RemoveCache_WhenUpdateMerchantGames(int merchantID)
        {
            CacheHelper.RemoveCacheByKey(CacheKey.VendorAPI_GetMerchantEnableGames(merchantID));
            CacheHelper.RemoveCacheByKey(CacheKey.Entity_GetMerchantEnableGameIDs(merchantID));
            CacheHelper.RemoveCacheByKey(CacheKey.Lobby_InvisibleGames_Merchant(merchantID));
            CacheHelper.RemoveCacheByKey(CacheKey.PublicAPI_EventSupportGamesViewModel(merchantID));

            RemoveEachGameVendorCacheOfTargetMerchant(merchantID);
        }


        /// <summary>
        /// 修改商戶進階配置 (迴圈中)
        /// </summary>
        public static void RemoveCache_WhenUpdateMerchantSetting(int merchantID, int settingKey)
        {
            CacheHelper.RemoveCacheByKey(CacheKey.DbEntity_ParseModel_MerchantCustomizedSetting(merchantID, settingKey));
        }

        /// <summary>
        /// 修改商戶進階配置 (迴圈結束後)
        /// </summary>
        public static void RemoveCache_WhenUpdateMerchantSetting_WhenAllDone(int merchantID)
        {
            CacheHelper.RemoveCacheByPrefixKeyword(CacheKey.DbEntity_ParseModel_Multiple_MerchantCustomizedSetting(merchantID, null));
            CacheHelper.RemoveCacheByKey(CacheKey.PublicAPI_EventSupportGamesViewModel(merchantID));
            RemoveEachGameVendorCacheOfTargetMerchant(merchantID);
        }


        /// <summary>
        /// 修改組織
        /// </summary>
        public static void RemoveCache_WhenUpdateOrganization()
        {
            CacheHelper.RemoveCacheByKey(CacheKey.AdminSys_OrganizationsList());
        }


        private static void RemoveEachGameVendorCacheOfTargetMerchant(int merchantID)
        {
            var gameVendors = ExtensionOfEnum.GetEnumAllValue<GameVendorTypeEnum>();

            foreach (var gameVendor in gameVendors)
            {
                string cacheKey1 = CacheKey.ExternalAPI_MerchantValidateInfo(merchantID, gameVendor.ToInteger());
                string cacheKey2 = CacheKey.ExternalVendor_VendorConfigSettings(merchantID, gameVendor.ToInteger());

                CacheHelper.RemoveCacheByKey(cacheKey1);
                CacheHelper.RemoveCacheByKey(cacheKey2);
            }
        }



    }

}
