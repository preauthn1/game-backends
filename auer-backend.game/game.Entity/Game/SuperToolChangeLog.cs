using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
    public class SuperToolChangeLog
    {
        /// <summary>
        /// 識別ID
        /// </summary>
        public int LogID { get; set; }
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
        /// 類別1: 編輯獎池金額 2:增加放分金額
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 紀錄
        /// </summary>
        public string Result { get; set; }
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
