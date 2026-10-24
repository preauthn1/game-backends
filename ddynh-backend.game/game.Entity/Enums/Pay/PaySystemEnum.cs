using FW.Core.Helper.Enum;

namespace game.Entity.Payment
{
    public enum PaySystemEnum : int
    {
        [EnumDescription("AE支付中心")]
        AEPayCenter = 1,
        [EnumDescription("第三方")]
        ThirdPartyPay = 2
    }
}
