using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class Coupon_CouponDetail
    {
        public String CouponCode { get; set; }
        public int? UserID { get; set; }
        public long Amount { get; set; }
        public DateTime? UseDate { get; set; }
    }
}
