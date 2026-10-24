using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.RawEntity
{

    public class TagsRawEntity
    {
        [Key]
        public int TagID { get; set; }
        public int? UniqueKey { get; set; }
        public int Type { get; set; }
        public string TagName { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UpdateBy { get; set; }


    }
}
