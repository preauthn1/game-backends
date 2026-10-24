using target.Models.CoreLibs;
using target.Models.Table;
using target.Models.ViewModels;
using System;

namespace target.BL.Interface
{
    public interface IAdminSysComplexService
    {
        string LaunchGameByAdminSys(int userID, InternalTestBundleRequestModel model);

        CustomerServiceUtilityResultModel GetCustmoerServiceUtilityInfo(int userID);

        CRUDResultModel CopyAndBatchCreateNewMerchant(int userID, MerchantBatchCopyAndCreateRequestModel model);

        CRUDResultModel CopyMerchantByAgent(int userID, MerchantCopyByAgentRequestModel model);

        string BatchCreateNewMerchantByEachParentAndPrefixName(int userID, MerchantBatchCopyAndCreateRequestModel model);

        PublicApiBetRecordReplayInfoViewModel GetBetRecordReplayInfoByGameVendor(LazyCommonRequestModel model);

        LuckyDrawResultViewModel ManualSendEventBonus(int userID, ManualSendEventBonisRequestModel model);

        CRUDResultModel ChangeGameUrlDomainByCS(int userID);

        CRUDResultModel CreateDevMerchantByAM(int userID, LazyCommonRequestModel model);

        CheckPlayerMoneyResultViewModel CheckPlayerMoney(int userID, CheckPlayerMoneyRequestModel model);
    }



}
