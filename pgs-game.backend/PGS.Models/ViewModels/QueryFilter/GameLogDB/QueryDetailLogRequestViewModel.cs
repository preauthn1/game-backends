using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryPerGameDetailLogApiRequestParameterModel : QueryDetailLogRequestViewModel
    {

        public int? PlayerID { get; set; }

        public int? GameID { get; set; }

        public int? ServerID { get; set; }


        /// <summary>
        /// 不提供模糊查詢
        /// </summary>
        public string PlayerName { get; set; }


        [SwaggerIgnore]
        public string Lang { get; set; }


        [SwaggerIgnore]
        public string AccessToken { get; set; }


        /// <summary>
        /// 同時支援兩種格式的單號查詢 (後端會判斷要查哪一種)
        /// </summary>
        public string TransactionNoOrThirdNo { get; set; }
    }



    public class QueryDetailLogRequestViewModel : PaginationParameter
    {
        /// <summary>
        /// GameTime的意思
        /// </summary>
        public DateTime? GameStartTimeBegin { get; set; }

        /// <summary>
        /// GameTime的意思
        /// </summary>
        public DateTime? GameStartTimeEnd { get; set; }


        public DateTime? GameTimeMatch { get; set; }


        /// <summary>
        /// PGS的投注單號
        /// </summary>
        public string TransactionNo { get; set; }

        public List<string> ListOfTransactionNo { get; set; }

        public string ThirdPartyBetRecordNo { get; set; }

        /// <summary>
        /// 對應 SystemTagEnum
        /// </summary>
        public int? TagID { get; set; }

        public int? FilterMerchantIDByHierarchyMerchant { get; set; }

        public List<int> FilterMerchantIDsByMultipleHierarchyMerchant { get; set; }

        public decimal? PayoutAmountLargeThan { get; set; }


        public bool IsJackpotOnly { get; set; }


        [SwaggerIgnore]
        public List<int> PlayerIDs { get; set; }

        [SwaggerIgnore]
        public List<int> GameIDs { get; set; }

        public int? GameVendorID { get; set; }
    

        [SwaggerIgnore]
        public List<int> ServerIDs { get; set; }

        [SwaggerIgnore]
        public List<int> MerchantIDs { get; set; }

        public int? MerchantID { get; set; }

        /// <summary>
        /// PK主鍵
        /// </summary>
        public long? PerGameDetailLogID { get; set; }

        public List<long> PerGameDetailLogIDs { get; set; }


        public bool IsLockQuery { get; set; }

        /// <summary>
        /// 追蹤紀錄是哪邊呼叫的
        /// </summary>
        public string DebugSourceMethod { get; set; }

    }



    public class PerGameDeleteProcess_UpdateMerchantPlayerParameter
    {
        public long Id { get; set; }

        public DateTime GameTime { get; set; }

        public int NewMerchantID { get; set; }

        public int NewPlayerID { get; set; }

        public string NewPlayerName { get; set; }

    }




}
