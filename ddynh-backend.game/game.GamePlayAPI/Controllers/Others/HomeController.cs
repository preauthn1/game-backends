using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace game.GamePlayAPI.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : ApiController
    {

        [HttpGet]
        [Route("")]
        public object Index(string password = "")
        {
            string helloMsg = $"Hello , {SettingHelper.WebSiteName}";
            if (!SystemHelper.IsLocalhost() && password != SystemConst.ApiPassword)
            {
                var para1 = new
                {
                    Msg = helloMsg,
                    ServerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                };

                return para1;
            }

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
