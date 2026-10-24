using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Stat
{
    /// <summary>
    /// 机器人在线人数记录
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class ServerOnLineRobotLog : BaseEntity
    {
         /// <summary>
         /// 房间ID
         /// </summary>
         public int ServerID
         {
             get;
             set;
         }

         /// <summary>
         /// 总在线人数
         /// </summary>
         public int TotalCount
         {
             get;
             set;
         }

         /// <summary>
         /// 统计时间
         /// </summary>
         public DateTime StatTime
         {
             get;
             set;
         }

         public virtual GameServer GameServer
         {
             get;
             set;
         }

    }
}
