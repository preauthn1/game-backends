using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
    public class SuperToolAssignReward
    {
        /// <summary>
        /// 識別碼
        /// </summary>
        public int LogID { get; set; }
        /// <summary>
        /// 遊戲編號
        /// </summary>
        public int GameID { get; set; }
        /// <summary>
        /// 遊戲局號(showName)
        /// </summary>
        public int FlagID { get; set; }
        /// <summary>
        /// 桌號
        /// </summary>
        public int TableNo { get; set; }
        /// <summary>
        /// 完成度
        /// </summary>
        public int Complete { get; set; }
        /// <summary>
        /// 開獎結局
        /// </summary>
        public string Result { get; set; }
        /// <summary>
        /// 狀態 0 取消  1 啟用 2 暫停
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 修改人
        /// </summary>
        public string Operator { get; set; }
        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreateTime { get; set; }

        private string _strCreateTime;
        public string StrCreateTime
        {
            get { return _strCreateTime; }
            set { _strCreateTime = CreateTime.ToString("yyyy/MM/dd HH:mm:ss"); }
        }

    }

    
}
