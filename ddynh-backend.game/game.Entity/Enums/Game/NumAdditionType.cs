using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{


    public enum NumAdditionType : byte
    {
        [EnumDescription("大厅")]
        Hall,
        [EnumDescription("房间")]
        Room
    }


}
