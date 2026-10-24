using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Money
{
    
    public enum PromotionEventPositionTypeEnum : byte
    {
        [EnumDescription("首頁弹窗")]
        HomePage = 1,

        [EnumDescription("公告内文页")]
        MailPage = 2,
        
    }
}
