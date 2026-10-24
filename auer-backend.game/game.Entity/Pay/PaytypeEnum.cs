using FW.Core.Helper.Enum;

namespace game.Entity.Pay
{

    public enum PayType : int
    {
        //[EnumDescription("支付宝")]
        //Alipay = 1,
        //[EnumDescription("微信")]
        //Weixin = 2,
        [EnumDescription("銀聯")]
        Bank = 3,
        //[EnumDescription("QQ")]
        //QQ = 4,
        //[EnumDescription("京東支付")]
        //JD = 5,
        //[EnumDescription("久聊支付")]
        //Naychat = 6,
        //[EnumDescription("云闪付")]
        //CloudPay = 7,
        //[EnumDescription("大额专属通道")]
        //LargeExclusive = 8,
        //[EnumDescription("支付宝扫码")]
        //AliQRpay = 11,
        //[EnumDescription("支付宝固额")]
        //AliFixedpay = 12,
        //[EnumDescription("支付宝红包")]
        //AlipayEnvelope = 13,
        //[EnumDescription("支付宝转帐")]
        //AlipayTransfer = 14,
        //[EnumDescription("支付宝二维码")]
        //AlipayQR = 15,
        //[EnumDescription("支付宝扫码+")]
        //AliQRpayPlus = 16,
        //[EnumDescription("支付宝收银")]
        //AliCash = 17,
        //[EnumDescription("冬瓜支付")]
        //AlipayXiami = 18,
        //[EnumDescription("冬瓜微信")]
        //WeixinXiami = 19,
        //[EnumDescription("铁锤支付")]
        //Tiechuipay = 20,
        //[EnumDescription("微信扫码")]
        //WeixinQR = 21,
        //[EnumDescription("微信固额")]
        //WeixinFixed = 22,
        //[EnumDescription("微信红包")]
        //WeChatEnvelope = 23,
        //[EnumDescription("微信转帐")]
        //WeChatTransfer = 24,
        //[EnumDescription("银行卡充值")]
        //BankCardRecharge = 31,
        [EnumDescription("GASH點數卡")]
        COPGAM05 = 41,
        [EnumDescription("GASH會員轉點")]
        COPGAM09 = 42,
        [EnumDescription("GASH第一銀行微信支付")]
        BNK00701 = 43,
        [EnumDescription("GASH中国银联卡")]
        BNK82204 = 44,
        //[EnumDescription("微转银二维码")]
        //WeixinToBank = 81,
        //[EnumDescription("支转银二维码")]
        //AlipayToBank = 82,
        //[EnumDescription("云闪付转银二维码")]
        //CloudPayToBank = 83,
        //[EnumDescription("IOS")]
        //[Disabled(true)]
        //IOS = 98,
        //[EnumDescription("代理")]
        //agent = 99
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
        [EnumDescription("永旺")]
        Yongwang = 34,
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
        [EnumDescription("久付支付")]
        EightZpay = 53,
        [EnumDescription("新金鑫支付")]
        JinxinPay = 54,
        [EnumDescription("惠诚支付")]
        HuiChengPay = 55,
        [EnumDescription("讯科支付")]
        XKPay = 56,
        [EnumDescription("佰富")]
        Baifu = 57,
        [EnumDescription("轰达统一")]
        TungyiHonda = 58,
        [EnumDescription("盛世")]
        ShengShi = 59,
        [EnumDescription("畅支付")]
        Zumire = 60,
        [EnumDescription("翔优")]
        Xiangyou = 61,
        [EnumDescription("火星支付")]
        BitexPay = 62,
        [EnumDescription("东方闪付")]
        QuickPay = 63,
        [EnumDescription("融海闪付")]
        RongHai = 64,
        [EnumDescription("金发闪付")]
        JFPay = 65,
        [EnumDescription("中邦")]
        Zhongbang = 66,
        [EnumDescription("葡萄")]
        Putao = 67,
        [EnumDescription("新全付支付")]
        XinQuanFu = 68,
        [EnumDescription("68支付")]
        LiuBa = 69,
        [EnumDescription("诚易玖支付")]
        ChengYiJiu = 70,
        [EnumDescription("完美支付")]
        PerfectPay = 71,
        [EnumDescription("天猫支付")]
        TMAllPay = 72,
        [EnumDescription("巅峰冬瓜支付")]
        DFXiamipay = 73,
        [EnumDescription("星坤支付RH")]
        XingKunRH = 74,
        [EnumDescription("高盛支付")]
        GaoSheng = 75,
        [EnumDescription("星坤支付YH")]
        XingKunYH = 76,
        [EnumDescription("星坤支付XK")]
        XingKunXK = 77,
        [EnumDescription("旗胜支付")]
        QiShengPay = 78,
        [EnumDescription("招财宝")]
        ZhaoCaiBao = 79,
        [EnumDescription("小美支付")]
        XiaoMei = 80,
        [EnumDescription("澄支付")]
        SumiPay = 81,
        [EnumDescription("天翔支付")]
        FytxPay = 82,
        [EnumDescription("聚宝支付")]
        JBPay = 83,
        [EnumDescription("星支付")]
        StarPay = 84,
        [EnumDescription("易码支付")]
        YiMaPay = 85,
        [EnumDescription("巅峰统一")]
        TungyiDF = 86,
        [EnumDescription("聚光支付")]
        JuGuangPay = 87,
        [EnumDescription("众诚支付")]
        ZhongChengPay = 88,
        [EnumDescription("OK支付")]
         OKPay = 89,
        [EnumDescription("BDPAY")]
        BDPay = 90,
        [EnumDescription("收米支付")]
        ShouMiPay = 91,
        [EnumDescription("东方支付")]
        DongFangPay = 92,
        [EnumDescription("贝富支付")]
        BeifuPay = 93,
        [EnumDescription("星耀支付")]
        Xingyao = 94,
        [EnumDescription("春秋支付")]
        Chunqiu = 95,
        [EnumDescription("多宝支付")]
        DuoBaoPay = 96,
        [EnumDescription("实时支付")]
        LivePay = 97,
        [EnumDescription("唯创支付")]
        WeiChuang = 98,
        [EnumDescription("金钻支付")]
        JZPay = 99,
        [EnumDescription("旺旺宝支付")]
        Wangwangbao = 101,
        [EnumDescription("易收支付")]
        Easypay = 102,
        [EnumDescription("易联支付")]
        Yilian = 103,
        [EnumDescription("点点支付")]
        Diandian = 104,
        [EnumDescription("OPay支付")]
        OPay = 105,
        [EnumDescription("怪兽支付")]
        MonsterPay = 106,
        [EnumDescription("Gash支付")]
        GashPay = 2001,
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
        CloudPay = 11012,
        [EnumDescription("Gash充值")]
        GashPay = 11013,
    }
}
