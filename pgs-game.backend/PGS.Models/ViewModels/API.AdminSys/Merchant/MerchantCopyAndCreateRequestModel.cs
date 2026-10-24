using target.Models.Enums;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class MerchantCopyAndCreateRequestModel
    {

        //從指定的商戶複製設定
        public int SourceMerchantID { get; set; }

        public string MerchantName { get; set; }

        public string Memo { get; set; }

    }



    public class BaseMerchantCopyModel
    {

        public int? ParentMerchantID { get; set; }


        public int? WalletMode { get; set; }


        public int? IMChannelID { get; set; }


        /// <summary>
        /// 如果前端有自己勾選 以勾選的為主
        /// </summary>
        public List<int> GameIDs { get; set; }


        /// <summary>
        /// AM自己填的資訊
        /// </summary>
        public string AgentMemo { get; set; }

    }



    public class MerchantCopyByAgentRequestModel : BaseMerchantCopyModel
    {
        //有些欄位在 BaseMerchantCopyModel
        //有些欄位在 BaseMerchantCopyModel
        //有些欄位在 BaseMerchantCopyModel

        public string Name { get; set; }

        public string Account { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public int RoleID { get; set; }

        public int OrganizationID { get; set; }

        public string WhiteListIPAddress { get; set; }

    }




    public class MerchantBatchCopyAndCreateRequestModel : MerchantCopyByAgentRequestModel
    {
        //有些欄位在 BaseMerchantCopyModel
        //有些欄位在 BaseMerchantCopyModel
        //有些欄位在 BaseMerchantCopyModel

        //從指定的商戶複製設定
        public int SourceMerchantID { get; set; }


        public List<string> MerchantNameList { get; set; }

        public string Memo { get; set; }


        public string Currency { get; set; }


        public bool IsCreateUserAccount { get; set; }


        /// <summary>
        /// 對應 MerchantCopyExtraSettingEnum
        /// </summary>
        public List<int> ExtraSettingFlag { get; set; }


        public ExtraConfigForCopyMerchant ExtraConfig { get; set; }


        public MerchantCopyCreateUserAccountExtraProperty CreateAccountInfo { get; set; }

    }



    public class ExtraConfigForCopyMerchant
    {

        /// <summary>
        /// 此欄位 BatchCreateNewMerchantByEachParentAndPrefixName 的情境才會用到
        /// </summary>
        public List<int> MultipleParentMerchantIDs { get; set; }


        public string PrefixName { get; set; }

    }



    

    public class MerchantCopyCreateUserAccountExtraProperty
    {
        public int OrganizationID { get; set; }

        /// <summary>
        /// 不要包含前綴
        /// </summary>
        public string Account { get; set; }

        public string Password { get; set; }

        public int RoleID { get; set; }

        public int MerchantID { get; set; }

        public bool IsHierarchyMode { get; set; }

        public bool IsSingleAdminAccount { get; set; }
    }



    public class MerchantBatchCopyCommandParseModel
    {
        public string MerchantName { get; set; }

        public MerchantWalletModeEnum WalletMode { get; set; }
    }




}
