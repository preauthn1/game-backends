using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class MerchantCreateRequestModel
    {

        public string Name { get; set; }
        public string Memo { get; set; }
        public string Currency { get; set; }
        public string DefaultLanguage { get; set; }
        public int Status { get; set; }
        public string ApiSecretKey { get; set; }
        public int WalletMode { get; set; }
        public string Code { get; set; }
        public int Type { get; set; }
        public MerchantCreateExtraSettingModel ExtraSettings { get; set; }

    }


    public class MerchantParentAgentValidateBeforeCreateParameter
    {
        public int ParentMerchantID { get; set; }

        public int? MerchantType { get; set; }

        public string Currency { get; set; }
    }



    public class MerchantCreateExtraSettingModel
    {
        public int? ParentMerchantID { get; set; }

        public int? CopyGameSettingFromMerchantID { get; set; }


        public List<SystemConfigSettingViewModel> CustomizedSettings { get; set; }
    }


    public class MerchantUpdateRequestModel
    {
        public int MerchantID { get; set; }
        public string Name { get; set; }
        public string Memo { get; set; }
        public string Currency { get; set; }
        public string DefaultLanguage { get; set; }
        public int Status { get; set; }
        public int WalletMode { get; set; }
        public string ApiSecretKey { get; set; }
        public string ApiValidatePlayerSessionUrl { get; set; }
        public string ApiSingleWalletGetAmountUrl { get; set; }
        public int Type { get; set; }

    }


    public class MerchantCompareRequestModel
    {

        public int LeftMerchantID { get; set; }

        public int RightMerchantID { get; set; }
    }

    public class MerchantFullInformationVIewModel
    {

        public int MerchantID { get; set; }
        public string Name { get; set; }
        public string Memo { get; set; }
        public string Currency { get; set; }
        public string ChargeCurrency { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public int MerchantType { get; set; }
        public string MerchantTypeText { get; set; }
        public int WalletMode { get; set; }
        public string WalletModeText { get; set; }
        public Guid? MerchantGuid { get; set; }


        public int TotalEnableEvents { get; set; }
        public int TotalEnableGames { get; set; }
        public int TotalEnableServers { get; set; }


        public List<MerchantGameSettingEntity> EnableGames { get; set; }
        public List<MerchantCustomizedSettingEntity> AdvanceSettings { get; set; }
        public List<VendorConfigEntity> GameVendorConfigs { get; set; }


    }





    }
