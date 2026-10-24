using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class EventMerchantMappingEntity
    {
        [Key]
        public int MappingID { get; set; }
        public int EventID { get; set; }
        public int MerchantID { get; set; }





    }
}
