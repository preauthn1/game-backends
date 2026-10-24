using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
    /// <summary>
    /// 游戏信息
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class Game : BaseEntity
    {
        /// <summary>
        /// 游戏名
        /// </summary>
        [Required]
        [StringLength(64)]
        public string GameName
        {
            get;
            set;
        }
        /// <summary>
        /// 安卓版本
        /// </summary>
        public string AndroidVersion
        {
            get;
            set;
        }
        /// <summary>
        /// iOS版本
        /// </summary>
        public string IOSVersion
        {
            get;
            set;
        }

        /// <summary>
        /// 状态
        /// </summary>
        public byte Status
        {
            get;
            set;
        }
    }
}
