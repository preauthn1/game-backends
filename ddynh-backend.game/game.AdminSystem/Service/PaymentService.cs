using game.Entity.Payment;
using System;
using System.Collections.Generic;
using System.Linq;


namespace game.AdminSystem
{
    public static class PaymentService
    {
        public static List<PayType> GetPayTypeAll()
        {
            return SiteHelper.GetAPIResult<List<PayType>>(string.Format("/Payment/CDGetPayType?operatorID={0}", SiteService.GetAdminUserID()));
        }
        public static List<PayGateway> GetPayGatewayAll()
        {
            return SiteHelper.GetAPIResult<List<PayGateway>>(string.Format("/Payment/CDGetPayGateway?operatorID={0}", SiteService.GetAdminUserID()));
        }

    }
}