using System;
using System.Collections.Generic;
using game.Entity.Money;
using game.Entity.Stat;

namespace game.Service
{
    /// <summary>
    /// 金币统计
    /// </summary>
    public interface IMoneyStatService
    {

        /// <summary>
        /// 获取金币流向统计记录
        /// </summary>
        /// <param name="sourceTypeID"></param>
        /// <param name="changeType"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<MoneySourceLog> GetMoneySourceStatLog(int operatorID, int? platformids, int? areaID, int? sourceTypeGroup, int? sourceTypeID, MoneySourceChangeType? changeType, DateTime? beginTime, DateTime? endTime);

        /// <summary>
        /// 获取游戏税收记录
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<GameTaxLog> GetGameTaxLog(int operatorID, int? platformids, int? areaID, int? gameID, DateTime? beginTime, DateTime? endTime);

        /// <summary>
        /// 获取游戏房间税收记录
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="serverID"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<GameServerTaxLog> GetGameServerTaxLog(int operatorID, int? platformids, int? areaID, int? gameID, int? serverID, DateTime? beginTime, DateTime? endTime);

        /// <summary>
        /// 获取总金币记录
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<SumMoneyLog> GetSumMoneyLog(int operatorID, DateTime? beginTime, DateTime? endTime);

        /// <summary>
        /// 按金币总量排行获取用户金币报表
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        List<UserMoneyReport> GetUserTopMoneyReport(Int64? amount, int count, int operatorID);

        List<UserGameTotalRank> GetGameTotalRankByGameID(int gameID, int topN);

        List<SystemGainLog> GetSystemGainLog(int operatorID, DateTime? beginTime, DateTime? endTime);

        /// <summary>
        /// 获取银行流向统计记录
        /// </summary>
        /// <param name="sourceTypeID"></param>
        /// <param name="changeType"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<BankSourceLog> GetBankSourceStatLog(int operatorID, int? platformids, int? areaID, int? sourceTypeID, BankSourceChangeType? changeType, DateTime? beginTime, DateTime? endTime);
    }
}
