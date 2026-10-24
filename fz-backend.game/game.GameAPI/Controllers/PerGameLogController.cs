using System;
using System.Collections.Generic;
using System.Web.Http;
using FW.Core.Helper;
using game.Entity.PerLog;
using game.Service;

namespace game.GameAPI.Controllers
{
    public class PerGameLogController : BaseAPIController
    {
        private IPerGameLogService _service;

        public PerGameLogController(IPerGameLogService service)
        {
            _service = service;
        }

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
        [HttpGet]
        public PagedList<DetailLog> GetDetailLog(string deskCode, int? userID = null, int? gameID = null, int? serverID = null, Int64? dealNo = null, DateTime? beginTime = null, DateTime? endTime = null, int pageIndex = 1, int pageSize = 20)
        {

            return _service.GetDetailLog(userID, gameID, serverID, dealNo, beginTime, endTime, pageIndex, pageSize, deskCode);
        }

        [HttpGet]
        public List<DetailLog> GetDetailLogByLogID(string logID, DateTime gametime)
        {
            return _service.GetDetailLogByLogID(logID, gametime);
        }

        [HttpGet]
        public PerGameLog GetPerGameLogByLogID(string logID, DateTime gametime)
        {
            return _service.GetPerGameLogByLogID(logID, gametime);
        }

        [HttpGet]
        public List<string> GetPerGameLogByTradeID(string tradeID)
        {
            return _service.GetPerGameLogByTradeID(tradeID);
        }

        [HttpGet]
        public List<DetailLog> GetGameLogByTradeID(long tradeID, int userID)
        {
            return _service.GetGameLogByTradeID(tradeID, userID);
        }


        [HttpGet]
        public PagedList<OnlineUser> GetOnlineUser(int operatorID, int? serverID, int? gameID = null, int pageIndex = 1, int pageSize = 20, int? sortName = null, int? sortLifting = null)
        {
            return _service.GetOnlineUser(operatorID, gameID, serverID, pageIndex, pageSize, sortName, sortLifting);
        }
    }
}