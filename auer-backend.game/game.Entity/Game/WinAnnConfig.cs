using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
    /// <summary>
    /// 赢了通知配置
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class WinAnnConfig : BaseEntity
    {
        /// <summary>
        /// 自动通知金额上限
        /// </summary>
        [MoneyInt]
        public long UpperLimit
        {
            get;
            set;
        }

        public virtual Game GameInfo
        {
            get;
            set;
        }
    }
}
