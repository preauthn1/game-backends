using FW.Core.Helper;
using game.Entity;
using game.Entity.Event;
using game.Entity.Game;
using game.Entity.Users;
using game.Infrastructure;
using game.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using game.Infrastructure.Extensions;
using game.Entity.ViewModels;
using FW.Core.Engine;
using System.Web.Http.Description;

namespace game.GamePlayAPI.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class TestCaseController : BaseController
    {

        public IGameLaunchService GameLaunchService { get; set; }
        


        [Route("test/exception")]
        [HttpPost , HttpGet]
        public object TestException()
        {
            throw new Exception("測試中");
        }


        [Route("test/exception2")]
        [HttpPost, HttpGet]
        public object TestTerproException()
        {
            throw new TerproException(ApiErrorCode.NotFoundData);
        }



        [Route("game/test-debug")]
        [HttpPost]
        public object TestDebug([FromBody]GameLaunchRequestViewModel model)
        {
            var result = GameLaunchService.TestCaseTestCase(model);
            return result;
        }


        [Route("game/test-debug2")]
        [HttpPost]
        public object TestDebug222([FromBody]GameLaunchRequestViewModel model)
        {           
            return "XXX";
        }


    }
}