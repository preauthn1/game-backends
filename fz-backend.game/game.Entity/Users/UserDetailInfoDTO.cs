using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Users
{
    /// <summary>
    /// 用户详情中查询范围内的统计信息
    /// </summary>
    public class UserDetailInfoDTO
    {
        /// <summary>
        /// 相同ip地址
        /// </summary>
        public int SameIPCount { get; set; }
        /// <summary>
        /// 相同mac地址
        /// </summary>
        public int SameMacCount { get; set; }

        /// <summary>
        /// 总充值金额
        /// </summary>
        public long SumCharge { get; set; }
        /// <summary>
        /// 总兑换金额
        /// </summary>
        public long SumConvert { get; set; }
        /// <summary>
        /// 总游戏税收金额
        /// </summary>
        public long SumGameTax { get; set; }

    }
}
