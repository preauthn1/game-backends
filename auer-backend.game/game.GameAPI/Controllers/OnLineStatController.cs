using System;
using System.Collections.Generic;
using System.Web.Http;
using game.Entity.Stat;
using game.Service;

namespace game.GameAPI.Controllers
{
    public class OnLineStatController : BaseAPIController
    {
        private IOnLineStatService _onsServie;
        public OnLineStatController(IOnLineStatService onsService)
        {
            _onsServie = onsService;
        }


        [HttpGet]
        public List<ServerOnLineUserLog> GetServerOnLineUserLog(int operatorID, int platformID, int? gameID = null, int? serverID = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            return _onsServie.GetServerOnLineUserLog(operatorID, platformID, gameID, serverID, beginTime, endTime);
        }

        [HttpGet]
        public List<DayMaxServerOnLineUserLog> GetMaxServerOnLineUserLog(int operatorID, int platformID, int? gameID = null, int? serverID = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            return _onsServie.GetMaxServerOnLineUserLog(operatorID, platformID, gameID, serverID, beginTime, endTime);
        }

        [HttpGet]
        public List<GameOnLineUserLog> GetGameOnLineUserLog(int operatorID, int platformID, int? gameID = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            return _onsServie.GetGameOnLineUserLog(operatorID, platformID, gameID, beginTime, endTime);
        }

        [HttpGet]
        public List<DayMaxGameOnLineUserLog> GetMaxGameOnLineUserLog(int operatorID, int platformID, int? gameID = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            return _onsServie.GetMaxGameOnLineUserLog(operatorID, platformID, gameID, beginTime, endTime);
        }

        [HttpGet]
        public List<HallOnLineUserLog> GetHallOnLineUserLog(int operatorID, int platformID, DateTime? beginTime = null, DateTime? endTime = null)
        {
            return _onsServie.GetHallOnLineUserLog(operatorID, platformID, beginTime, endTime);
        }

        [HttpGet]
        public List<DayMaxHallOnLineUserLog> GetMaxHallOnLineUserLog(int platformID, DateTime? beginTime = null, DateTime? endTime = null)
        {
            return _onsServie.GetMaxHallOnLineUserLog(platformID, beginTime, endTime);
        }
        [HttpGet]
        public List<TotalGameOnLineLog> GetTotalGameOnLineLog(int platformID, int areaID, DateTime? beginTime = null, DateTime? endTime = null)
        {
            return _onsServie.GetTotalGameOnLineLog(platformID, areaID, beginTime, endTime);
        }
        [HttpGet]
        public List<DayMaxTotalGameOnLineLog> GetDayMaxTotalGameOnLineLog(int platformID, int areaID, DateTime? beginTime, DateTime? endTime)
        {
            return _onsServie.GetDayMaxTotalGameOnLineLog(platformID, areaID, beginTime, endTime);
        }

        [HttpGet]
        public List<GameOnlineChatData> GetGameOnlineChatData(int platformID, int? gameID, int dataType, DateTime? beginTime, DateTime? endTime)
        {
            return _onsServie.GetGameOnlineChatData(platformID, gameID, dataType, beginTime, endTime);
        }
        [HttpGet]
        public List<TotalGameOnLineLogArea> GetTotalGameOnLineLogArea(int areaID, DateTime? beginTime = null, DateTime? endTime = null)
        {
            return _onsServie.GetTotalGameOnLineLogArea(areaID, beginTime, endTime);
        }
    }
}