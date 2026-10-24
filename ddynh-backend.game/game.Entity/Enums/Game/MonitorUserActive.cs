using FW.Core.Helper.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
  
    public enum MonitorUserActive
    {
        [EnumDescription("歷史")]
        History = 0,
        [EnumDescription("当前")]
        Current = 1
    }
}
