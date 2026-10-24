using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Stat
{
    public class ChargeKeepLog
    {
        /// <summary>
        /// 统计时间
        /// </summary>
        public DateTime StatTime { get; set; }
        /// <summary>
        /// 2 Android
        /// 3 IOS
        /// </summary>
        public int HDType { get; set; }
        /// <summary>
        /// 當日註冊且充值人數
        /// </summary>
        public int RegChargeUsers { get; set; }
        /// <summary>
        /// 1天留存
        /// </summary>
        public double Day1Keep { get; set; }
        /// <summary>
        /// 2天留存
        /// </summary>
        public double Day2Keep { get; set; }
        /// <summary>
        /// 7天留存
        /// </summary>
        public double Day7Keep { get; set; }
        /// <summary>
        /// 30天留存
        /// </summary>
        public double Day30Keep { get; set; }
        /// <summary>
        /// 平台名
        /// </summary>
        public string Platform { get; set; }
    }
}
