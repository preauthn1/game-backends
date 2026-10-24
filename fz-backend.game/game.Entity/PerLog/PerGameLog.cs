using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.PerLog
{
    /// <summary>
    /// 每局游戏记录
    /// </summary>    
    public class PerGameLog
    {
        public Int64 Id { get; set; }

        /// <summary>
        /// 牌局标识
        /// </summary>
        public string LogID { get; set; }

        /// <summary>
        /// 牌局编号
        /// </summary>
        public Int64 DealNo { get; set; }

        /// <summary>
        /// 服务器ID
        /// </summary>
        public int ServerID { get; set; }

        /// <summary>
        /// 服务器名称
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// 游戏名称
        /// </summary>
        public string GameName { get; set; }

        /// <summary>
        /// 税收
        /// </summary>
        public Int64 TaxAmount { get; set; }

        /// <summary>
        /// 游戏数据
        /// </summary>        
        public string GameData { get; set; }

        /// <summary>
        /// 参数1
        /// </summary>
        public Int64 Param1 { get; set; }

        /// <summary>
        /// 参数2
        /// </summary>
        public Int64 Param2 { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime GameTime { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}
