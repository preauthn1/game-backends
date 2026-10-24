using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
    public class WinPrizeLog : BaseEntity
    {
        /// <summary>
        /// 修改人
        /// </summary>
        public string AdminUser { get; set; }
        /// <summary>
        /// 遊戲編號
        /// </summary>
        public int GameID { get; set; }
        /// <summary>
        /// 伺服器編號
        /// </summary>
        public int ServerID { get; set; }
        /// <summary>
        /// 桌子編號
        /// </summary>
        public int TableNo { get; set; }
        /// <summary>
        /// 開獎類型
        /// </summary>
        public string PrizeType { get; set; }
        /// <summary>
        /// 牌局編號
        /// </summary>
        public long LogID { get; set; }
        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 創建時間
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 查詢開始時間
        /// </summary>
        public DateTime? BeginTime { get; set; }
        /// <summary>
        /// 查詢結束時間
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 格式化更新時間
        /// </summary>
        private string _strUpdateTime;
        public string StrUpdateTime
        {
            get { return _strUpdateTime; }
            set { _strUpdateTime = UpdateTime == null ? "-" : UpdateTime.Value.ToString("yyyy/MM/dd HH:mm:ss"); }
        }
        /// <summary>
        /// 格式化創建時間
        /// </summary>
        private string _strCreateTime;
        public string StrCreateTime
        {
            get { return _strCreateTime; }
            set { _strCreateTime = CreateTime.ToString("yyyy/MM/dd HH:mm:ss"); }
        }

    }
}
