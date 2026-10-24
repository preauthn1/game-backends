using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Customer
{

    public enum UpdateCashAuditTypeEnum
    {
        [EnumDescription("人工补单(不计充值)")]
        ManualCashIn_ExcludeCharge = 1,

        [EnumDescription("人工入款(计充值金额)")]
        ManualCashIn_IncludeCharge = 2,

        [EnumDescription("优惠活动")]
        EventBonus = 3,

    }

}
