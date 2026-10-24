using FW.Core.Helper.Enum;
using game.Entity.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Users
{
    public class EventSetting
    {
        /// <summary>
        /// 活動編號
        /// </summary>
        public int? EventID { get; set; }
        /// <summary>
        /// 活動名稱
        /// </summary>
        public string EventName { get; set; }
        /// <summary>
        /// 起始時間
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 結束時間
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 狀態
        /// </summary>
        public Status Status { get; set; }
        /// <summary>
        /// 最低儲值金額
        /// </summary>
        [MoneyInt]
        public long GiftCash { get; set; }
        /// <summary>
        /// 修改者
        /// </summary>
        public string Updater { get; set; }
        /// <summary>
        /// 修改時間
        /// </summary>
        public DateTime UpdateTime { get; set; }

        private string _statusName;
        public string StatusName
        {
            get { return _statusName; }
            set { _statusName = Status.GetDisplayName(); }
        }

        private string _strStartTime;
        public string StrStartTime
        {
            get { return _strStartTime; }
            set { _strStartTime = StartTime.HasValue ? StartTime.Value.ToString("yyyy-MM-dd HH:mm") : "-";}
        }

        private string _strEndTime;
        public string StrEndTime
        {
            get { return _strEndTime; }
            set { _strEndTime = EndTime.HasValue ? EndTime.Value.ToString("yyyy-MM-dd HH:mm") : "-"; }
        }

        private string _strUpdateTime;
        public string StrUpdateTime
        {
            get { return _strUpdateTime; }
            set { _strUpdateTime = UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"); }
        }
    }
}
