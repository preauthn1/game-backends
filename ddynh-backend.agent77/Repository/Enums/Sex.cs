using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ViewModel
{
    /// <summary>
    /// 性别
    /// </summary>
    public enum Sex : byte
    {
        [Description("男")]
        Man = 0,
        [Description("女")]
        Woman = 1
    }

}
