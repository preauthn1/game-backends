using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ViewModel
{

    public enum AgentApplyStatusEnum : byte
    {
        [Description("审核中")]
        Waiting = 0,

        [Description("已通过")]
        Pass = 1,

        /// <summary>
        /// 不確定是否是這個編號
        /// </summary>
        [Description("已拒绝")]
        Fail = 2
    }

}
