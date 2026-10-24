using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class MerchantCreditViewModel
    {
        public int MerchantID { get; set; }

        public string Name { get; set; }

        public string Currency { get; set; }

        public string CurrencyText { get; set; }

        public string ChargeCurrency { get; set; }

        public string ChargeCurrencyText { get; set; }

        public int WalletMode { get; set; }

        public int Status { get; set; }

        public string StatusText { get; set; }

        public int MerchantType { get; set; }

        public string MerchantTypeText { get; set; }

        public int ParentMerchantID { get; set; }

        public string ParentMerchantName { get; set; }

        public string WalletModeText { get; set; }

        public string ApiSecretKey { get; set; }


        public decimal CreditAmount { get; set; }

        public bool IsCreditModeEnable { get; set; }

        public int NumberOfAccount { get; set; }

        public bool IsUnlimit { get; set; }

        /// <summary>
        /// 是否為自己的商戶 若編輯信用額度 (前端要禁止點擊)
        /// </summary>
        public bool IsSelfCredit { get; set; }

        public bool IsSelfMerchant 
        { 
            get
            {
                return this.IsSelfCredit;
            }
        }

        public bool IsSingleWallet { get; set; }


        public bool IsSingleWalletDoaminNotSetup { get; set; }


        public bool IsProductionEnvType { get; set; }

        public bool IsHintTransferConvertRate { get; set; }


        public decimal TransferConvertRate { get; set; }


        public List<string> WhiteListServerIPs { get; set; }

        public int EnableGamesCount { get; set; }

        public int EnableGamesCountOfParent { get; set; }

        public int NotEnableGamesCount { get; set; }



        public DateTime CreateTime { get; set; }


        /// <summary>
        /// 只有特定關鍵字才會顯示
        /// </summary>
        public string MemoDisplay { get; set; }

        public string Memo { get; set; }

        public string Memo_ThisIsDevKey { get; set; }

        public string Memo_DevBETACoin { get; set; }

        public string Memo_WhereToDownloadDocument { get; set; }


        public string AgentMemo { get; set; }

        /// <summary>
        /// 可能要注意多語系
        /// </summary>
        public List<string> ExtraTags { get; set; }

        public List<string> ExtraTagsBlackColor { get; set; }

    }



    public class MerchantCreditExportViewModel : MerchantCreditViewModel
    {
        public string UserAccount { get; set; }

        public string UserPassword { get; set; }

        public string BackOfficeUrl { get; set; }

        public string SingleWalletPgsIdentity { get; set; }

        public string ApiUrl { get; set; }

        public string VendorCode { get; set; }

        public bool IsEndpointNotSetup { get; set; }
    }



    public class MerchantCreditUpdateRequestModel
    {
        public int MerchantID { get; set; }

        /// <summary>
        /// 請參考 PlusOrMinusEnum
        /// </summary>
        public int PlusType { get; set; }

        public decimal UpdateAmount { get; set; }

        public string Memo { get; set; }
    }


    public class MerchantCreditUpdatePreviewViewModel
    {
        /// <summary>
        /// 當前登入者
        /// </summary>
        public MerchantCreditPreviewItemModel MyCredit { get; set; }

        /// <summary>
        /// 更新對象的
        /// </summary>
        public MerchantCreditPreviewItemModel TargetMerchantCredit { get; set; }
    }


    public class MerchantCreditPreviewItemModel
    {
        public int MerchantID { get; set; }

        public string MerchantName { get; set; }

        public string Currency { get; set; }

        public bool IsUnlimit { get; set; }

        public bool IsSuperAdmin { get; set; }

        public bool IsMultipleHierarchy { get; set; }

        public decimal CreditAmount { get; set; }
    }


    public class MerchantCreditEazyCheckModel
    {
        public bool IsUnlimit { get; set; }

        public bool IsSuperAdmin { get; set; }

        public decimal CreditAmount { get; set; }
    }



}
