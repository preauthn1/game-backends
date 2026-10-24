using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ViewModel
{

    /// <summary>
    /// 硬件类型
    /// </summary>
    public enum HDType : byte
    {
        [Description("PC")]
        PC = 1,
        [Description("安卓")]
        Android = 2,
        [Description("苹果")]
        IOS = 3
    }

}
