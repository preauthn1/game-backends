using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Warning
{
    /// <summary>
    /// 后台帐号 报警提示
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class WarningStatus : BaseEntity
    {
        /// <summary>
        /// 后台帐号是否提示报警
        /// </summary>
        public WarningStatusStatus Status { get; set; }
   
    }


    /// <summary>
    /// 后台帐号是否提示报警
    /// </summary>
    public enum WarningStatusStatus : byte
    {
        [EnumDescription("不提示")]
        No = 0,
        [EnumDescription("提示")]
        Yes = 1
    }
}
