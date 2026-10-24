using game.Entity.Event;
using game.Entity.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.AdminSystem.Models.ViewModels
{

    public class UnionpayBankCodeUpdateViewModel : UnionpayBankCodeEntity
    {

        public bool IsUpdatePage { get; set; }

        public int Status { get; set; }

    }



    /// <summary>
    /// 顯示資料用
    /// </summary>
    public class UnionpayBankCodeDisplayViewModel : UnionpayBankCodeEntity
    {
        public string IsShowText { get; set; }
        public string CreateTimeText { get; set; }
        public string UpdateTimeText { get; set; }
    }


    public class QueryUnionpayBankCodeJQueryRequestModel : JQueryDataTablePagingModel
    {
        public string PlatformIDsText { get; set; }

    }



}