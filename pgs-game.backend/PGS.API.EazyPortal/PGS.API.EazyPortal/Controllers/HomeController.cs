using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace target.API.EazyPortal.Controllers
{
    [ApiController]
    public class HomeController : ControllerBase
    {      
        [Route("")]
        public object HomePage()
        {
            string myString = null;
            return new 
            {
                code = 0,
                message = myString,
                data = new 
                {
                    environmentName = "production",
                    webSiteName = "target.API.Public.Production",
                }
            };
        }
    }
}
