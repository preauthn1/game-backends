using game.Entity.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using game.Infrastructure.Extensions;

namespace game.WebSite.Services
{
    /// <summary>
    /// 取得MerchantData
    /// </summary>
    public static class MerchantDataService
    {
        /// <summary>
        /// 取得MerchantData
        /// </summary>
        public static CDMerchantUUID GetMerchantData(CDPayOrder order)
        {
            if (order == null)
            {
                return null;
            }

            var platformID = SiteHelper.GetAPIResult<int>($"/Payment/GetPlatformId?UserID={order.UserID.ToString()}");
            var payList = SiteHelper.GetAPIResult<List<CDMerchantUUID>>($"/Payment/GetMerchantUuid?payTypeID={order.PayTypeID}&platformID={platformID}");

            if (payList.IsEmptyOrNull())
            {
                return null;
            }
            int key = (payList.Count() > 1) ? new Random().Next(0, payList.Count() - 1) : 0;
            return payList[key];
        }
    }
}