using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.RawEntity
{

    public class UserMemoRawEntity
    {
        [Key]
        public int Id { get; set; }
        public int Type { get; set; }
        public int UserID { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }


    }
}
