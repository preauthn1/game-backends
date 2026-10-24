using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

using game.Service;
using game.Entity.Stat;
using game.Infrastructure;
using FW.Core.Helper;
using game.Entity.Money;

namespace game.GameAPI.Controllers
{
    public class RobotOnLineStatController : BaseAPIController
    {
        private IRobotOnLineStatService _onsServie;
        public RobotOnLineStatController(IRobotOnLineStatService onsService)
        {
            _onsServie = onsService;
        }


        [HttpGet]
        public List<ServerOnLineRobotLog> GetServerOnLineRobotLog(int? gameID = null, int? serverID = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            return _onsServie.GetServerOnLineRobotLog(gameID, serverID, beginTime, endTime);
        }

        [HttpGet]
        public List<GameOnLineRobotLog> GetGameOnLineRobotLog(int? gameID = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            return _onsServie.GetGameOnLineRobotLog(gameID, beginTime, endTime);
        }

        [HttpGet]
        public List<TotalGameOnLineRobotLog> GetTotalGameOnLineRobotLog(DateTime? beginTime = null, DateTime? endTime = null)
        {
            return _onsServie.GetTotalGameOnLineRobotLog(beginTime, endTime);
        }
    }
}