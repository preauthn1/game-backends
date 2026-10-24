using game.Entity.Event;
using game.Entity.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.AdminSystem.Models.ViewModels
{

   
    /// <summary>
    /// 顯示資料用
    /// </summary>
    public class TransferRecordDisplayViewModel : TransferRecordRawEntity
    {

        public string AmountText { get; set; }


        public string StatusText { get; set; }


        public string TransferTypeText { get; set; }


        public string GameMerchantText { get; set; }


        public string CreateTimeText { get; set; }


        public string PlatformName { get; set; }

    }


    public class QueryTransferRecordJQueryRequestModel : JQueryDataTablePagingModel
    {

        public int? PlatformID { get; set; }

        public int? GameMerchantID { get; set; }

        public int? UserID { get; set; }

        public string OrderNo { get; set; }

        public DateTime? BeginDate { get; set; }

        public DateTime? EndDate { get; set; }



    }



}