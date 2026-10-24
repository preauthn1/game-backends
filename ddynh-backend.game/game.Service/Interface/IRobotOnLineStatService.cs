using game.Entity.Stat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Service
{
    public interface IRobotOnLineStatService
    {
        /// <summary>
        /// 获取房间在线人数记录
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="serverID"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<ServerOnLineRobotLog> GetServerOnLineRobotLog(int? gameID, int? serverID, DateTime? beginTime, DateTime? endTime);

        /// <summary>
        /// 获取游戏在线人数记录
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<GameOnLineRobotLog> GetGameOnLineRobotLog(int? gameID, DateTime? beginTime, DateTime? endTime);
        /// <summary>
        /// 获取总游戏在线人数记录
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<TotalGameOnLineRobotLog> GetTotalGameOnLineRobotLog(DateTime? beginTime, DateTime? endTime);
    }
}
