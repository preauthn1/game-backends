using game.Entity.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using game.Infrastructure.Extensions;

namespace game.WebSite.Services
{
    /// <summary>
    /// 取得支付類型(是否是定額)
    /// </summary>
    public class PayTypeService
    {
        public PayType GetPayTypeByOrder(CDPayOrder order)
        {
            if (order == null)
            {
                return null;
            }

            var payType = SiteHelper.GetAPIResult<PayType>($"/Payment/GetPayTypeByID?payTypeID={order.PayTypeID}");

            if (payType == null)
            {
                return null;
            }

            return payType;
        }
    }
}