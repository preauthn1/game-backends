using FW.Core.Helper.Enum;

namespace game.Entity.Pay
{

    public enum PayType : int
    {
        [EnumDescription("支付宝")]
        Alipay = 1,
        [EnumDescription("微信")]
        Weixin = 2,
        [EnumDescription("銀聯")]
        Bank = 3,
        [EnumDescription("QQ")]
        QQ = 4,
        [EnumDescription("京東支付")]
        JD = 5,
        [EnumDescription("久聊支付")]
        Naychat = 6,
        [EnumDescription("云闪付")]
        CloudPay = 7,
        [EnumDescription("大额专属通道")]
        LargeExclusive = 8,
        [EnumDescription("支付宝扫码")]
        AliQRpay = 11,
        [EnumDescription("支付宝固额")]
        AliFixedpay = 12,
        [EnumDescription("支付宝红包")]
        AlipayEnvelope = 13,
        [EnumDescription("支付宝转帐")]
        AlipayTransfer = 14,
        [EnumDescription("支付宝二维码")]
        AlipayQR = 15,
        [EnumDescription("支付宝扫码+")]
        AliQRpayPlus = 16,
        [EnumDescription("支付宝收银")]
        AliCash = 17,
        [EnumDescription("冬瓜支付")]
        AlipayXiami = 18,
        [EnumDescription("冬瓜微信")]
        WeixinXiami = 19,
        [EnumDescription("铁锤支付")]
        Tiechuipay = 20,
        [EnumDescription("微信扫码")]
        WeixinQR = 21,
        [EnumDescription("微信固额")]
        WeixinFixed = 22,
        [EnumDescription("微信红包")]
        WeChatEnvelope = 23,
        [EnumDescription("微信转帐")]
        WeChatTransfer = 24,
        [EnumDescription("银行卡充值")]
        BankCardRecharge = 31,
        [EnumDescription("微转银二维码")]
        WeixinToBank = 81,
        [EnumDescription("支转银二维码")]
        AlipayToBank = 82,
        [EnumDescription("云闪付转银二维码")]
        CloudPayToBank = 83,
        [EnumDescription("IOS")]
        [Disabled(true)]
        IOS = 98,
        [EnumDescription("代理")]
        agent = 99
    }

    public enum PayGatewayType : int
    {
        [EnumDescription("艾米森")]
        Aimisen = 1,
        [EnumDescription("W支付")]
        Wfu = 2,
        [EnumDescription("70Card")]
        Card70 = 3,
        [EnumDescription("同樂")]
        Tungle = 4,
        [EnumDescription("聚合")]
        Jiuhe = 5,
        [EnumDescription("统一")]
        Tungyi = 6,
        [EnumDescription("闪付")]
        Shanfu = 7,
        [EnumDescription("MCPay")]
        MCPay = 8,
        [EnumDescription("XPay")]
        XPay = 9,
        [EnumDescription("云支付")]
        XunPay = 10,
        [EnumDescription("超凡支付")]
        CfanPay = 11,
        [EnumDescription("快联")]
        KuaiLian = 12,
        [EnumDescription("UNPay")]
        UNPay = 13,
        [EnumDescription("店员通")]
        DianYuanTong = 14,
        [EnumDescription("宝盒支付")]
        Baohepay = 16,
        [EnumDescription("天边支付")]
        Tianbianpay = 17,
        [EnumDescription("铁锤")]
        Tiechuipay = 18,
        [EnumDescription("冬瓜")]
        Xiamipay = 19,
        [EnumDescription("财付")]
        XF188pay = 20,
        [EnumDescription("亿点")]
        GP_YDpay = 21,
        [EnumDescription("蜻蜓")]
        Qingting = 22,
        [EnumDescription("蜻蜓(个)")]
        QingtingQe = 23,
        [EnumDescription("客付通")]
        KFTpay = 24,       
        [EnumDescription("汇通")]
        Huitong = 25,     
        [EnumDescription("汇源通")]
        HYTpay = 26,
        [EnumDescription("环信2站")]
        HX2Zpay = 27,
        [EnumDescription("金牌MCPay")]
        GPMCpay = 28,
        [EnumDescription("星入宝")]
        xingrubao = 29,
        [EnumDescription("永安")]
        YAPay = 30,
        [EnumDescription("蓉银")]
        Rongyin = 31,
        [EnumDescription("融支付")]
        RongPay = 32,
        [EnumDescription("U鑫")]
        Uxin = 33,
        [EnumDescription("维护宝")]
        WHBPay = 35,
        [EnumDescription("8pay")]
        EightPay = 37,
        [EnumDescription("HiPay")]
        HiPay = 38,
        [EnumDescription("聚眾")]
        JuZhongPay = 39,
        [EnumDescription("安迅通")]
        Anxanpay = 40,
        [EnumDescription("万伽")]
        WangyuaPay = 41,
        [EnumDescription("派派")]
        PPZFPay = 42,
        [EnumDescription("365支付")]
        Pay365 = 43,
        [EnumDescription("金城支付")]
        JinCheng = 44,
        [EnumDescription("凯旋")]
        KaiXuanPay = 45,
        [EnumDescription("盈支付")]
        YINPay = 46,
        [EnumDescription("闪入宝")]
        ShanRuBao = 47,
        [EnumDescription("快付支付")]
        KuaiFu = 48,
        [EnumDescription("牛牛支付")]
        NiuNiuPay = 49,
        [EnumDescription("盒付支付")]
        HeFuPay = 50,
        [EnumDescription("慧云支付")]
        Huiyun = 51,
        [EnumDescription("多聚发支付")]
        Djfzf = 52,
        [EnumDescription("AE支付")]
        AEPay = 99,


    }

    public enum PaySourceType : int
    {
        [EnumDescription("支付宝")]
        Alipay = 11001,
        [EnumDescription("微信")]
        Weixin = 11002,
        [EnumDescription("銀聯")]
        BankCard = 11004,
        [EnumDescription("QQ")]
        QQ = 11003,
        [EnumDescription("IOS")]
        IOS = 11007,
        [EnumDescription("代理充值")]
        Agent = 11008,
        [EnumDescription("京东充值")]
        JD = 11010,
        [EnumDescription("久聊充值")]
        Naychat = 11011,
        [EnumDescription("云闪付")]
        CloudPay = 11012
    }
}
