using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Enums.ThirdPartyPay
{
    public enum SFTPayChannelEnum : byte
    {
        ZHIFUBAO = 2,
        WEIXIN = 1,
    }

    public enum SFTPayTypeEnum : byte
    {
        QR = 2,
        H5 = 1
    }

    public enum SFTPayWayEnum : byte
    {
        QRCode = 1,
        Transfer = 2,
        BankCard = 3,
        RedPackets = 4
    }
}
