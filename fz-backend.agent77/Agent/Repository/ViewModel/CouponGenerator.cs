using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ViewModel
{
    public class CouponGenerator
    {
        public string PlatformID { get; set; }
        public int CouponAmount { get; set; }
        public int CouponCounts { get; set; }
    }
}
