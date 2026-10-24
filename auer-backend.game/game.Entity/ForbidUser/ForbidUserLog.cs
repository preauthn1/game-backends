using FW.Core.Helper;
using FW.Core.Helper.Enum;
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
    /// 封号记录
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class ForbidUserLog : BaseEntity
    {
        /// <summary>
        /// 封号账号
        /// </summary>
        public int UserID
        {
            get;
            set;
        }

        /// <summary>
        /// 封号类型
        /// </summary>
        public ForbidType ForbidType
        {
            get;
            set;
        }

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
        /// 说明
        /// </summary>
        [Required, StringLength(128)]
        public string Memo
        {
            get;
            set;
        }

        /// <summary>
        /// IP
        /// </summary>
        public string IPAddress
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

        public virtual UserInfo UserInfo
        {
            get;
            set;
        }


    }

    /// <summary>
    /// 封号类型
    /// </summary>
    public enum ForbidType : byte
    {
        [EnumDescription("封号")]
        Forbid = 0,
        [EnumDescription("解封")]
        UnForbid = 1
    }
}
