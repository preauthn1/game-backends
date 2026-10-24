using FW.Core.Helper;
using FW.Core.Repository;
using System;

namespace game.Entity.Money
{
    /// <summary>
    /// 银行记录
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class BankLogSourceType : BaseEntity
    {
        /// <summary>
        /// 流通类型
        /// </summary>
        public int SourceTypeID
        {
            get;
            set;
        }

        /// <summary>
        /// 更新的数量
        /// </summary>
        [MoneyInt]
        public Int64 UpdateAmount
        {
            get;
            set;
        }
    }
}
