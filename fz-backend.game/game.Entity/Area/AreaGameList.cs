using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FW.Core.Repository;

namespace game.Entity.Area
{
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class AreaGameList : BaseEntity
    {
        /// <summary>
        /// 区域ID
        /// </summary>
        public int AreaID { get; set; }

        /// <summary>
        /// 游戏ID
        /// </summary>
        public int GameID { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int Sort { get; set; }

        public virtual Area Area { get; set; }

        public virtual Game.Game Game { get; set; }
    }
}
