using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.RawEntity
{

    public class UserTagMappingRawEntity
    {
        [Key]
        public int Id { get; set; }
        public int UserID { get; set; }
        public int TagID { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }


    }
}
