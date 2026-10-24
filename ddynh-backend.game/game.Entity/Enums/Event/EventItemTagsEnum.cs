using FW.Core.Helper.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Event
{  

    public enum EventItemTagEnum : byte
    {
        [EnumDescription("无")]
        None = 0,
        
        [EnumDescription("热门")]
        Hot = 1,

        [EnumDescription("推荐")]
        Recommend = 2,

        [EnumDescription("最新")]
        Newest = 3,

    }


}
