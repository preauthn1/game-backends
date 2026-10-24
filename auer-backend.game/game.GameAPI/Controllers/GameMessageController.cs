using System.Collections.Generic;
using System.Web.Http;
using game.Entity.Game;
using game.Infrastructure;
using game.Service;

namespace game.GameAPI.Controllers
{
    public class GameMessageController : BaseAPIController
    {
        private IGameMessageService _gmServie;
        public GameMessageController(IGameMessageService gmService)
        {
            _gmServie = gmService;
        }

        [HttpGet]
        public List<GameMessage> GetGameMessage(int adminID, int? platformids)
        {
            return _gmServie.GetGameMessage(adminID, platformids);
        }

        [HttpPost]
        public APIResult AddGameMessage(GameMessage msg, int adminID)
        {
            string message = "";
            bool result = _gmServie.AddGameMessage(msg, adminID, ref message);
            return new APIResult(result, message, msg.Id);
        }

        [HttpGet]
        public APIResult DelGameMessage(int msgID)
        {
            string message = "";
            bool result = _gmServie.DelGameMessage(msgID, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public GameMessage GetGameMessageByID(int msgID)
        {
            return _gmServie.GetGameMessageByID(msgID);
        }

        [HttpPost]
        public APIResult ModifyGameMessage(GameMessage msg, int adminID)
        {
            string message = "";
            bool result = _gmServie.ModifyGameMessage(msg, adminID, ref message);
            return new APIResult(result, message);
        }

    }
}