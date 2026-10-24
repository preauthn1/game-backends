using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Stat
{
    public class ActiveUserKeepLog
    {
        /// <summary>
        /// 统计时间
        /// </summary>
        public DateTime StatTime { get; set; }
        /// <summary>
        /// 當日有效會員
        /// 当天新增的用户中，任意进入游戏中体验游戏时长>=30秒以上的新用户数量
        /// </summary>
        public int ActiveUsers { get; set; }
        /// <summary>
        /// 1天留存
        /// </summary>
        public double Day1Keep { get; set; }
        /// <summary>
        /// 3天留存
        /// </summary>
        public double Day3Keep { get; set; }
        /// <summary>
        /// 5天留存
        /// </summary>
        public double Day5Keep { get; set; }
        /// <summary>
        /// 7天留存
        /// </summary>
        public double Day7Keep { get; set; }
        /// <summary>
        /// 14天留存
        /// </summary>
        public double Day14Keep { get; set; }
        /// <summary>
        /// 30天留存
        /// </summary>
        public double Day30Keep { get; set; }
        /// <summary>
        /// 3天付費率
        /// 新增有效用户，在注册日起3日内的累积付费人数/新增有效用户的人数
        /// </summary>
        public double Day3Payrate { get; set; }

        /// <summary>
        /// 5天付費率
        /// </summary>
        public double Day5Payrate { get; set; }
        /// <summary>
        /// 7天付費率
        /// </summary>
        public double Day7Payrate { get; set; }
    }
}
