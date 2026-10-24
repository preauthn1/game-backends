using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{
    public class UpdateServerRelationRequestModel
    {        
        public int ServerID { get; set; }

        public int PlatformID { get; set; }

        public int TargetStatus { get; set; }

    }
}
