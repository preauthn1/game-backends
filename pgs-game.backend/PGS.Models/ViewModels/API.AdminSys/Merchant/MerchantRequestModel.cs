using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class MerchantViewModel
    {
        public int MerchantID { get; set; }

        public Guid? MerchantGuid { get; set; }

        public int Type { get; set; }

        public string MerchantTypeName { get; set; }

        public int? ParentMerchantID { get; set; }

        public string ParentMerchantName { get; set; }

        public int HierarchyLayer { get; set; }

        public string Name { get; set; }

        public string Memo { get; set; }

        public string Memo2 { get; set; }

        public string Code { get; set; }

        public string Currency { get; set; }

        public string CurrencyText { get; set; }

        public string ChargeCurrency { get; set; }

        public string ChargeCurrencyText { get; set; }



        public string DefaultLanguage { get; set; }

        public decimal? DefaultBalance { get; set; }

        public string IMSoftwareType { get; set; }

        public string IMChannelName { get; set; }


        public int Status { get; set; }

        public string StatusText { get; set; }

        public string ApiSecretKey { get; set; }

        public string ApiValidatePlayerSessionUrl { get; set; }

        public string ApiSingleWalletGetAmountUrl { get; set; }

        public string SearchExtraSettingValue { get; set; }


        public int WalletMode { get; set; }

        public string WalletModeText { get; set; }

        public string CreateBy { get; set; }

        public DateTime CreateTime { get; set; }

        public string UpdateBy { get; set; }

        public DateTime UpdateTime { get; set; }

        public string ExternalAPI_TestAccountLink_Dev { get; set; }

        public string ExternalAPI_TestAccountLink_Pro { get; set; }

        public int NumberOfGames { get; set; }

        public int NumberOfServers { get; set; }

        /// <summary>
        /// 前端關鍵字搜尋用
        /// </summary>
        public string KeywordForJsSearch { get; set; }
    }


    public class MerchantCopyInfoPreviewViewModel
    {
        public MerchantViewModel Info { get; set; }

        public string SingleWalletIdentityToken { get; set; }

        public bool IsSingleWalletDoaminNotSetup { get; set; }

        public bool IsW88OperatorCodeNotSetup { get; set; }

        public bool IsEnableGameEmpty { get; set; }

        public bool IsEnablePGSGameOnlyButServerEmpty { get; set; }

        public bool IsAnyPgsGameNotMappingServer { get; set; }

        public bool IsCodeNotSetup { get; set; }

        public bool IsCreditMode { get; set; }

        /// <summary>
        /// 若被標記為true。controller層會呼叫加值信用額度
        /// </summary>
        public bool IsDepositCreditAfterProcess { get; set; }


        public int? ParentMerchantID { get; set; }


        public int MerchantID { get; set; }


        /// <summary>
        /// 複製商戶資訊前
        /// </summary>
        public List<string> Memo_CN { get; set; }

        public List<string> Memo_EN { get; set; }

        public List<string> ExtraHintTags { get; set; }

        /// <summary>
        /// 要勾選額外信息才會顯示的資訊
        /// </summary>
        public List<ExtraInfoItemModel> ExtraInfoItemsDefaultHidden { get; set; }

        public int EnablePgsGames { get; set; }

        public int EnableThirdPartyGames { get; set; }

        public int EnablePgsGameServers { get; set; }

        public string VendorConfigMappingsText { get; set; }

        public string SingleWalletCallbackEndpointUrl { get; set; }

        public string AdminsSysAccountPassword { get; set; }

        /// <summary>
        /// 遊戲素材
        /// </summary>
        public string GamesMaterialUrl { get; set; }

        public string VendorApiUrl { get; set; }

        public string IMChannelName { get; set; }

        public int IMChannelID { get; set; }

        /// <summary>
        /// 可能代碼為: target、bwg
        /// </summary>
        public string VendorCode { get; set; }

        public decimal PGSConvertRate { get; set; }

        /// <summary>
        /// 是否已完成 金鑰提供
        /// </summary>
        public bool IsProvideComplete { get; set; }

        public bool IsMerchantTypeForProduction { get; set; }

        public bool IsMerchantHierarchy { get; set; }


        public string CredentialMessage_CN { get; set; }

        public string CredentialMessage_EN { get; set; }


        public List<MappingVendorConfigViewModel> VendorConfigMappings { get; set; }

        public List<MerchantVendorConfigSettingAndEnableGamesItemViewModel> EnableGameVendorGames { get; set; }

    }



    public class ExtraInfoItemModel
    {
        public string IconStyle { get; set; }

        public string Text { get; set; }

        public int CustomizedSettingKey { get; set; }
    }







    public class MappingVendorConfigViewModel
    {


        public string VendorConfigText { get; set; }

        public int VendorConfigID { get; set; }

        public int VendorID { get; set; }

        public bool IsWarning { get; set; }
    }


    public class MerchantStatusAndHierarchyPreviewInfoModel
    {

        public int MerchantID { get; set; }

        public string MerchantName { get; set; }

        public int Status { get; set; }

        public int NumberOfChilds { get; set; }

    }


}
