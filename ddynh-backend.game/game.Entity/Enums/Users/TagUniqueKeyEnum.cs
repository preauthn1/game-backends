using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Users
{
    /// <summary>
    /// 對應Tags這張表的uniqueKey欄位
    /// </summary>
    public enum TagUniqueKeyEnum
    {
     
        [EnumDescription("内部测试帐号")]
        InternalTestAccount = 1001

    }
}
