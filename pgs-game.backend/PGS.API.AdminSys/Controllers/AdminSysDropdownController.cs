using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using target.Web.Modules.Filter;
using target.Models.ViewModels.API.AdminSys;
using Microsoft.Extensions.Caching.Memory;
using target.Models.Enums;
using target.Models;

namespace target.API.AdminSys.Controllers
{
    [AdminTokenrAuthorizeFilter]
    public class AdminSysDropdownController : BaseController
    {
        public IAdminSysDropdownService AdminSysDropdownService { get; set; }


        /// <summary>
        /// 组织类型
        /// </summary>
        [Route("dropdown/organizations")]
        [HttpGet]
        public List<DropdownValueViewModel> GetAvailableOrgs()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetAvailableOrgs(userID);
            return query;
        }

        /// <summary>
        /// 商户
        /// </summary>
        [Route("dropdown/merchants")]
        [HttpGet]
        [ApiConfig(IsHeaderLogEnable = false, IsResponseLogEnable = false)]
        [ApiConfig(IsResponseLogEnable = false)]
        public List<DropdownValueViewModel> GetAvailableMerchants([FromQuery] QueryDropdownMerchantRequestViewModel filter)
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetAvailableMerchants(userID, filter);
            return query;
        }


        [Route("dropdown/IMChannels")]
        [HttpGet]
        public List<DropdownValueViewModel> GetIMChannels()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetIMChannels(userID);
            return query;
        }

        /// <summary>
        /// 商户 (有在代理樹狀結構中的)
        /// </summary>
        [Route("dropdown/hierarchy-merchants")]
        [HttpGet]
        public List<DropdownValueViewModel> GetHierarchyMerchants([FromQuery] LazyCommonRequestModel filter)
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetHierarchyMerchants(userID, filter);
            return query;
        }

        /// <summary>
        /// 商户分類
        /// </summary>
        [Route("dropdown/merchant-types")]
        [HttpGet]
        public List<DropdownValueViewModel> GetMerchantTypes()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetMerchantTypes();
            return query;
        }


        /// <summary>
        /// 特殊篩選 (參考 AdminSysExtraFilterEnum)
        /// 要查哪些額外欄位 根據categoryID分類
        /// CategoryID 對應 DataCategoryEnum
        /// </summary>
        [Route("dropdown/extra-filter-type")]
        [HttpGet]
        public List<DropdownValueViewModel> GetExtraFtilerType([FromQuery] LazyCommonRequestModel filter)
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetExtraFtilerType(userID, filter);
            return query;
        }


        /// <summary>
        /// 要查哪些額外欄位 (數據報表/代理總報表/遊戲報表)
        /// </summary>
        [Route("dropdown/stat-report-search-columns")]
        [Route("dropdown/search-extra-columns/{reportType}")]
        [HttpGet]
        public List<DropdownValueViewModel> GetStaReportSearchColumns(string reportType)
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetStaReportSearchColumns(userID, reportType);
            return query;
        }


        [Route("dropdown/one-to-many-setting-types")]
        [HttpGet]
        public List<DropdownValueViewModel> GetOneToManyTypes()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetOneToManyTypes();
            return query;
        }


        [Route("dropdown/one-times-job-types")]
        [HttpGet]
        public List<DropdownValueViewModel> GetOneTimesJobTypes()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetOneTimesJobTypes();
            return query;
        }


        [Route("dropdown/copy-merchant-extra-settings")]
        [HttpGet]
        public List<DropdownValueViewModel> GetMerchantCopyExtraSettingEnum()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetMerchantCopyExtraSettingEnum();
            return query;
        }


        /// <summary>
        /// 活動統計資訊的排序欄位
        /// </summary>
        [Route("dropdown/event-counter-order-by")]
        [HttpGet]
        public List<DropdownValueViewModel> GetEventCounterOrderByColumn()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetEventCounterOrderByColumn();
            return query;
        }


        /// <summary>
        /// 商户設置
        /// </summary>
        [Route("dropdown/merchant-setting-keys")]
        [HttpGet]
        public List<DropdownValueViewModel> GetMerchantCustomizedSettingKeys()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetMerchantCustomizedSettingKeys();
            return query;
        }


        /// <summary>
        /// 內部測試用商户
        /// </summary>
        [Route("dropdown/internal-merchants")]
        [HttpGet]
        public List<DropdownValueViewModel> GetInternalMerchants()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetInternalMerchants(userID);
            return query;
        }


        /// <summary>
        /// 取得允許加測試帳號的商户
        /// </summary>
        [Route("dropdown/merchants-allow-create-test-account")]
        [HttpGet]
        public List<DropdownValueViewModel> GetMerchantsOfAllowCreateTestAccount()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetMerchantsOfAllowCreateTestAccount(userID);
            return query;
        }


        /// <summary>
        /// 簡易查詢的項目
        /// </summary>
        [Route("dropdown/dynamic-query-items")]
        [HttpGet]
        public List<DropdownValueViewModel> GetDynamicQueryItems()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetDynamicQueryItems();
            return query;
        }


        /// <summary>
        /// 遊戲大廳房間類型
        /// </summary>
        [Route("dropdown/room-levels")]
        [HttpGet]
        public List<DropdownValueViewModel> GetRoomLevels()
        {
            var query = AdminSysDropdownService.GetRoomLevels();
            return query;
        }


        /// <summary>
        /// 遊戲分類
        /// </summary>
        [Route("dropdown/game-types")]
        [HttpGet]
        public List<DropdownValueViewModel> GetGameTypes()
        {
            var query = AdminSysDropdownService.GetGameTypes();
            return query;
        }


        /// <summary>
        /// 操作類型
        /// </summary>
        [Route("dropdown/user-operate-types")]
        [HttpGet]
        public List<DropdownValueViewModel> GetUserOperateTypes()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetUserOperateTypeEnum(userID);
            return query;
        }


        /// <summary>
        /// 投注單狀態
        /// </summary>
        [Route("dropdown/bet-record-status")]
        [HttpGet]
        public List<DropdownValueViewModel> GetBetRecordStatus()
        {
            var query = AdminSysDropdownService.GetBetRecordStatus();
            return query;
        }


        [Route("dropdown/prize-record-status")]
        [HttpGet]
        public List<DropdownValueViewModel> GetEventPrizeRecordStatusEnum()
        {
            var query = AdminSysDropdownService.GetEventPrizeRecordStatusEnum();
            return query;
        }


        [Route("dropdown/currency")]
        [HttpGet]
        public List<DropdownValueViewModel> GetCurrency()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetCurrency(userID);
            return query;
        }

        /// <summary>
        /// 角色类型
        /// </summary>
        [Route("dropdown/roles")]
        [HttpGet]
        public List<DropdownValueViewModel> GetAvailableRoles()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetAvailableRoles(userID);
            return query;
        }

        /// <summary>
        /// 游戏类型
        /// </summary>
        [Route("dropdown/games")]
        [ApiConfig(IsHeaderLogEnable = false, IsResponseLogEnable = false)]
        [HttpGet]
        public List<DropdownValueViewModel> GetGames([FromQuery] QueryGameListApiRequestParameter filter)
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetGames(userID, filter);
            return query;
        }

        [Route("dropdown/servers")]
        [HttpGet]
        public List<DropdownValueViewModel> GetServers([FromQuery] QueryServerListApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetServers(userID, filter);
            return query;
        }

        [Route("dropdown/server-and-vendorConfigs")]
        [HttpGet]
        public List<DropdownValueViewModel> GetServersAndVendorConfig([FromQuery] QueryServerListApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query1 = AdminSysDropdownService.GetServers(userID, filter);
            var query2 = AdminSysDropdownService.GetVendorConfig(userID, new QueryVendorConfigApiRequestParameterModel
            {

            });

            query2 = query2 ?? new List<DropdownValueViewModel>();
            query2.AddRange(query1);

            //一般用戶直接返回空陣列
            if(!AdminSysDropdownService.CheckIsSuperAdminOrg(userID))
            {
                return new List<DropdownValueViewModel>();
            }

            return query2;
        }

        [Route("dropdown/ip-templates")]
        [HttpGet]
        public List<DropdownValueViewModel> GetIPAddressTemplate([FromQuery] LazyCommonRequestModel filter)
        {
            var query = AdminSysDropdownService.GetIPAddressTemplate(filter);
            return query;
        }

        [Route("dropdown/generalTags")]
        [HttpGet]
        public List<DropdownValueViewModel> GetGeneralTags([FromQuery] LazyCommonRequestModel filter)
        {
            var query = AdminSysDropdownService.GetGeneralTags(filter);
            return query;
        }


        [Route("dropdown/InvoiceExportCategory")]
        [HttpGet]
        public List<DropdownValueViewModel> GetInvoiceExportCategory()
        {
            var query = AdminSysDropdownService.GetInvoiceExportCategory();
            return query;
        }

        [Route("dropdown/tagTypes")]
        [HttpGet]
        public List<DropdownValueViewModel> GetTagTypes()
        {
            var query = AdminSysDropdownService.GetDropdownsByEnum<TagTypeEnum>();
            return query;
        }

        [Route("dropdown/notifyQueueType")]
        [HttpGet]
        public List<DropdownValueViewModel> GetBetRecordNotifyQueueTypeEnum()
        {
            var query = AdminSysDropdownService.GetDropdownsByEnum<BetRecordMerchantNotifyQueueTypeEnum>();
            return query;
        }


        [Route("dropdown/tg-announcement-templates")]
        [HttpGet]
        public List<DropdownValueViewModel> GetTelegramAnnouncementTemplates()
        {
            var query = AdminSysDropdownService.GetDropdownsByEnum<TelegramAnnouncementTemplateEnum>();
            return query;
        }


        [Route("dropdown/agent-special-report-type")]
        [HttpGet]
        public List<DropdownValueViewModel> GetAgentSpecialReportTypeEnum()
        {
            var query = AdminSysDropdownService.GetDropdownsByEnum<AgentSpecialReportTypeEnum>();
            return query;
        }


        /// <summary>
        /// 时区类型
        /// </summary>
        [Route("dropdown/timezone-areas")]
        [HttpGet]
        [ApiConfig(IsResponseLogEnable = false, IsHeaderLogEnable = false)]
        public List<DropdownValueViewModel> GetTimeZoneAreas()
        {
            var query = AdminSysDropdownService.GetTimeZoneAreas();
            return query;
        }


        /// <summary>
        /// 交易類型
        /// </summary>
        [Route("dropdown/transactionTypes")]
        [HttpGet]
        public List<DropdownValueViewModel> GetTransactionTypes()
        {
            var query = AdminSysDropdownService.GetTransactionTypes();
            return query;
        }

        /// <summary>
        /// 特殊轉帳類型(lobby、遊戲商)
        /// </summary>
        [Route("dropdown/game-vendor-transfer-type")]
        [HttpGet]
        public List<DropdownValueViewModel> GetGameVendorTransferTypes()
        {
            var query = AdminSysDropdownService.GetGameVendorTransferTypes();
            return query;
        }


        [Route("dropdown/agentAggregativeType")]
        [HttpGet]
        public List<DropdownValueViewModel> GetAgentAggregativeType()
        {
            var query = AdminSysDropdownService.GetAgentAggregativeType();
            return query;
        }


        /// <summary>
        /// 遊戲廠商 (含pgs)
        /// </summary>
        [Route("dropdown/game-vendors")]
        [HttpGet]
        public List<DropdownValueViewModel> GetGameVendorTypes()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetGameVendorTypes(userID);
            return query;
        }


        /// <summary>
        /// 遊戲廠商 (不含slot88)
        /// </summary>
        [Route("dropdown/game-vendors-without-slot88")]
        [HttpGet]
        public List<DropdownValueViewModel> GetGameVendorTypesExcluedeSlot88()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetGameVendorTypesExcludeSlot88(userID);
            return query;
        }

        /// <summary>
        /// 遊戲列表-額外篩選條件
        /// </summary>
        [Route("dropdown/game-extra-filter-type")]
        [HttpGet]
        public List<DropdownValueViewModel> GetGameListExtraFilterTypes()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetGameListExtraFilterTypes(userID);
            return query;
        }


        /// <summary>
        /// 商户特殊搜尋標籤
        /// </summary>
        [Route("dropdown/merchant-special-filter")]
        [HttpGet]
        public List<DropdownValueViewModel> GetMerchantSpecialFilterTypes()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetMerchantSpecialFilterTypes();
            return query;
        }


        /// <summary>
        /// sw logs -額外篩選條件
        /// </summary>
        [Route("dropdown/sw-log-special-filter-type")]
        [HttpGet]
        public List<DropdownValueViewModel> GetSwLogsSpecialFilterTypes()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetDropdownsByEnum<SpecialFilterTypeOfSwApiRequestLog>();
            return query;
        }

        /// <summary>
        /// 額外篩選條件 (users)
        /// </summary>
        [Route("dropdown/special-filter-type/users")]
        [HttpGet]
        public List<DropdownValueViewModel> GetUsersSpecialFilterTypes()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetDropdownsByEnum<SpecialFilterTypeOfUsers>();
            return query;
        }

        /// <summary>
        /// 額外篩選條件 (ip-template)
        /// </summary>
        [Route("dropdown/special-filter-type/ip-template")]
        [HttpGet]
        public List<DropdownValueViewModel> GetIPAddressSpecialFilterTypes()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetDropdownsByEnum<SpecialFilterTypeOfIPAddressTemplate>();
            return query;
        }


        /// <summary>
        /// 額外篩選條件 (event)
        /// </summary>
        [Route("dropdown/special-filter-type/event")]
        [HttpGet]
        public List<DropdownValueViewModel> GetEventSpecialFilterTypes()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetDropdownsByEnum<SpecialFilterTypeOfEvent>();
            return query;
        }


        /// <summary>
        /// 額外篩選條件 (ip-template)
        /// </summary>
        [Route("dropdown/special-filter-type/imChannels")]
        [HttpGet]
        public List<DropdownValueViewModel> GetIMChannelSpecialFilterTypes()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetDropdownsByEnum<SpecialFilterTypeOfIMChannels>();
            return query;
        }

        /// <summary>
        /// 額外篩選條件 (api-log)
        /// </summary>
        [Route("dropdown/special-filter-type/api-log")]
        [HttpGet]
        public List<DropdownValueViewModel> GetApiRequestLogSpecialFilterTypes()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetDropdownsByEnum<SpecialFilterTypeOfApiReqeustLog>();
            return query;
        }

        /// <summary>
        /// 三方遊戲商
        /// </summary>
        [Route("dropdown/third-party-vendors")]
        [HttpGet]
        public List<DropdownValueViewModel> GetThirdPartyVendors()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetThirdPartyVendors(userID);
            return query;
        }


        /// <summary>
        /// 取得內部測試帳號
        /// </summary>
        [Route("dropdown/internal-test-accounts")]
        [HttpGet]
        public List<DropdownValueViewModel> GetInternalTestPlayerAccounts()
        {
            var query = AdminSysDropdownService.GetInternalTestPlayerAccounts();
            return query;
        }


        /// <summary>
        /// perGameLog的tags
        /// </summary>
        [Route("dropdown/perGame-Tags")]
        [ApiConfig(IsResponseLogEnable = false, IsHeaderLogEnable = false)]
        [HttpGet]
        public List<DropdownValueViewModel> GetPerGameTags()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetPerGameTags(userID);
            return query;
        }


        /// <summary>
        /// 允許手動添加的 perGameLog的tags
        /// </summary>
        [Route("dropdown/perGame-Tags/allow-manual-add")]
        [ApiConfig(IsResponseLogEnable = false, IsHeaderLogEnable = false)]
        [HttpGet]
        public List<DropdownValueViewModel> GetPerGameTagsAllowManualAdd()
        {
            var query = AdminSysDropdownService.GetPerGameTagsAllowManualAdd();
            return query;
        }

        /// <summary>
        /// 根據分類取得tags
        /// </summary>
        [Route("dropdown/system-tags")]
        [HttpGet]
        public List<DropdownValueViewModel> GetSystemTags(DataCategoryEnum dateCategory = DataCategoryEnum.BetRecord)
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetSystemTags(userID, dateCategory);
            return query;
        }


        /// <summary>
        /// 後端的語系
        /// </summary>
        [Route("dropdown/BE-Languages")]
        [HttpGet]
        public List<DropdownValueViewModel> GetLanguage()
        {
            var query = AdminSysDropdownService.GetLanguage();
            return query;
        }


        /// <summary>
        /// 語系代碼 (zh-cn)
        /// </summary>
        [Route("dropdown/language-codes")]
        [HttpGet]
        public List<DropdownValueViewModel> GetLanguageCodes()
        {
            var query = AdminSysDropdownService.GetLanguageCodes();
            return query;
        }

        [Route("dropdown/users")]
        [HttpGet]
        public List<DropdownValueViewModel> GetUsers([FromQuery] LazyCommonRequestModel filter)
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetUsers(userID, filter);
            return query;
        }

        [Route("dropdown/player-tags")]
        [HttpGet]
        public List<DropdownValueViewModel> GeyPlayerTags()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetPlayerTags(userID);
            return query;
        }



        [Route("dropdown/player-tags/allow-manual-add")]
        [HttpGet]
        public List<DropdownValueViewModel> GeyAllowAddPlayerTags()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetAllowManualAddPlayerTags(userID);
            return query;
        }



        [Route("dropdown/events")]
        [HttpGet]
        public List<DropdownValueViewModel> GetEvents([FromQuery] LazyCommonRequestModel filter)
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetEvents(userID, filter);
            return query;
        }

        [Route("dropdown/event-types")]
        [HttpGet]
        public List<DropdownValueViewModel> GetEventTypes()
        {
            var query = AdminSysDropdownService.GetEventTypes();
            return query;
        }

        [Route("dropdown/event-template-types")]
        [HttpGet]
        public List<DropdownValueViewModel> GetEventTemplateTypes()
        {
            var query = AdminSysDropdownService.GetEventTemplateTypeEnum();
            return query;
        }

        [Route("dropdown/event-settings-for-manual-add")]
        [HttpGet]
        public List<DropdownValueViewModel> GetAllowManualAddEventSettings()
        {
            var query = AdminSysDropdownService.GetAllowManualAddEventSettings();
            return query;
        }

        [Route("dropdown/normal-active-status")]
        [HttpGet]
        public List<DropdownValueViewModel> GetNormalActiveStatus()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetNormalActiveStatus(userID);
            return query;
        }

        [Route("dropdown/enable-disable-status")]
        [HttpGet]
        public List<DropdownValueViewModel> GetEnableDisableStatus()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetEnableDisableStatus();
            return query;
        }


        [Route("dropdown/event-content-color-type")]
        [HttpGet]
        public List<DropdownValueViewModel> GetEventContentColorType()
        {
            var query = AdminSysDropdownService.GetEventContentColorType();
            return query;
        }

        [Route("dropdown/fish-log-types")]
        [ApiConfig(IsHeaderLogEnable = false, IsResponseLogEnable = false)]
        [HttpGet]
        public List<DropdownValueViewModel> GetFishLogTypes()
        {
            var query = AdminSysDropdownService.GetAGGFishMoneyRecordTypeEnum();
            return query;
        }

        [Route("dropdown/normal-yes-no-notselect-status")]
        [HttpGet]
        public List<DropdownValueViewModel> GetNormalYesOrNoOrNorSelectEnum()
        {
            var query = AdminSysDropdownService.GetNormalYesOrNoOrNorSelectEnum();
            return query;
        }


        [Route("dropdown/visibility-status")]
        [HttpGet]
        public List<DropdownValueViewModel> GetVisibleStatusEnum()
        {
            var query = AdminSysDropdownService.GetVisibleStatusEnum();
            return query;
        }

        [Route("dropdown/EventContent-LayoutType")]
        [HttpGet]
        public List<DropdownValueViewModel> GetEventContentLayoutType()
        {
            var query = AdminSysDropdownService.GetEventContentLayoutType();
            return query;
        }


        [Route("dropdown/event-prize-types")]
        [HttpGet]
        public List<DropdownValueViewModel> GetEventPrizeType()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetEventPrizeType(userID);
            return query;
        }


        [Route("dropdown/agg-event-prize-types")]
        [HttpGet]
        public List<DropdownValueViewModel> GetSupportAGGEventPrizeType()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetAGGEventPrizeType(userID);
            return query;
        }


        [Route("dropdown/link-types")]
        [HttpGet]
        public List<DropdownValueViewModel> GetLinkTypes()
        {
            var query = AdminSysDropdownService.GetLinkType();
            return query;
        }


        [Route("dropdown/announcement-types")]
        [HttpGet]
        public List<DropdownValueViewModel> GetAnnouncementType()
        {
            var query = AdminSysDropdownService.GetAnnouncementType();
            return query;
        }


        [Route("dropdown/vendor-config")]
        [HttpGet]
        public List<DropdownValueViewModel> GetVendorConfigs([FromQuery] QueryVendorConfigApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetVendorConfig(userID, filter);
            return query;
        }


        [Route("dropdown/vendor-config-currency-display")]
        [HttpGet]
        public List<DropdownValueViewModel> GetVendorConfigsCurrencyDisplay([FromQuery] QueryVendorConfigApiRequestParameterModel filter)
        {
            filter = filter ?? new QueryVendorConfigApiRequestParameterModel();
            filter.IsShowCurrencyNameOnly = true;

            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetVendorConfig(userID, filter);
            return query;
        }


        /// <summary>
        /// 支援agg特殊API的遊戲商 (e.g. RTP設定、強制結算...etc)
        /// </summary>
        [Route("dropdown/vendor-config-support-aggAPI")]
        [HttpGet]
        public List<DropdownValueViewModel> GetVendorConfigsWithAGGApiSupport([FromQuery] QueryVendorConfigApiRequestParameterModel filter)
        {
            var myFilter = new QueryVendorConfigApiRequestParameterModel
            {
                VendorIDsText = string.Join(",", SystemConstant.SlotsExtensionApiSupportVendors)
            };

            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetVendorConfig(userID, myFilter);
            return query;
        }


        [Route("dropdown/agg-freespin-status")]
        [HttpGet]
        public List<DropdownValueViewModel> GetAGGFreespinStatus()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetAGGFreespinStatusEnum(userID);
            return query;
        }


        [Route("dropdown/credit-change-type")]
        [HttpGet]
        public List<DropdownValueViewModel> GetMerchantCreditChangeType()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetMerchantCreditChangeTypeEnum();
            return query;
        }


        [Route("dropdown/vendor-config/support-fetch-report")]
        [HttpGet]
        public List<DropdownValueViewModel> GetVendorConfigSuuportFetchReportAPI([FromQuery] QueryVendorConfigRequestViewModel filter)
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetVendorConfigSuuportFetchReportAPI(userID, filter);
            return query;
        }

        [Route("dropdown/root-agents")]
        [HttpGet]
        public List<DropdownValueViewModel> GetRootAgents()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetRootAgents(userID);
            return query;
        }

        [Route("dropdown/merchant-select-template")]
        [HttpGet]
        public List<DropdownValueViewModel> GetMerchantSelectTemplate([FromQuery] LazyCommonRequestModel filter)
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetMerchantSelectTemplate(userID, filter);
            return query;
        }



        [Route("dropdown/by-type-name/{typeName}")]
        [HttpGet]
        public List<DropdownValueViewModel> GetEnumsDropdownByTypeName(string typeName)
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetEnumsDropdownByTypeName(typeName);
            return query;
        }

        [Route("dropdown/bo-announcement-types")]
        [HttpGet]
        public List<DropdownValueViewModel> GetBOAnnouncementType()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetBOAnnouncementType();
            return query;
        }


        [Route("dropdown/bo-announcement-hintTypes")]
        [HttpGet]
        public List<DropdownValueViewModel> GetBOAnnouncementHintType()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetBOAnnouncementHintType();
            return query;
        }



        [Route("dropdown/assign-types")]
        [HttpGet]
        public List<DropdownValueViewModel> GetAdminSysAssignTypes()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetAdminSysAssignTypes(userID);
            return query;
        }


        [Route("dropdown/user-assign-merchant-mode")]
        [HttpGet]
        public List<DropdownValueViewModel> GetUserAssignMerchantMode()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetUserAssignMerchantMode();
            return query;
        }

        [Route("dropdown/sort-direction")]
        [HttpGet]
        public List<DropdownValueViewModel> GetSortDirection()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetSortDirection();
            return query;
        }

        [Route("dropdown/latest-month")]
        [HttpGet]
        public List<DropdownValueViewModel> GetLatestMonth([FromQuery] LazyCommonRequestModel filter)
        {
            var query = AdminSysDropdownService.GetLatestMonth(filter.Count);
            return query;
        }


        [Route("dropdown/sort-amount-columns")]
        [HttpGet]
        public List<DropdownValueViewModel> GetAmountColumns()
        {
            int userID = GetCurrentUserID();
            var query = AdminSysDropdownService.GetAmountColumns();
            return query;
        }

    }
}
