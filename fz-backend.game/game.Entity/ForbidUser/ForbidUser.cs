using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.ForbidUser
{
    /// <summary>
    /// 封号用户
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class ForbidUser : BaseEntity
    {
        /// <summary>
        /// 现金数
        /// </summary>
        [MoneyInt]
        public Int64 CashAmount
        {
            get;
            set;
        }

        /// <summary>
        /// 银行数
        /// </summary>
        [MoneyInt]
        public Int64 BankAmount
        {
            get;
            set;
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get;
            set;
        }

        /// <summary>
        /// 说明
        /// </summary>
        [Required, StringLength(128)]
        public string Memo
        {
            get;
            set;
        }

        /// <summary>
        /// 用户
        /// </summary>
        public virtual UserInfo UserInfo
        {
            get;
            set;
        }

    }
}
