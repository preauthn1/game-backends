using FW.Core.Helper.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Area
{
    /// <summary>
    /// 熱門程度
    /// </summary>
    public enum Popular
    {
        [EnumDescription("无")]
        None = 0,
        [EnumDescription("火")]
        Fire = 1,
        [EnumDescription("热门")]
        Hot = 2,
        [EnumDescription("试玩")]
        Try = 3,
        [EnumDescription("刺激")]
        Spur = 4,
        [EnumDescription("推荐")]
        Recommend = 5
    }

}
