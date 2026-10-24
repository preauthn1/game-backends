using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class GeneralTagEntity
    {
        [Key]
        public int TagID { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }

        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }


    }
}
