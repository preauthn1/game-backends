using game.Entity.Event;
using game.Entity.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using game.Entity.Payment;

namespace game.AdminSystem.Models.ViewModels
{

    public class PayGatewayAdminUpdateViewModel : PayGatewayEntity
    {

        public bool IsUpdatePage { get; set; }

        public bool IsSupportt_Alipay { get; set; }

        public bool IsSupportt_Unionpay { get; set; }



        /// <summary>
        /// 后端接数值用
        /// </summary>
        public string IsSupportt_Alipay_CheckText { get; set; }

        public string IsSupportt_Unionpay_CheckText { get; set; }





    }



    /// <summary>
    /// 顯示資料用
    /// </summary>
    public class PayGatewayDisplayViewModel : PayGatewayEntity
    {
        public string PlatformName { get; set; }

    }


    public class QueryPayGatewayJQueryRequestModel : JQueryDataTablePagingModel
    {
        public int? PlatformID { get; set; }


        public string PlatformIDsText { get; set; }

    }



}