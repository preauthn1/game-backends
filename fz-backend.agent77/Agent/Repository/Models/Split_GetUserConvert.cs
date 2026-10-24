using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class Split_GetUserConvert
    {
        public int RowNum { get; set; }
        public int UserID { get; set; }
        public string LogDate { get; set; }
        public long ConvertAmount { get; set; }
    }
}
