using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.API.AdminSys;

namespace target.BL.Interface
{

    public interface IAdminSysDropdownService
    {
        bool CheckIsSuperAdminOrg(int userID);

        List<DropdownValueViewModel> GetDropdownsByEnum<T>() where T : System.Enum;

        List<DropdownValueViewModel> GetGeneralTags(LazyCommonRequestModel filter);

        List<DropdownValueViewModel> GetNormalActiveStatus(int userID);

        List<DropdownValueViewModel> GetEnableDisableStatus();

        List<DropdownValueViewModel> GetVisibleStatusEnum();

        List<DropdownValueViewModel> GetEventContentLayoutType();

        List<DropdownValueViewModel> GetGames(int userID, QueryGameListApiRequestParameter filter);

        List<DropdownValueViewModel> GetGameTypes();

        List<DropdownValueViewModel> GetAgentAggregativeType();

        List<DropdownValueViewModel> GetInternalTestPlayerAccounts();

        List<DropdownValueViewModel> GetServers(int userID, QueryServerListApiRequestParameterModel filter);

        List<DropdownValueViewModel> GetBetRecordStatus();

        List<DropdownValueViewModel> GetEventPrizeRecordStatusEnum();

        List<DropdownValueViewModel> GetAllowManualAddEventSettings();

        List<DropdownValueViewModel> GetMerchantSpecialFilterTypes();

        List<DropdownValueViewModel> GetStaReportSearchColumns(int userID , string reportType);

        List<DropdownValueViewModel> GetAvailableRoles(int userID);

        List<DropdownValueViewModel> GetIMChannels(int userID);

        List<DropdownValueViewModel> GetCurrency(int userID);

        List<DropdownValueViewModel> GetRoomLevels();

        List<DropdownValueViewModel> GetLanguage();

        List<DropdownValueViewModel> GetLanguageCodes();

        List<DropdownValueViewModel> GetEventCounterOrderByColumn();

        List<DropdownValueViewModel> GetMerchantCopyExtraSettingEnum();

        List<DropdownValueViewModel> GetInvoiceExportCategory();

        List<DropdownValueViewModel> GetUserOperateTypeEnum(int userID);

        List<DropdownValueViewModel> GetMerchantCustomizedSettingKeys();

        List<DropdownValueViewModel> GetAvailableMerchants(int userID, QueryDropdownMerchantRequestViewModel filter);

        List<DropdownValueViewModel> GetInternalMerchants(int userID);

        List<DropdownValueViewModel> GetMerchantsOfAllowCreateTestAccount(int userID);

        List<DropdownValueViewModel> GetHierarchyMerchants(int userID , LazyCommonRequestModel filter);

        List<DropdownValueViewModel> GetDynamicQueryItems();

        List<DropdownValueViewModel> GetMerchantTypes();

        List<DropdownValueViewModel> GetOneToManyTypes();

        List<DropdownValueViewModel> GetIPAddressTemplate(LazyCommonRequestModel filter);

        List<DropdownValueViewModel> GetOneTimesJobTypes();

        List<DropdownValueViewModel> GetTransactionTypes();

        List<DropdownValueViewModel> GetGameVendorTransferTypes();

        List<DropdownValueViewModel> GetAvailableOrgs(int userID);

        List<DropdownValueViewModel> GetGameVendorTypes(int userID);

        List<DropdownValueViewModel> GetGameListExtraFilterTypes(int userID);

        List<DropdownValueViewModel> GetGameVendorTypesExcludeSlot88(int userID);

        List<DropdownValueViewModel> GetThirdPartyVendors(int userID);

        List<DropdownValueViewModel> GetTimeZoneAreas();

        List<DropdownValueViewModel> GetPerGameTags(int userID);

        List<DropdownValueViewModel> GetPerGameTagsAllowManualAdd();

        List<DropdownValueViewModel> GetPlayerTags(int userID);

        List<DropdownValueViewModel> GetAllowManualAddPlayerTags(int userID);

        List<DropdownValueViewModel> GetUsers(int userID, LazyCommonRequestModel filter);

        List<DropdownValueViewModel> GetMerchantCreditChangeTypeEnum();

        List<DropdownValueViewModel> GetSystemTags(int userID , DataCategoryEnum dateCategory);

        List<DropdownValueViewModel> GetEvents(int userID , LazyCommonRequestModel filter);

        List<DropdownValueViewModel> GetEventTypes();

        List<DropdownValueViewModel> GetLinkType();

        List<DropdownValueViewModel> GetAnnouncementType();

        List<DropdownValueViewModel> GetEventContentColorType();

        List<DropdownValueViewModel> GetEventTemplateTypeEnum();

        List<DropdownValueViewModel> GetRootAgents(int userID);

        List<DropdownValueViewModel> GetEventPrizeType(int userID);

        List<DropdownValueViewModel> GetAGGEventPrizeType(int userID);

        List<DropdownValueViewModel> GetVendorConfig(int userID, QueryVendorConfigApiRequestParameterModel filter);

        List<DropdownValueViewModel> GetVendorConfigSuuportFetchReportAPI(int userID, QueryVendorConfigRequestViewModel filter);

        List<DropdownValueViewModel> GetMerchantSelectTemplate(int userID, LazyCommonRequestModel filter);

        List<DropdownValueViewModel> GetExtraFtilerType(int userID, LazyCommonRequestModel filter);

        List<DropdownValueViewModel> GetNormalYesOrNoOrNorSelectEnum();

        List<DropdownValueViewModel> GetAGGFishMoneyRecordTypeEnum();

        List<DropdownValueViewModel> GetAGGFreespinStatusEnum(int userID);

        List<DropdownValueViewModel> GetEnumsDropdownByTypeName(string typeName);
        
        List<DropdownValueViewModel> GetUserAssignMerchantMode();

        List<DropdownValueViewModel> GetSortDirection();

        List<DropdownValueViewModel> GetLatestMonth(int count);

        List<DropdownValueViewModel> GetAmountColumns();

        List<DropdownValueViewModel> GetBOAnnouncementType();

        List<DropdownValueViewModel> GetBOAnnouncementHintType();

        List<DropdownValueViewModel> GetAdminSysAssignTypes(int userID);
    }
}
