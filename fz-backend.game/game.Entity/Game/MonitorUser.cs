using FW.Core.Helper.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
    public class MonitorUser
    {
        public int ServerID { get; set; }
        public int GameID { get; set; }
        public string UserIDList { get; set; }
        public string ShowName { get; set; }
        public MonitorUserActive Active { get; set; }
    }

    public enum MonitorUserActive
    {
        [EnumDescription("歷史")]
        History = 0,
        [EnumDescription("当前")]
        Current = 1
    }
}
