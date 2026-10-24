using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Enums.ThirdPartyPay
{
    /// <summary>
    /// 通匯支付類型
    /// </summary>
    public enum THPayTypeEnum : byte
    {
        QUICKPASS_QR = 9,
        ZHIFUBAO_QR = 11
    }

    /// <summary>
    /// 通匯表單回傳是否成功:0失败 1成功
    /// </summary>
    public enum THPayPayReturnSuccessEnum:byte
    {
        Success = 1,
        Fail = 0
    }

    /// <summary>
    /// 通匯異步通知狀態Enum:目前只有狀態2和狀態3
    /// </summary>
    public enum THPayResponseStatusEnum : byte
    {
        CreatePayment = 0,
        CreatePaymentError = 1,
        UnPaid = 2,
        PaySuccess = 3,
        PayError = 4,
        Delete = 5,
        Paid = 6
    }
}
