using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class Coupon_DailyReportModel
    {
        public Int32 CouponPullCount { get; set; }

        public Int64 CouponPullAmount { get; set; }

        public Int64 CouponUsedAmount { get; set; }

        public List<Coupon_DailyItem> DailyItemList { get; set; }
    }
}
