using target.Models.Enums;
using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class QueryTransactionRecordRequestParameterModel : QueryTransactionRecordRequestViewModel
    {
        /// <summary>
        /// 商戶ID
        /// </summary>
        public int? MerchantID { get; set; }


        /// <summary>
        /// 玩家ID
        /// </summary>
        public int? PlayerID { get; set; }

        /// <summary>
        /// 帳號
        /// </summary>
        public string PlayerName { get; set; }

    }

    public class QueryTransactionRecordRequestViewModel : PaginationParameter
    {
        [SwaggerIgnore]
        public List<int> MerchantIDs { get; set; }

        [SwaggerIgnore]
        public List<int> PlayerIDs { get; set; }

        /// <summary>
        /// 透過代理樹狀結構去篩選MerchantIDs (跟直接用MerchantIDs去 in query速度有差距)
        /// </summary>
        public int? FilterMerchantIDByHierarchyMerchant { get; set; }


        public List<int> FilterMerchantIDsByMultipleHierarchyMerchant { get; set; }


        public decimal? AmountMatch { get; set; }


        /// <summary>
        /// 遊戲
        /// </summary>
        public int? GameID { get; set; }


        /// <summary>
        /// 現金流通類型 (TransactionTypeEnum)
        /// </summary>
        public int? TransactionType { get; set; }


        /// <summary>
        /// 現金流通類型 (TransactionTypeEnum)
        /// </summary>
        public List<int> TransactionTypeList { get; set; }


        /// <summary>
        /// IP地址
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// 創建時間起
        /// </summary>
        public DateTime? CreateTimeBegin { get; set; }



        public string TargetTableName { get; set; }



        /// <summary>
        /// 創建時間迄
        /// </summary>
        public DateTime? CreateTimeEnd { get; set; }


        public string ReferenceOrderNo { get; set; }


        public List<string> ReferenceOrderNoList { get; set; }

    }

    public class QueryTransactionRecordSPRequestViewModel : PaginationParameter
    {
        /// <summary>
        /// 玩家ID
        /// </summary>
        public int? PlayerID { get; set; }
        /// <summary>
        /// 遊戲
        /// </summary>
        public GameListEnum? GameID { get; set; }
        /// <summary>
        /// 商戶ID
        /// </summary>
        public int? MerchantID { get; set; }
        /// <summary>
        /// 商戶IDs
        /// </summary>
        public List<int> MerchantIDs { get; set; }
        /// <summary>
        /// 現金流通類型
        /// </summary>
        public TransactionTypeEnum? TransactionType { get; set; }
        /// <summary>
        /// 錢包類型
        /// </summary>
        public WalletTypeEnum? WalletType { get; set; }
        /// <summary>
        /// IP地址
        /// </summary>
        public string IPAddress { get; set; }
        /// <summary>
        /// 創建時間起
        /// </summary>
        public DateTime? CreateTimeBegin { get; set; }
        /// <summary>
        /// 創建時間迄
        /// </summary>
        public DateTime? CreateTimeEnd { get; set; }
    }
}
