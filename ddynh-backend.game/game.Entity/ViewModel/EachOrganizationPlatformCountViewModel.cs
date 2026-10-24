using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{
    public class EachOrganizationPlatformCountSummaryViewModel
    {        
        public int OrgID { get; set; }

        public int TotalUserCount { get; set; }

        public int TotalPlatformCount { get; set; }
    }

    public class EachPlatformOrganizationCountViewModel
    {
        public int PlatformID { get; set; }

        public int TotalOrgCount { get; set; }

    }
}
