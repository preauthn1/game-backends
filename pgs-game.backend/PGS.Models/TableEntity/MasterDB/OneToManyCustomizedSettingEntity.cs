using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class OneToManyCustomizedSettingEntity
    {
        [Key]
        public int OneToManyCustomizedSettingID { get; set; }
        public int Type { get; set; }
        public long? OwnerID { get; set; }
        public string OwnerIDMemo { get; set; }
        public string DisplayName { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }

        public int Status { get; set; }


    }
}
