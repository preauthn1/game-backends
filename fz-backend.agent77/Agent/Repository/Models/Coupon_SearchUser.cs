using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class Coupon_SearchUser
    {
        public Int32 UserID { get; set; }
        public Int32 CouponCount { get; set; }
        public Int64 CouponAmount { get; set; }
        public Int64 UsedCouponAmount { get; set; }
        public DateTime LastLogon { get; set; }
    }
}
