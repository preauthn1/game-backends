using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.DTO.GameMerchantApi.BG
{



    public class BGTransferRecordResponseModel : BGBaseResponseModel
    {       

        public BGResultResponseModel result { get; set; }

    }


    public class BGStatsResponseModel
    {
        public int userCount { get; set; }
        public int incomeAmount { get; set; }
        public int balanceAmount { get; set; }
        public int expenditureAmount { get; set; }
    }

    public class BGItemResponseModel
    {
        public object userLayerId { get; set; }
        public decimal amount { get; set; }
        public string loginId { get; set; }
        public string bizType { get; set; }
        public string cashflowId { get; set; }
        public string operateTime { get; set; }
        public object memo { get; set; }
        public object chargeNo { get; set; }
        public int userId { get; set; }
        public string accountItemName { get; set; }
        public decimal balance { get; set; }
        public object withdrawNo { get; set; }
        public bool cashflow { get; set; }
        public string bizId { get; set; }
        public string sn { get; set; }
        public int accountItem { get; set; }
        public object operatorId { get; set; }
    }

    public class BGResultResponseModel
    {
        public int total { get; set; }
        public int pageIndex { get; set; }
        public BGStatsResponseModel stats { get; set; }
        public int pageSize { get; set; }
        public object etag { get; set; }
        public int page { get; set; }
        public List<BGItemResponseModel> items { get; set; }
    }


}
