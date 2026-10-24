using target.Models;
using System;
using System.Collections.Generic;
using System.Text;
using target.Utility.Extensions;
using target.Models.Models;
using target.Models.Enums;
using target.Models.Table;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;

namespace target.Utility.Helper
{
    /// <summary>
    /// 後端小工具站台 (target.Backend.Utility專案) 專用的
    /// </summary>
    public class BackendUtilityHelper
    {
        public static List<string> IgnoreExcelColumnsKeyword()
        {
            //要排除的多語系key (可能是名稱已更改)
            return new List<string>
            {
                "Enums_EventTemplateTypeEnum_QuestEvent_SpecificGame_ByTotalRound_GetLuckWheelTicket_10_Quest",
                "Enums_EventTemplateTypeEnum_QuestEvent_AllInOneTotalPoint_GetLuckWheelTicket_10_Quest",
                "Enums_UserOperateTypeEnum_ManualDepositWalletAmount",
                "Enums_EventTemplateTypeEnum_QuestEvent_GetLuckWheelTicket_3_Quest",
                "Enums_EventTemplateTypeEnum_QuestEvent_GetLuckWheelTicket_5_Quest",
            };
        }

        /// <summary>
        /// 定義了 target.Backend.Utility專案中  哪些enums要自動產生多語系
        /// </summary>
        public static List<Type> GetAutoGeneratei18nKeysEnums()
        {
            return new List<Type>
            {
                typeof(MerchantCustomizedSettingKeyEnum),
                typeof(MerchantLoadingStyleModeEnum),
                typeof(MerchantTypeEnum),
                typeof(NormalActiveStatus),
                typeof(SystemConfigSettingEnum),
                typeof(SystemMaintainStatusEnum),
                typeof(DataCategoryEnum),
                typeof(UserOperateTypeEnum),
                typeof(NormalSuccessStatusEnum),
                typeof(VendorConfigSettingEnum),
                typeof(GameListEnum),
                typeof(GameTypeEnum),
                typeof(GameVendorTypeEnum),
                typeof(BetRecordStatusEnum),
                typeof(SystemTagEnum),
                typeof(TransactionTypeEnum),
                typeof(LanguageEnum),
                typeof(EventPrizeRecordStatusEnum),
                typeof(EventPrizeTypeEnum),
                typeof(EventRuleTypeEnum),
                typeof(CurrencyEnum),
                typeof(EventCustomizedSettingKeyEnum),
                typeof(EventTypeEnum),
                typeof(EventTemplateTypeEnum),
                typeof(EventMultiplePrizeTypeEnum),
                typeof(OneToManyCustomizedSettingTypeEnum),
                typeof(OneToManyCustomizedSettingItemTypeEnum),
                typeof(AGGJackpotGroupEnum),
                typeof(LinkTypeEnum),
                typeof(AnnouncementTypeEnum),
                typeof(VisibleStatusEnum),
                typeof(EventContentLayoutTypeEnum),
                typeof(ChartDataTypeEnum),
                typeof(HDTypeEnum),
                typeof(LobbyStyleEnum),
                typeof(MerchantCreditChangeTypeEnum),
                typeof(AgentAggregativeTypeEnum),
                typeof(EventCounterOrderByEnum),
                typeof(GameOpenModeEnum),
                typeof(SingleWalletPlatformApiLogicTypeEnum),
                typeof(AdminSysExtraFilterEnum),
                typeof(AGGFishMoneyRecordTypeEnum),
                typeof(SortDirectionEnum),
                typeof(NormalYesOrNoOrNorSelectEnum),
                typeof(BOAnnouncementTypeEnum),
                typeof(BOAnnouncementHintTypeEnum),
                typeof(AssignTypeEnum),
                typeof(MessageAssignTypeEnum),
                typeof(LobbyBGMTypeEnum),
                typeof(LobbyGameSortEnum),
                typeof(GameListExtraFilterTypeEnum),
                typeof(EventContentColorStypeType),
                typeof(EventContentBrandingType),
                typeof(SpecialMerchantFilterType),
                typeof(StatReportSearchColumnType),
                typeof(GameListCustomizedSettingKeyEnum),
                typeof(UserAssignMerchantModeEnum),
                typeof(GameImageStyleTypeEnum),
                typeof(OneTimesJobTypeEnum),
                typeof(OneTimesJobStatusEnum),
                typeof(MerchantCopyExtraSettingEnum),
                typeof(TagTypeEnum),
                typeof(TelegramAnnouncementTemplateEnum),
                typeof(LobbyMainApiVersionEnum),
                typeof(IMChannelTypeEnum),
                typeof(IMChannelCustomizedSettingKeyEnum),
                typeof(SpecialFilterTypeOfSwApiRequestLog),
                typeof(SpecialFilterTypeOfUsers),
                typeof(SpecialFilterTypeOfIPAddressTemplate),
                typeof(SpecialFilterTypeOfIMChannels),
                typeof(SpecialFilterTypeOfApiReqeustLog),
                typeof(AccountingInvoiceExportCategoryEnum),
                typeof(BetRecordMerchantNotifyQueueTypeEnum),
                typeof(SpecialFilterTypeOfEvent),
                typeof(AgentSpecialReportTypeEnum),
                typeof(ElasticLogTableTypeEnum),
                typeof(AGGFreespinStatusEnum),
            };
        }

    }
}
