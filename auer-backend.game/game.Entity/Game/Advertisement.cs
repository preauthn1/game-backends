using System;
using FW.Core.Helper.Enum;

namespace game.Entity.Game
{
    public class Advertisement
    {
        public int Id { get; set; }
        public int PlatformID { get; set; }
        public int AreaID { get; set; }
        public string Picture { get; set; }
        public string Link { get; set; }
        public Behavior Behavior { get; set; }
        public Status Status { get; set; }
        public DateTime CreateTime { get; set; }

        private string _behaviorName;
        public string BehaviorName
        {
            get { return _behaviorName; }
            set { _behaviorName = Behavior.GetDisplayName(); }
        }

        private string _statusName;
        public string StatusName
        {
            get { return _statusName; }
            set { _statusName = Status.GetDisplayName(); }
        }

        private string _strCreateTime;
        public string StrCreateTime
        {
            get { return _strCreateTime; }
            set { _strCreateTime = CreateTime.ToString("yyyy-MM-dd HH:mm"); }
        }

        /// <summary>
        /// 是否設定為预设公告使用
        /// </summary>
        public byte? SetDefault { get; set; }


    }

    public enum Behavior
    {
        [EnumDescriptionAttribute("複製网址")]
        Copy = 0,
        [EnumDescriptionAttribute("连结广告位置")]
        Location = 1,
        [EnumDescriptionAttribute("一键入厅")]
        IntoGame = 2,
        [EnumDescriptionAttribute("無行為")]
        NoAction = 3,
        [EnumDescriptionAttribute("前往充值")]
        GoPayment = 4,
        [EnumDescriptionAttribute("前往苹果商店")]
        GoApple = 5,
        [EnumDescriptionAttribute("取得代理微信联络资讯")]
        GoWeChat = 6,
        [EnumDescriptionAttribute("跳转渠道官网")]
        GoOfficialSite = 7,
        [EnumDescriptionAttribute("前往活动页面")]
        GoCampaign = 8,
        [EnumDescriptionAttribute("前往首充返利页面")]
        GoFirstDeposit = 9
    }

    public enum Status
    {
        [EnumDescriptionAttribute("关")]
        Close = 0,
        [EnumDescriptionAttribute("开")]
        Open = 1,
        [EnumDescriptionAttribute("刪除")]
        Delete = -1
    }
}
