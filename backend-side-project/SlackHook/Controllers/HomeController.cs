using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using game.Infrastructure.Extensions;
using game.Infrastructure.Utility.ThirdParty;

namespace SlackHook.Controllers
{
    public class HomeController : ApiController
    {
        [HttpGet]
        [Route("")]
        public object HomePage()
        {
            return "Hello";
        }



    }
}
