using System;
using FW.Core.Helper.Enum;

namespace game.Entity.Game
{

    /// <summary>
    /// 遊戲名稱有用table管理，但是管理上比較像列舉
    /// </summary>
    public enum GameListEnum
    {
        [EnumDescription("斗地主")]
        DDZ_鬥地主 = 1,

        [EnumDescription("抢庄牛牛")]
        QZNN_搶庄牛牛 = 2,

        [EnumDescription("诈金花")]
        ZJH_炸金花 = 3,

        [EnumDescription("德州扑克")]
        dzpk_德州撲克 = 4,

        [EnumDescription("梭哈")]
        wzp_梭哈 = 5,

        [EnumDescription("抢庄21点")]
        QZ_搶庄21 = 6,

        [EnumDescription("通比牛牛")]
        tbnn_通比牛牛 = 7,

        [EnumDescription("三公")]
        sango_三公 = 8,

        [EnumDescription("股市大亨")]
        SGJ_股市大亨 = 9,

        [EnumDescription("十三水")]
        sss_十三水 = 10,

        [EnumDescription("龙虎斗")]
        LHD_龙虎斗 = 11,

        [EnumDescription("推筒子")]
        ttz_推筒子 = 12,

        [EnumDescription("百家乐")]
        BJL_百家乐 = 13,

        [EnumDescription("捕鱼")]
        BY_捕鱼 = 14,

        [EnumDescription("四川麻将")]
        scmj_四川麻将 = 15,

        [EnumDescription("雙人牛牛")]
        srnn_雙人牛牛 = 16,

        [EnumDescription("雙人麻將")]
        ermj_雙人麻將 = 17,

        [EnumDescription("二八杠")]
        FKEB_二八杠 = 18,

        [EnumDescription("百人牛牛")]
        BRNN_百人牛牛 = 19,

        [EnumDescription("红黑大战")]
        HHDZ_红黑大战 = 20,

        [EnumDescription("红黑牛牛")]
        HHNN_红黑牛牛 = 21,

        [EnumDescription("21点")]
        Point_21点 = 22,

        [EnumDescription("性感百家乐")]
        SGBJL_性感百家乐 = 23,

        [EnumDescription("多福多财")]
        DFDC_多福多财 = 25,

        [EnumDescription("糖果屋")]
        TGW_糖果屋 = 26,

        [EnumDescription("阿拉丁")]
        ALD_阿拉丁 = 27,

        [EnumDescription("招财进宝")]
        ZCJB_招财进宝 = 29,

        [EnumDescription("经典21点")]
        JD21D_经典21点 = 40,

        [EnumDescription("五龙争霸")]
        WLZB_五龙争霸 = 1001,

        [EnumDescription("星光派对")]
        XGPD_星光派对 = 1002,

        [EnumDescription("熊猫报喜")]
        XMBX_熊猫报喜 = 1003,

        [EnumDescription("金吉报喜_无尽宝藏")]
        JJBX_金吉报喜_无尽宝藏 = 1004,

        [EnumDescription("东海之王")]
        DHZW_东海之王 = 1005,

        [EnumDescription("五龙争霸_彩金版")]
        WLZBJP_五龙争霸_彩金版 = 1006,

        [EnumDescription("财神到_彩金版")]
        CSDJP_财神到_彩金版 = 1007,

        [EnumDescription("五龙皇朝")]
        WLHC_五龙皇朝 = 1008,

        [EnumDescription("百萬金輪五龍爭霸")]
        WLJR_百萬金輪五龍爭霸 = 1009,

        [EnumDescription("擊鼓迎福")]
        JGYF_擊鼓迎福 = 1010,

        [EnumDescription("吉祥如意")]
        JXRY_吉祥如意 = 1011,

        [EnumDescription("財富連連")]
        CFLL_財富連連 = 1012,

        [EnumDescription("加勒比扑克")]
        JLBPK_加勒比扑克 = 2001,

        [EnumDescription("赌场战争")]
        DCZZ_赌场战争 = 2003,

        [EnumDescription("海盗船")]
        HDC_海盗船 = 2004,

        [EnumDescription("赌场黑杰克")]
        DCHJK_赌场黑杰克 = 2005,

        [EnumDescription("红包扫雷")]
        HBSL_红包扫雷 = 2006,

        [EnumDescription("富贵三宝")]
        FGSB_富贵三宝 = 2007,

        [EnumDescription("超级六")]
        CJL_超级六 = 2008,



        [EnumDescription("APL彩票")]
        [EnumSettingValue("LO")]
        APL_Lottery = 9001,


        [EnumDescription("BG娱乐城")]
        [EnumSettingValue("BG")]
        BG_Casino = 9032,
        

        [EnumDescription("APL捕鱼")]
        [EnumSettingValue("FH")]
        APL_Fish = 9051,


        [EnumDescription("BG捕鱼")]
        [EnumSettingValue("")]
        BG_Fish = 9052,


        [EnumDescription("EGaming娱乐城")]
        [EnumSettingValue("")]
        EGaming_BJL = 777031,

    }

}
