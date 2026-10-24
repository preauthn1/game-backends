using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.RawEntity
{

    public class AttrRawEntity
    {
        [Key]
        public int Id { get; set; }
        public int UserID { get; set; }
        public byte TypeID { get; set; }
        public long? Param { get; set; }
        public string SParam { get; set; }
        public byte Status { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime LastPostTime { get; set; }


    }
}
