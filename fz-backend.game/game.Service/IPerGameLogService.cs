using FW.Core.Helper;
using game.Entity.PerLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Service
{
    /// <summary>
    /// 每局游戏记录接口
    /// </summary>
    public interface IPerGameLogService
    {
        /// <summary>
        /// 每局游戏记录查询
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="gameID"></param>
        /// <param name="serverID"></param>
        /// <param name="dealNo"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<DetailLog> GetDetailLog(int? userID, int? gameID, int? serverID, Int64? dealNo, DateTime? beginTime, DateTime? endTime, int pageIndex, int pageSize, string deskCode = "");

        /// <summary>
        /// 每局游戏记录详细
        /// </summary>
        /// <param name="logID"></param>
        /// <returns></returns>
        List<DetailLog> GetDetailLogByLogID(string logID, DateTime gametime);

        /// <summary>
        /// 每局游戏记录
        /// </summary>
        /// <param name="logID"></param>
        /// <returns></returns>
        PerGameLog GetPerGameLogByLogID(string logID, DateTime gametime);

        /// <summary>
        /// 根據交易ID取得牌局ID
        /// </summary>
        /// <param name="tradeID"></param>
        /// <returns></returns>
        List<string> GetPerGameLogByTradeID(string tradeID);

        List<DetailLog> GetGameLogByTradeID(long tradeID, int userID);


        PagedList<OnlineUser> GetOnlineUser(int operatorID, int? gameID, int? serverID, int pageIndex, int pageSize, int? sortName, int? sortLifting);

    }
}
