using System;
using FW.Core.Helper;
using FW.Core.Repository;

namespace game.Entity.Stat
{
    /// <summary>
    /// 渠道日表报数据
    /// </summary>
    [Database(Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class AreaDataDay : BaseEntity
    {
        /// <summary>
        /// 老用户充值数
        /// </summary>
        public long OldCharge { get; set; }
        /// <summary>
        /// 新用户充值数
        /// </summary>
        public long NewCharge { get; set; }
        /// <summary>
        /// 老用户兑换数
        /// </summary>
        public long OldConvert { get; set; }
        /// <summary>
        /// 新用户兑换数
        /// </summary>
        public long NewConvert { get; set; }


        /// <summary>
        /// 老用户兑换手续费
        /// </summary>
        public long OldConvertTax { get; set; }
        /// <summary>
        /// 新用户兑换手续费
        /// </summary>
        public long NewConvertTax { get; set; }
        /// <summary>
        /// 老用户登录数
        /// </summary>
        public long OldLogin { get; set; }
        /// <summary>
        /// 新用户登录数
        /// </summary>
        public long NewLogin { get; set; }
        /// <summary>
        /// 登录IP总数
        /// </summary>
        public long LoginIP { get; set; }
        /// <summary>
        /// 老用户登录IP总数
        /// </summary>
        public long OldLoginIP { get; set; }
        /// <summary>
        /// 新用户登录IP总数
        /// </summary>
        public long NewLoginIP { get; set; }
        /// <summary>
        /// 登录Mac总数
        /// </summary>
        public long LoginMac { get; set; }
        /// <summary>
        /// 老用户登录Mac总数
        /// </summary>
        public long OldLoginMac { get; set; }
        /// <summary>
        /// 新用户登录Mac总数
        /// </summary>
        public long NewLoginMac { get; set; }
        /// <summary>
        /// 游戏税收
        /// </summary>
        public long GameTax { get; set; }
        /// <summary>
        /// 游戏税收详情
        /// </summary>
        public string GameTaxDetail { get; set; }
        /// <summary>
        /// 渠道ID 0表示所有渠道的数据
        /// </summary>
        public int AreaID { get; set; }

        /// <summary>
        /// 渠道名称
        /// </summary>
        public string AreaName { get; set; }
        /// <summary>
        /// 平台ID 0表示所有平台的数据
        /// </summary>
        /// 
        public int PlatformID { get; set; }

        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; set; }

        /// <summary>
        /// 统计时间
        /// </summary>
        public DateTime StatTime { get; set; }

        /// <summary>
        /// 付費平均比
        /// </summary>
        public long ARPU { get; set; }
        
        /// <summary>
        /// 註冊充值人數
        /// </summary>
        public long RegRechargeUser { get; set; }
        /// <summary>
        /// 當天註冊充值人數
        /// </summary>
        public long NewOrderUser { get; set; }

       
        public long TotalOrder { get; set; }
        /// <summary>
        /// 每日充值人数
        /// </summary>
        public long Total_OrderUser { get; set; }

        /// <summary>
        /// 币商充值
        /// </summary>
        public long TotalAgentChargeAmount { get; set; }
        /// <summary>
        /// 币商充值人数
        /// </summary>
        public long TotalAgentChargeUser { get; set; }
        /// <summary>
        /// 兑换
        /// </summary>
        public long TotalConvertAmount { get; set; }
        /// <summary>
        /// 兑换人数
        /// </summary>
        public long TotalConvertUser { get; set; }
        /// <summary>
        /// 活跃人数
        /// </summary>
        public long DAU { get; set; }
        /// <summary>
        /// 註冊用戶人數
        /// </summary>
        public long TotalRegCount { get; set; }
        /// <summary>
        /// 注册设备数
        /// </summary>
        public long TotalLoginMac { get; set; }
        /// <summary>
        /// 注册IP数
        /// </summary>
        public long TotalLoginIP { get; set; }
        /// <summary>
        /// 绑定手机数
        /// </summary>
        public long TotalBindUser { get; set; }
        /// <summary>
        /// 次日留存率
        /// </summary>
        public string OneDayKeep { get; set; }
        /// <summary>
        /// 三日留存率
        /// </summary>
        public string OneThirdDayKeep { get; set; }
        /// <summary>
        /// 当日新增充值金额
        /// </summary>
        public long TotalNewCharge { get; set; }
    }

    public class AreaDataDaySummary
    {
        public DateTime StatTime { get; set; }
        public int AreaID { get; set; }
        public string AreaName { get; set; }

        //營收 
        /// <summary>
        /// 營收
        /// </summary>
        public long Income { get; set; }
        /// <summary>
        /// 用戶現金
        /// </summary>
        public long TotalMoney { get; set; }
        /// <summary>
        /// 用戶保險箱現金
        /// </summary>
        public long TotalBankMoney { get; set; }

        //充值
        /// <summary>
        /// 不重複充值人數
        /// </summary>
        public int Total_OrderUser { get; set; }
        /// <summary>
        /// 充值成功筆數
        /// </summary>
        public int TotalOrderCounts { get; set; }
        /// <summary>
        /// 新用户不重複充值人數
        /// </summary>
        public int NewOrderUser { get; set; }
        /// <summary>
        /// 總充值金額
        /// </summary>
        public long TotalCharge { get; set; }
        /// <summary>
        /// 舊用戶充值金額 (已經包含代理充值數據)
        /// </summary>
        public long OldCharge { get; set; }
        /// <summary>
        /// 新用戶充值金額(已經包含代理充值數據)
        /// </summary>
        public long NewCharge { get; set; }
        /// <summary>
        /// 舊用戶代理充值金額
        /// </summary>
        public long OldAgentCharge { get; set; }
        /// <summary>
        /// 新用戶代理充值金額
        /// </summary>
        public long NewAgentCharge { get; set; }


        //兌換
        /// <summary>
        /// 兌換去重用戶數
        /// </summary>
        public int TotalConvertUser { get; set; }
        /// <summary>
        /// 新用戶兌換去重用戶數
        /// </summary> 
        public int NewConvertUser { get; set; }
        /// <summary>
        /// 總兌換金額
        /// </summary>
        public long TotalConvert { get; set; }
        /// <summary>
        /// 舊用戶兌換金額
        /// </summary>
        public long OldConvert { get; set; }
        /// <summary>
        /// 新用戶兌換金額
        /// </summary>
        public long NewConvert { get; set; }
        /// <summary>
        /// 舊用戶兌換手續費金額
        /// </summary>
        public long OldConvertTax { get; set; }
        /// <summary>
        /// 新用戶兌換手續費金額
        /// </summary>
        public long NewConvertTax { get; set; }

        //註冊用戶數
        /// <summary>
        /// 當日註冊用戶總數
        /// </summary>
        public int TotalRegCount { get; set; }
        /// <summary>
        /// 當日註冊IOS用戶數
        /// </summary>
        public int IOSRegCount { get; set; }
        /// <summary>
        /// 當日註冊安卓用戶數
        /// </summary>
        public int AndroidRegCount { get; set; }
        /// <summary>
        /// 當日綁定用戶數
        /// </summary>
        public int TotalBindUser { get; set; }
        /// <summary>
        /// 當日綁定IOS用戶數
        /// </summary>
        public int BindIOSUser { get; set; }
        /// <summary>
        /// 當日綁定安卓用戶數
        /// </summary>
        public int BindAndroidUser { get; set; }

        //登錄用戶數
        /// <summary>
        /// 舊用戶数登入游戏
        /// </summary>
        public int OldLogin { get; set; }
        /// <summary>
        /// 新用户数登入游戏
        /// </summary>
        public int NewLogin { get; set; }
        /// <summary>
        /// 蘋果註冊數且登入遊戲
        /// </summary>
        public int IOSCount { get; set; }
        /// <summary>
        /// 新用戶蘋果註冊數且登入遊戲
        /// </summary>
        public int NewiOSCount { get; set; }
        /// <summary>
        /// 安卓註冊數且登入遊戲
        /// </summary>
        public int AndroidCount { get; set; }
        /// <summary>
        /// 新用戶安卓註冊數且登入遊戲
        /// </summary>
        public int NewAndroidCount { get; set; }
        /// <summary>
        /// 1日留存
        /// </summary>
        public int Keep1 { get; set; }
        /// <summary>
        /// 3日留存
        /// </summary>
        public int Keep3 { get; set; }
        /// <summary>
        /// 5日留存
        /// </summary>
        public int Keep5 { get; set; }
        /// <summary>
        /// 7日留存
        /// </summary>
        public int Keep7 { get; set; }


        //稅收
        /// <summary>
        /// 系统税收, 用戶(不含機器人)游戏税收
        /// </summary>
        public long GameTax { get; set; }
        /// <summary>
        /// 用戶輸贏, 用戶(不含機器人)遊戲輸贏金額
        /// </summary>
        public long UserGameAmount { get; set; }
        /// <summary>
        /// 系統輸贏, 機器人遊戲輸贏金額
        /// </summary>
        public long RobotGameAmount { get; set; }
        /// <summary>
        /// 遊戲輸贏, 用戶+机器人輸贏總金額
        /// </summary>
        public long AmountTotal { get; set; }
        /// <summary>
        /// 游戏税收內容
        /// </summary>
        public string GameTaxDetail { get; set; }     
    }

    public class DailyMoneyLog
    {
        public string stattime { get; set; }
        public int TotalRegCount { get; set; }
        public float Charge { get; set; }
        public float Convert { get; set; }
        public float Rate { get; set; }
        public float DiffAmount { get; set; }
    }
}
