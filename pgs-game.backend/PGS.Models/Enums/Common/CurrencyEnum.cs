using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.CultureText;


namespace target.Models.Enums
{
    /// <summary>
    /// 幣別
    /// </summary>
    [Description("資料庫是存字串_編號本身沒意義")]
    public enum CurrencyEnum
    {

        /// <summary>
        /// 泰銖
        /// </summary>
        THB,

        /// <summary>
        /// 馬來幣
        /// </summary>
        MYR,

        /// <summary>
        /// 印尼盾
        /// </summary>
        IDR,


        /// <summary>
        /// 越南盾
        /// </summary>
        VND,


        /// <summary>
        /// 新加坡幣
        /// </summary>
        SGD,


        /// <summary>
        /// 汶萊幣
        /// </summary>
        BND,


        /// <summary>
        /// 人民幣
        /// </summary>
        CNY,

        /// <summary>
        /// 台幣
        /// </summary>
        TWD,

        /// <summary>
        /// 美元
        /// </summary>
        USD,


        /// <summary>
        /// 數位貨幣 USDT
        /// </summary>
        USDT,

       

        /// <summary>
        /// 越南幣 (1000:1)
        /// </summary>
        KVND,


        /// <summary>
        /// 印尼幣 (1000:1)
        /// </summary>
        KIDR,


        /// <summary>
        /// 澳幣
        /// </summary>
        AUD,

        /// <summary>
        /// 尼伯爾
        /// </summary>
        NPR,

        /// <summary>
        /// 孟加拉
        /// </summary>
        BDT,

        /// <summary>
        /// 印度盧比
        /// </summary>
        INR,

        /// <summary>
        /// 肯亞幣
        /// </summary>
        KES,

        /// <summary>
        /// BETA幣
        /// </summary>
        BETA,

        /// <summary>
        /// 加拿大
        /// </summary>
        CAD,

        /// <summary>
        /// 捷克幣
        /// </summary>
        CZK,

        /// <summary>
        /// 丹麥
        /// </summary>
        DKK,

        /// <summary>
        /// 歐元
        /// </summary>
        EUR,

        /// <summary>
        /// 英鎊
        /// </summary>
        GBP,

        /// <summary>
        /// 港元
        /// </summary>
        HKD,

        /// <summary>
        /// 日幣
        /// </summary>
        JPY,

        /// <summary>
        /// 韓元
        /// </summary>
        KRW,

        /// <summary>
        /// 墨西哥
        /// </summary>
        MXN,

        /// <summary>
        /// 緬甸元 (對方還有支援另一種 1:1000 MYK)
        /// </summary>
        MMK,


        /// <summary>
        /// 緬甸 1:1000
        /// </summary>
        KMMK,


        /// <summary>
        /// 挪威
        /// </summary>
        NOK,


        /// <summary>
        /// 巴西
        /// </summary>
        BRL,


        /// <summary>
        /// 紐西蘭
        /// </summary>
        NZD,

        /// <summary>
        /// 菲律賓
        /// </summary>
        PHP,

        /// <summary>
        /// 波蘭
        /// </summary>
        PLN,

        /// <summary>
        /// 瑞典克朗
        /// </summary>
        SEK,


        /// <summary>
        /// 俄羅斯盧布
        /// </summary>
        RUB,


        /// <summary>
        /// 寮國貨幣
        /// </summary>
        LAK,


        /// <summary>
        /// 烏干達先令
        /// </summary>
        UGX,

        /// <summary>
        /// 摩洛哥迪拉姆
        /// </summary>
        MAD,


        /// <summary>
        /// 南非鍰
        /// </summary>
        ZAR,

        /// <summary>
        /// 奈及利亞奈拉
        /// </summary>
        NGN,


        /// <summary>
        /// 巴基斯坦
        /// </summary>f
        PKR,



        /// <summary>
        /// 斯里蘭卡
        /// </summary>
        LKR,


        /// <summary>
        /// 突尼西亞-第納爾
        /// </summary>
        TND,

        /// <summary>
        /// 阿拉伯-阿聯迪拉姆
        /// </summary>
        AED,

        /// <summary>
        /// 阿爾及利亞-第納爾
        /// </summary>
        DZD,

        /// <summary>
        /// 亞美尼亞-德拉姆
        /// </summary>
        AMD,

        /// <summary>
        /// 土耳其-里拉
        /// </summary>
        TRY,

        /// <summary>
        /// 哥倫比亞-披索 (1:1000)
        /// </summary>
        KCOP,

        /// <summary>
        /// 哥倫比亞-披索 (1:1)
        /// </summary>
        COP,


        /// <summary>
        /// 瑞士法郎
        /// </summary>
        CHF,

        /// <summary>
        /// 智利 披索
        /// </summary>
        CLP,

        /// <summary>
        /// 柬埔寨里爾
        /// </summary>
        KHR,

        /// <summary>
        /// 祕魯太陽幣
        /// </summary>
        PEN,


        /// <summary>
        /// 巴布亞紐幾內亞-吉納
        /// </summary>
        PGK,


        /// <summary>
        /// 伊拉克第納爾（Iraqi Dinar）
        /// </summary>
        IQD,


        /// <summary>
        /// 敘利亞鎊 （Syrian Pound）
        /// </summary>
        SYP,

        /// <summary>
        /// 阿根廷披索
        /// </summary>
        ARS,

        /// <summary>
        /// 玻利維亞諾
        /// </summary>
        BOB,

        /// <summary>
        /// 哥斯大黎加科朗
        /// </summary>
        CRC,

        /// <summary>
        /// 多明尼加披索
        /// </summary>
        DOP,

        /// <summary>
        /// 巴拉圭-瓜拉尼
        /// </summary>
        PYG,

        /// <summary>
        /// 烏拉圭披索
        /// </summary>
        UYU,

        /// <summary>
        /// 委內瑞拉-玻利瓦
        /// </summary>
        VES,


        /// <summary>
        /// 巴拿馬 巴爾波
        /// </summary>
        PAB,


        IRR,
        MNT,
        UAH,
        KZT,
        UZS,
        AZN,
        GEL,
        KGS,
        TMT,
        BYN,
        MDL,
        HUF,
        RSD,
        TZS,
        ZMW,
        XAF,
        GHS,
        CDF,
        XOF,
        SSP,
        HTG,
        NAD,
        ETB,
        EGP,
        MZN,
        RWF,
        LYD,
        BWP,
        SAR,
        MVR,

        /// <summary>
        /// MYR 100:1
        /// </summary>
        MYR100,


        /// <summary>
        /// 點數制
        /// </summary>
        MSC_POINT,
    }
}
