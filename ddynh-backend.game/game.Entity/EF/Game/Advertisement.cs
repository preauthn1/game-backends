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
}
