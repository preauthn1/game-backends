using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class Split_GetUserSumData
    {
        public long lngRewardAmount { get; set; }
        public long lngSplitAmount { get; set; }
        public int intSuccessCount { get; set; }
        public int intSuccessToday { get; set; }
        public int intSuccessLastday { get; set; }
    }
}
