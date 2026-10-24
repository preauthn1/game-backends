using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ViewModel
{
    public enum ModeType : int
    {
        [Description("全民代理")]
        Mode1 = 1,
        [Description("流水代理")]
        Mode2 = 2,
        [Description("裂变代理")]
        Mode3 = 3,
        [Description("联运代理")]
        Mode4 = 4,
        [Description("无限极代理")]
        Mode5 = 5,
        [Description("礼码代理")]
        Mode6 = 6
    }


}
