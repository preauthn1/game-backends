using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Stat
{
    /// <summary>
    /// 渠道日表报数据
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class GradeSetting : BaseEntity
    {
        /// <summary>
        /// 等级限制的贡献税收数量下限
        /// </summary>
        [MoneyInt]
        public long Amount { get; set; }

        public long UserCount { get; set; }
    }
}
