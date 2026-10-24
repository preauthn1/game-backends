using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Users
{
    public class EventDetailLog
    {
        /// <summary>
        /// 活動編號
        /// </summary>
        public int EventID { get; set; }
        /// <summary>
        /// 活動名稱
        /// </summary>
        public string EventName { get; set; }
        /// <summary>
        /// 使用者ID
        /// </summary>
        public int UserID { get; set; }
        /// <summary>
        /// 登入名稱
        /// </summary>
        public string LoginName { get; set; }
        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 領取金額
        /// </summary>
        public long GiftCash { get; set; }
        /// <summary>
        /// IP位置
        /// </summary>
        public string IPAddress { get; set; }
        /// <summary>
        /// 起始時間
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 結束時間
        /// </summary>
        public DateTime? EndTime { get; set; }

        private string _strCreateTime;
        public string StrCreateTime
        {
            get { return _strCreateTime; }
            set { _strCreateTime = CreateTime.ToString("yyyy-MM-dd HH:mm"); }
        }

        private string _strStartTime;
        public string StrStartTime
        {
            get { return _strStartTime; }
            set { _strStartTime = StartTime.HasValue ? StartTime.Value.ToString("yyyy-MM-dd HH:mm") : "-"; }
        }

        private string _strEndTime;
        public string StrEndTime
        {
            get { return _strEndTime; }
            set { _strEndTime = EndTime.HasValue ? EndTime.Value.ToString("yyyy-MM-dd HH:mm") : "-"; }
        }
    }
}
