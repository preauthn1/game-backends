using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Stat
{
    /// <summary>
    /// 游戏房间税收记录
    /// </summary>
    public class GameServerTaxLog
    {
        /// <summary>
        /// 游戏
        /// </summary>
        public string GameName
        {
            get;
            set;
        }

        /// <summary>
        /// 服务器
        /// </summary>
        public string ServerName
        {
            get;
            set;
        }

        /// <summary>
        /// 金额
        /// </summary>
        [MoneyInt]
        public Int64 Amount
        {
            get;
            set;
        }

        [MoneyInt]
        public Int64 RobotAmount
        {
            get;
            set;
        }

        /// <summary>
        /// 税收
        /// </summary>
        [MoneyInt]
        public Int64 TaxAmount
        {
            get;
            set;
        }

        [MoneyInt]
        public Int64 RobotTaxAmount
        {
            get;
            set;
        }
       
        /// <summary>
        /// 统计时间
        /// </summary>
        public DateTime StatTime
        {
            get;
            set;
        }

        /// <summary>
        /// 流水
        /// </summary>
        [MoneyInt]
        public Int64 FlowAmount
        {
            get;
            set;
        }
    }
}
