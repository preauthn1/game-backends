using FW.Core.Helper.Enum;
using game.Entity.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Users
{
    public class SevenDaysLoginBonus
    {
        /// <summary>
        /// 平台編號
        /// </summary>
        public int? PlatformID { get; set; }
        /// <summary>
        /// 平台名稱
        /// </summary>
        public string PlatformName { get; set; }
        /// <summary>
        /// 渠道編號
        /// </summary>
        public int? AreaID { get; set; }
        /// <summary>
        /// 渠道名稱
        /// </summary>
        public string AreaName { get; set; }
        /// <summary>
        /// 第一天獎勵金額
        /// </summary>
        public int? Day1 { get; set; }
        /// <summary>
        /// 第二天獎勵金額
        /// </summary>
        public int? Day2 { get; set; }
        /// <summary>
        /// 第三天獎勵金額
        /// </summary>
        public int? Day3 { get; set; }
        /// <summary>
        /// 第四天獎勵金額
        /// </summary>
        public int? Day4 { get; set; }
        /// <summary>
        /// 第五天獎勵金額
        /// </summary>
        public int? Day5 { get; set; }
        /// <summary>
        /// 第六天獎勵金額
        /// </summary>
        public int? Day6 { get; set; }
        /// <summary>
        /// 第七天獎勵金額
        /// </summary>
        public int? Day7 { get; set; }
        /// <summary>
        /// 狀態
        /// </summary>
        public Status Status { get; set; }
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

        private string _strUpdateTime;
        public string StrUpdateTime
        {
            get { return _strUpdateTime; }
            set { _strUpdateTime = UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"); }
        }
    }
}
