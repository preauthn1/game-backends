using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class UserTokenRecordEntity
    {
        [Key]
        public long Id { get; set; }
        public string TokenMD5 { get; set; }
        public int UserID { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ExpiredTime { get; set; }

    }
}
