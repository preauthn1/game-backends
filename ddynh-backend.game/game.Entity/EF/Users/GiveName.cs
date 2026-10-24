using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Users
{
    /// <summary>
    /// 姓、名数据库
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class GiveName : BaseEntity
    {
        /// <summary>
        /// 名称
        /// </summary>
        [Required]
        [StringLength(32)]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// 类型
        /// </summary>
        public GiveNameType Type
        {
            get;
            set;
        }
    }

}
