using FW.Core.Helper.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
    public class ProtectNodeLog
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public int UserID { get; set; }
        public int Action { get; set; }
        public string BackupNodeIP { get; set; }
        public string NodeIP { get; set; }
        public string UserIP { get; set; }
        public string UserFrom { get; set; }
        public string HDCode { get; set; }
        public ProtectNodeStatus Status { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }

        private string _strCreateTime;
        public string StrCreateTime
        {
            get { return _strCreateTime; }
            set { _strCreateTime = CreateTime.ToString("yyyy-MM-dd HH:mm:ss"); }
        }

        private string _strStatus;
        public string StrStatus
        {
            get { return _strStatus; }
            set { _strStatus = Status.ToString(); }
        }

        private string _strAction;
        public string StrAction
        {
            get { return _strAction; }
            set { _strAction = Action == 2 ? "直接取得备用节点" : "一般"; }
        }
    }


}
