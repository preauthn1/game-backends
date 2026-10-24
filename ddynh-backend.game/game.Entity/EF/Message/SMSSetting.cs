using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game.Entity.Warning;

namespace game.Entity.Message
{
    public class SMSSetting
    {
        public int SMSSettingId { get; set; }
        public int PlatformId { get; set; }
        public string PlatformName { get; set; }
        public int SMSId { get; set; }
        public string SMSName { get; set; }
        public string SMSInfo { get; set; }
        public int Sort { get; set; }
        public WarningTypeStatus Status { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
