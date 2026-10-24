using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace target.Models.Models
{
    public class GameVendorConvertRateTempModel
    {  
        public int GameVendorID { get; set; }

        public bool IsDefine { get; set; }

        public decimal ConvertRate { get; set; }
    }
}
