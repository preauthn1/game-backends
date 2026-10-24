using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{
    public class UpdatePlatformSettingPriorityRequestModel
    {        
        public int PayTypeID { get; set; }

        public int PlatformID { get; set; }

        public int Priority { get; set; }


    }

}
