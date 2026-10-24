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
    /// 报警类型
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class WarningType : BaseEntity
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string TypeName { get; set; }
        
        /// <summary>
        /// 报警类型状态
        /// </summary>
        public WarningTypeStatus Status { get; set; }

        /// <summary>
        /// 报警配置 XML格式数据
        /// </summary>
        public string Setting { get; set; }


    }

    /// <summary>
    /// 报警类型状态
    /// </summary>
    public enum WarningTypeStatus : byte
    {
        [EnumDescription("禁用")]
        Disabled = 0,
        [EnumDescription("启用")]
        Enabled = 1
    }
}
