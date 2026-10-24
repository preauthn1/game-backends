using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class EventPrizeBundleEntity
    {
        [Key]
        public int EventPrizeBundleID { get; set; }

        public int EventID { get; set; }

        public string Memo { get; set; }

        public int? LimitQuantity { get; set; }

        public bool IsDisabled { get; set; }


    }
}
