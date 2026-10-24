using System;
using System.Collections.Generic;
using game.Entity.Stat;

namespace game.Service
{
    public interface IOnLineStatService
    {
        /// <summary>
        /// 获取房间在线人数记录
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="serverID"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<ServerOnLineUserLog> GetServerOnLineUserLog(int operatorID, int platformID, int? gameID, int? serverID, DateTime? beginTime, DateTime? endTime);

        /// <summary>
        /// 获取房间最高在线人数记录
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="serverID"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<DayMaxServerOnLineUserLog> GetMaxServerOnLineUserLog(int operatorID, int platformID, int? gameID, int? serverID, DateTime? beginTime, DateTime? endTime);

        /// <summary>
        /// 获取游戏在线人数记录
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<GameOnLineUserLog> GetGameOnLineUserLog(int operatorID, int platformID, int? gameID, DateTime? beginTime, DateTime? endTime);

        /// <summary>
        /// 获取游戏最高在线记录
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<DayMaxGameOnLineUserLog> GetMaxGameOnLineUserLog(int operatorID, int platformID, int? gameID, DateTime? beginTime, DateTime? endTime);

        /// <summary>
        /// 获取大厅在线人数记录
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<HallOnLineUserLog> GetHallOnLineUserLog(int operatorID, int platformID, DateTime? beginTime, DateTime? endTime);

        /// <summary>
        /// 获取大厅最高在线人数记录
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<DayMaxHallOnLineUserLog> GetMaxHallOnLineUserLog(int platformID, DateTime? beginTime, DateTime? endTime, int? AdminID);
        /// <summary>
        /// 获取总游戏在线人数记录
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<TotalGameOnLineLog> GetTotalGameOnLineLog(int platformID, int areaID, DateTime? beginTime, DateTime? endTime, int? AdminID);

        #region 每日峰值查询
        /// <summary>
        /// 获取每日在线人数峰值
        /// </summary>
        /// <returns></returns>
        List<DayMaxTotalGameOnLineLog> GetDayMaxTotalGameOnLineLog(int platformID, int areaID, DateTime? beginTime, DateTime? endTime, int? AdminID);
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="dataType"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<GameOnlineChatData> GetGameOnlineChatData(int platformID, int? gameID, int dataType, DateTime? beginTime, DateTime? endTime);

        /// <summary>
        /// 渠道总游戏在线人数记录
        /// </summary>
        /// <param name="areaID"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<TotalGameOnLineLogArea> GetTotalGameOnLineLogArea(int areaID, DateTime? beginTime, DateTime? endTime);
    }
}
