using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Money
{
    /// <summary>
    /// 现金流通类型
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class MoneySourceType : BaseEntity
    {
        /// <summary>
        /// 名称
        /// </summary>
        [Required]
        [StringLength(32)]
        public string SourceName
        {
            get;
            set;
        }

        /// <summary>
        /// 流通类型
        /// </summary>
        public MoneySourceChangeType ChangeType
        {
            get;
            set;
        }

        /// <summary>
        /// 分组ID
        /// </summary>
        public int? GroupID
        {
            get;
            set;
        }

        /// <summary>
        /// 分组
        /// </summary>
        public virtual MoneySourceGroup Group { get; set; }

    }

    /// <summary>
    /// 现金变化方式
    /// </summary>
    public enum MoneySourceChangeType : byte
    {
        [EnumDescription("流入")]
        InFlow = 0,
        [EnumDescription("流出")]
        OutFlow = 1,
        [EnumDescription("流通")]
        Flow = 2
    }
}
