using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using game.Entity.Money;
using System.ComponentModel.DataAnnotations;

namespace game.Entity.Pay
{
    /// <summary>
    /// 支付渠道状态
    /// </summary>
    public enum CDPayTypeStatus : byte
    {
        [EnumDescription("正常")]
        Active = 1,
        [EnumDescription("关闭")]
        UnActive = 0
    }
}
