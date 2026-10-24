using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Money
{
    /// <summary>
    /// 银行记录
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class BankLog : BaseEntity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserID
        {
            get;
            set;
        }

        /// <summary>
        /// 流通类型
        /// </summary>
        public int SourceTypeID
        {
            get;
            set;
        }

        /// <summary>
        /// 当前金币数
        /// </summary>
        [MoneyInt] 
        public Int64 CashAmount
         {
             get;
             set;
         }


        /// <summary>
        /// 当前金额
        /// </summary>
        public Int64 CurrentAmount
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

        /// <summary>
        /// IP地址
        /// </summary>
        public string IPAddress
        {
            get;
            set;
        }

        /// <summary>
        /// 说明
        /// </summary>
        public string Memo
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

        public virtual UserInfo User
        {
            get;
            set;
        }

        public virtual BankSourceType SourceType
        {
            get;
            set;
        }

    }
}
