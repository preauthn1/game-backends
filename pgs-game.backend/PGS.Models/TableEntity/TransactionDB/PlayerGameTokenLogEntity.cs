using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class PlayerGameTokenLogEntity
    {
        [Key]
        public long Id { get; set; }
        public int MerchantID { get; set; }
        public int PlayerID { get; set; }
        public string TokenHash { get; set; }
        public string IPAddress { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ExpiredTime { get; set; }

    }
}
