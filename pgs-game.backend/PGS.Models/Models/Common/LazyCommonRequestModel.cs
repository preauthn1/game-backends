using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    /// <summary>
    /// 偷懶用model (收錄各種request欄位)
    /// </summary>
    public class LazyCommonRequestModel
    {
        public decimal Amount { get; set; }

        public long TargetID { get; set; }

        public int TagID { get; set; }

        public int Size { get; set; }

        public string Table { get; set; }

        public string Token { get; set; }

        public string UUID { get; set; }

        public int Type { get; set; }

        public string TypeText { get; set; }

        public string Language { get; set; }

        public int? SplitMinutes { get; set; }

        public int? EventTemplateType { get; set; }

        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public string DisplayName { get; set; }

        public int MerchantID { get; set; }

        public int ParentMerchantID { get; set; }

        public int? IMChannelID { get; set; }

        public int HierarchyOfTargerMerchant { get; set; }        

        public int OneToManyCustomizedSettingID { get; set; }

        public string PlayerUniqueID { get; set; }

        public string UniqueID { get; set; }

        public int GameID { get; set; }

        public string GameIDsText { get; set; }

        public string MerchantIDsText { get; set; }

        public int? AggregativeGroup { get; set; }


        /// <summary>
        /// GameVendorTypeEnum
        /// </summary>
        public int GameVendorID { get; set; }

        public string GameCode { get; set; }

        public int EventID { get; set; }

        public int Count { get; set; }

        public int TemplateID { get; set; }

        public string TableName { get; set; }

        public int CategoryID { get; set; }

        public bool IsSync { get; set; }

        public bool IsAsync { get; set; }

        public bool IsEnableFlag1 { get; set; }

        public bool IsEnableFlag2 { get; set; }

        public List<int> GameIDs { get; set; }

        public List<int> VendorConfigIDs { get; set; }

        public List<int> MerchantIDs { get; set; }

        public List<int> RoleIDs { get; set; }

        public List<string> ExcludeColumns { get; set; }


        public int Status { get; set; }

        public int YesOrNo { get; set; }

        public string TransactionNo { get; set; }

        public string BetRecordNoPGS { get; set; }

        public string RawText { get; set; }

        public string WhereSQL { get; set; }

        public List<string> TransactionNoList { get; set; }

        public List<string> ExcludeTransactionNoList { get; set; }

        public List<string> SpecificTransactionNoList { get; set; }

        public int VendorConfigID { get; set; }

        public string DateText { get; set; }

        public string Memo { get; set; }

        public DateTime? Begin { get; set; }

        public DateTime? End { get; set; }

        /// <summary>
        /// 台灣時間日期
        /// </summary>
        public DateTime? BeginDateTW { get; set; }

        public DateTime? EndDateTW { get; set; }


        public bool IsTriggerExtraProcess { get; set; }

        public List<int> PlayerIDs { get; set; }

        public int ExpectIntValue { get; set; }

        /// <summary>
        /// 最高階層 <= 判斷式 (總代 = 1層)
        /// </summary>
        public int? MaxLayer { get; set; }

        public int? OrganizationType { get; set; }


    }


    public class TextParseToListIntResultModel
    {
        public List<int> ids { get; set; }
    }



    public class LazyCommonResultViewModel
    {
        public LazyCommonResultViewModel()
        {

        }

        public LazyCommonResultViewModel(string msg)
        {
            this.Message = msg;
        }

        public string Message { get; set; }

        public int ResultID { get; set; }

        public long ResultNumber { get; set; }

        public List<string> ResultIPAddresss { get; set; }
    }

        
}
