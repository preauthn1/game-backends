using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class Coupon_DailyItem
    {
        public DateTime LogDate { get; set; }
        public Int64 CouponCount { get; set; }
        public Int64 Amount { get; set; }
        public Int64 DailyUsedCouponAmount { get; set; }
    }
}
