
using System;
using System.Collections.Generic;
using System.Web.Http;
using game.Infrastructure.Extensions;
using game.Infrastructure;
using game.Service.DAO;

namespace game.GameAPI.Controllers
{
    public class HomeController : BaseAPIController
    {
        
        [HttpGet]
        public object Index(string password = "")
        {
            string helloMsg = "Hello , game.GameAPI";
            if (!SystemHelper.IsLocalhost() && password != SystemConst.ApiPassword)
            {
                var para1 = new
                {
                    Msg = helloMsg,
                    ServerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    ComputerName = AppHelper.GetWindowsComputerUserName()
                };

                return para1;
            }

            var para = new
            {
                Msg = helloMsg,
                WebSiteName = System.Configuration.ConfigurationManager.AppSettings["WebSiteName"] ?? string.Empty,
                ServerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                IsGlobalApiLogEnable = SettingHelper.IsGlobalApiLogEnable,
                LastCompileTime = BinFileHelper.GetBinFileLastModifiedTimeStringFormat(),
                ComputerName = AppHelper.GetWindowsComputerUserName()
            };

            return para;
        }


    }
}