using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class Split_GetSystemRate
    {
        public int PlatformID { get; set; }
        public int Level { get; set; }
        public int MinRecruit1 { get; set; }
        public int MaxRecruit1 { get; set; }
        public int MinRecruit2 { get; set; }
        public int MaxRecruit2 { get; set; }
        public int MinRecruit3 { get; set; }
        public int MaxRecruit3 { get; set; }
        public int Stage1 { get; set; }
        public int Stage2 { get; set; }
        public int Stage3 { get; set; }
    }
}
