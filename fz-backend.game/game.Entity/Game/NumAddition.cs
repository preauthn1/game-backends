using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
    /// <summary>
    /// 人数加成配置
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class NumAddition : BaseEntity
    {
        /// <summary>
        /// 服务器ID   
        /// </summary>
        public int ServerID { get; set; }

        /// <summary>
        /// 服务器类型  0大厅 1房间
        /// </summary>
        public NumAdditionType ServerType { get; set; }

        /// <summary>
        /// 加成比例
        /// </summary>
        public decimal Addition { get; set; }

        /// <summary>
        /// 加成后增加随机数量 最小值
        /// </summary>
        public int RndStart { get; set; }

        /// <summary>
        /// 加成后增加随机数量 最大值
        /// </summary>
        public int RndEnd { get; set; }
    }

    public enum NumAdditionType : byte
    {
        [EnumDescription("大厅")]
        Hall,
        [EnumDescription("房间")]
        Room
    }


}
