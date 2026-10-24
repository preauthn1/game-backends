using System;
using FW.Core.Helper.Enum;

namespace game.Entity.Game
{
    public class Campaign
    {
        public int Id { get; set; }
        public int PlatformID { get; set; }
        public int AreaID { get; set; }
        public string Picture { get; set; }
        public int SeqID { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public CampaignBehavior CampaignBehavior { get; set; }
        public CampaignTag CampaignTag { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public CampaignStatus CampaignStatus { get; set; }
        public DateTime CreateTime { get; set; }

        private string _campaignTagName;
        public string CampaignTagName
        {
            get { return _campaignTagName; }
            set { _campaignTagName = CampaignTag.GetDisplayName(); }
        }

        private string _campaignBehaviorName;
        public string CampaignBehaviorName
        {
            get { return _campaignBehaviorName; }
            set { _campaignBehaviorName = CampaignBehavior.GetDisplayName(); }
        }

        private string _campaignStatusName;
        public string CampaignStatusName
        {
            get { return _campaignStatusName; }
            set { _campaignStatusName = CampaignStatus.GetDisplayName(); }
        }

        private string _strStartTime;
        public string StrStartTime
        {
            get { return _strStartTime; }
            set { _strStartTime = StartTime.ToString("yyyy-MM-dd HH:mm"); }
        }
        private string _strEndTime;
        public string StrEndTime
        {
            get { return _strEndTime; }
            set { _strEndTime = EndTime.ToString("yyyy-MM-dd HH:mm"); }
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

    public enum CampaignTag
    {
        [EnumDescriptionAttribute("无")]
        No = 0,
        [EnumDescriptionAttribute("热门")]
        Hot = 1,
        [EnumDescriptionAttribute("最新")]
        Latest = 2,
        [EnumDescriptionAttribute("推荐")]
        Recommend = 3,
        [EnumDescriptionAttribute("优惠")]
        Sales = 4
    }

    public enum CampaignBehavior
    {
        [EnumDescriptionAttribute("无")]
        NoAction = 0,
        [EnumDescriptionAttribute("开启浏览器并连结广告位置")]
        LaunchBrowser = 1
    }

    public enum CampaignStatus
    {
        [EnumDescriptionAttribute("关")]
        Close = 0,
        [EnumDescriptionAttribute("开")]
        Open = 1,
        [EnumDescriptionAttribute("刪除")]
        Delete = -1
    }
}
