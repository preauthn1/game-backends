using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class Unlimit_GetUserSumData
    {
        public long PoolAmount { get; set; }
        public int Totaluser { get; set; }
        public int SubDirectUser { get; set; }
        public long TotalPerformance { get; set; }
        public long WeekTotalPerformance { get; set; }
    }
}
