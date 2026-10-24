using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Job.API.ThirdPartyCommon.Controllers
{
    public class HomeController : ApiController
    {
        [HttpGet]
        [Route("")]
        public object Index(string password = "")
        {
            string helloMsg = $"Hello , {SettingHelper.WebSiteName}";           

            var para = new
            {
                Msg = helloMsg,
                WebSiteName = SettingHelper.WebSiteName,
                ServerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                IsGlobalApiLogEnable = SettingHelper.IsGlobalApiLogEnable,
                LastCompileTime = BinFileHelper.GetBinFileLastModifiedTimeStringFormat()
            };

            return para;
        }
    }
}
