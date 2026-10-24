using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Users
{
    public class EventLog
    {       
        
        public int EventType { get; set; }


        public int EventSettingID { get; set; }

        /// <summary>
        /// 活動名稱
        /// </summary>
        public string EventName { get; set; }
        /// <summary>
        /// 使用者編號
        /// </summary>
        public int? UserID { get; set; }
        /// <summary>
        /// 登入名稱
        /// </summary>
        public string LoginName { get; set; }
        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 領取次數
        /// </summary>
        public int GrantTimes { get; set; }


        private string _strUpdateTime;


        public string StrUpdateTime
        {
            get { return _strUpdateTime; }
            set { _strUpdateTime = UpdateTime.HasValue ? UpdateTime.Value.ToString("yyyy-MM-dd HH:mm") : "-"; }
        }

    }
}
