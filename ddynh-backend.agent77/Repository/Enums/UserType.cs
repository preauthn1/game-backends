using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ViewModel
{

    /// <summary>
    /// 用户类型
    /// </summary>
    public enum UserType : byte
    {
        [Description("正式账号")]
        Reg = 1,
        [Description("机器人")]
        Rebot = 2,
        [Description("游客号")]
        Fast = 3,
    }

}
