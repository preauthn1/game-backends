using FW.Core.Helper;
using game.Entity.AdminSys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{
    public class UpdatePromotionEventAreaMappingRequestModel 
    {        

        public int EventID { get; set; }

        public List<int> AreaIDs { get; set; }

    }
}
