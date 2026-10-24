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
    /// 银行流通类型
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class BankSourceType : BaseEntity
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

        public BankSourceChangeType ChangeType
        {
            get;
            set;
        }

    }


}
