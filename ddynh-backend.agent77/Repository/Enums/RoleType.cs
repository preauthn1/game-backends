using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ViewModel
{
    public enum RoleType : int
    {
        [Description("管理员")]
        Admin = 0,
        [Description("财务")]
        CFO = 1,
        [Description("总代理")]
        Biz = 2,
        [Description("代理")]
        Agent = 3

    }

}
