using target.Models.Table;
using target.Models.ViewModels.API.AdminSys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class VendorConfigViewModel
    {
        public int VendorConfigID { get; set; }
        public int VendorID { get; set; }
        public string VendorName { get; set; }
        public string SupportCurrency { get; set; }

        public bool IsTestEnvironment { get; set; }
        public bool IsSkipFetchSummary { get; set; }
        public string Name { get; set; }
        public string Memo { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UpdateBy { get; set; }

        /// <summary>
        /// 已分配使用的商戶數
        /// </summary>
        public int NumberOfMerchants { get; set; }



        public bool IsVipLevelSettingNotExist { get; set; }

        /// <summary>
        /// 三方遊戲商的識別碼/商戶代號
        /// </summary>
        public string AssignVendorID { get; set; }

        /// <summary>
        /// 三方遊戲商針對此配置的溝通用名詞 (e.g. VPlus是稱為 appID)
        /// </summary>
        public string AssignGameVendorDefineName { get; set; }


        /// <summary>
        /// 1外幣可以兌換多少台幣 (e.g. 1 USD = 30 TWD)
        /// </summary>
        public decimal ExchangeRateToTWD { get; set; }

        /// <summary>
        /// 已分配使用的商戶IDs
        /// (此欄位只有DetailAPI才有值) 
        /// </summary>
        public List<int> MappingMerchantIDs { get; set; }

        /// <summary>
        /// 已分配使用的商戶
        /// (此欄位只有DetailAPI才有值) 
        /// </summary>
        public List<VendorConfigMappingMerchantViewModel> MappingMerchants { get; set; }

    }

    public class VendorConfigMappingMerchantViewModel
    {
        public int MerchantID { get; set; }

        public string MerchantName { get; set; }

        public string Currency { get; set; }
    }


    public class VendorConfigMappingAddRequestModel
    {
        public int VendorConfigID { get; set; }

        public int MerchantID { get; set; }

    }



    public class VendorConfigDeleteRequestModel
    {
        public int VendorConfigID { get; set; }

        public int MerchantID { get; set; }

    }


    public class VendorConfigCreateRequestModel
    {
       
        public string Name { get; set; }


        /// <summary>
        /// GameVendorTypeEnum
        /// </summary>
        public int GameVendorID { get; set; }


        public bool IsTestEnvironment { get; set; }

    }



    public class VendorConfigCopyRequestModel
    {

        public string Name { get; set; }

        /// <summary>
        /// 以誰當基準複製
        /// </summary>
        public int CopyFromVendorConfigID { get; set; }


        public string SupportCurrency { get; set; }

        public string AssignVendorID { get; set; }

        /// <summary>
        /// 非必填 (服務器加密金鑰)
        /// </summary>
        public string AssignSecretKey { get; set; }
    }


    public class MerchantVendorConfigSettingsModel
    {
        public int MerchantID { get; set; }

        /// <summary>
        /// MerchantTypeEnum
        /// </summary>
        public int MerchantType { get; set; }

        public string MerchantTypeText { get; set; }


        public string MerchantName { get; set; }

        public List<MerchantVendorConfigSettingItemViewModel> Mappings { get; set; }
    }


    public class MerchantVendorConfigSettingItemViewModel
    {
        public int GameVendor { get; set; }

        public string GameVendorName { get; set; }

        public int EnableGames { get; set; }

        public int VendorConfigID { get; set; }

        public string VendorConfigName { get; set; }

        public bool IsAssignAnyGames { get; set; }

        public List<DropdownValueViewModel> DropdownItems { get; set; }

    }


    public class MerchantVendorConfigSettingAndEnableGamesItemViewModel
    {
        public int GameVendor { get; set; }

        public string GameVendorName { get; set; }

        public int EnableGames { get; set; }

        public int VendorConfigID { get; set; }

        public string VendorConfigName { get; set; }

        public bool IsWarning { get; set; }

        public decimal ConvertRate { get; set; }

    }



    public class MerchantVendorConfigSettingUpdateViewModel
    {
        public int MerchantID { get; set; }

        public int GameVendor { get; set; }

        public int VendorConfigID { get; set; }

    }




}
