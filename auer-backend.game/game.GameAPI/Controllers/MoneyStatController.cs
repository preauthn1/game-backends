using System;
using System.Collections.Generic;
using System.Web.Http;
using game.Entity.Money;
using game.Entity.Stat;
using game.Service;

namespace game.GameAPI.Controllers
{
    public class MoneyStatController : BaseAPIController
    {
        private IMoneyStatService _msServie;
        public MoneyStatController(IMoneyStatService msService)
        {
            _msServie = msService;
        }
        [HttpGet]
        public List<MoneySourceLog> GetMoneySourceStatLog(int operatorID, int? platformids, int? areaID, int? sourceTypeGroup = null, int? sourceTypeID = null, MoneySourceChangeType? changeType = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            return _msServie.GetMoneySourceStatLog(operatorID, platformids, areaID, sourceTypeGroup, sourceTypeID, changeType, beginTime, endTime);
        }

        [HttpGet]
        public List<SumMoneyLog> GetSumMoneyLog(int operatorID, DateTime? beginTime = null, DateTime? endTime = null)
        {
            return _msServie.GetSumMoneyLog(operatorID, beginTime, endTime);
        }

        [HttpGet]
        public List<UserMoneyReport> GetUserTopMoneyReport(int count, Int64? amount = null)
        {
            return _msServie.GetUserTopMoneyReport(amount, count);
        }

        [HttpGet]
        public List<SystemGainLog> GetSystemGainLog(int operatorID, DateTime? beginTime = null, DateTime? endTime = null)
        {
            return _msServie.GetSystemGainLog(operatorID, beginTime, endTime);
        }

        [HttpGet]
        public List<GameTaxLog> GetGameTaxLog(int operatorID, int? platformids, int? areaID, int? gameID = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            return _msServie.GetGameTaxLog(operatorID, platformids, areaID, gameID, beginTime, endTime);
        }

        /// <summary>
        /// 游戏房间税收统计
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [HttpGet]
        public List<GameServerTaxLog> GetGameServerTaxLog(int operatorID, int? platformids, int? areaID, int? gameID, int? serverID = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            return _msServie.GetGameServerTaxLog(operatorID, platformids, areaID, gameID, serverID, beginTime, endTime);
        }

        [HttpGet]
        public List<UserGameTotalRank> GetGameTotalRankByGameID(int gameID, int topN)
        {
            return _msServie.GetGameTotalRankByGameID(gameID, topN);
        }

        [HttpGet]
        public List<BankSourceLog> GetBankSourceStatLog(int operatorID, int? platformids, int? areaID, int? sourceTypeID = null, BankSourceChangeType? changeType = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            return _msServie.GetBankSourceStatLog(operatorID, platformids, areaID, sourceTypeID, changeType, beginTime, endTime);
        }
    }
}