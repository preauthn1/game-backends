using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
    public class SuperToolVIPUser_ZJH
    {
        /// <summary>
        /// 玩家ID
        /// </summary>
        public int UserID { get; set; }
        /// <summary>
        /// 下局指定牌型
        /// </summary>
        public int? NextType { get; set; }
        /// <summary>
        /// 下局输赢
        /// </summary>
        public int? NextWinLose { get; set; }
        /// <summary>
        /// 下局连续输赢局数
        /// </summary>
        public int? NextWinLoseNum { get; set; }
        /// <summary>
        /// 是否完成
        /// </summary>
        public int Finish { get; set; }
        /// <summary>
        /// 是否完成下局连续输赢局数
        /// </summary>
        public int FinishNextWinLoseNum { get; set; }
        /// <summary>
        /// 下局指定進入桌號
        /// </summary>
        public int? NextTable { get; set; }
        /// <summary>
        /// 狀態 0 暂停 1 继续
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

        private string _strFinish;
        public string StrFinish
        {
            get { return _strFinish; }
            set { _strFinish = Finish == 0 ? "否" : "是"; }
        }
    }
}
