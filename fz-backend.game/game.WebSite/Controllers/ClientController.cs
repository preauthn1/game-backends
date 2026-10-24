using game.Infrastructure;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace game.WebSite.Controllers
{
    public class ClientController : Controller
    {
        //
        // GET: /Client/
        public ActionResult GetIP()
        {            
            return Content(SiteHelper.GetIP()); //SystemHelper.UserIPAddress
        }

        public ActionResult RegHD(string HDCode, string HDInfo)
        {
            string userip = SiteHelper.GetIP();
            SiteHelper.GetAPIResult<string>(string.Format("/User/AddPreRegHD?ip={0}&hdcode={1}&hdInfo={2}", userip, HDCode, HDInfo));
            return Content("1");
        }

        //public ActionResult RegIP(int AreaID, string HDCode) 
        //{
        //    //取得userip
        //    string userip = SiteHelper.GetIP();

        //    //紀錄欲開帳號的使用者IP
        //    SiteHelper.GetAPIResult<string>(string.Format("/User/AddPreRegUserIP?areaid={0}&hdcode={1}&userip={2}", AreaID, HDCode, userip));
        //    return Content(userip);
        //}


        //public ActionResult MyIP()
        //{
        //    var r = HttpContext.Request;
        //    var REMOTE_ADDR = r.ServerVariables["REMOTE_ADDR"];
        //    var HTTP_X_FORWARDED_FOR = r.ServerVariables["HTTP_X_FORWARDED_FOR"];
        //    string ipinfo = string.Format("REMOTE_ADDR={0}<BR>HTTP_X_FORWARDED_FOR={1}", REMOTE_ADDR, HTTP_X_FORWARDED_FOR);

        //    string myIP = SiteHelper.GetIP();

        //    string header = "";
        //    foreach (string var in Request.ServerVariables)
        //    {
        //        header += var + "=" + Request[var] + "<p>";
        //    }

        //    return Content(string.Format("[My IP]<br>{0}<p>[IP Data]<br>{1}<p>[Header]<br>{2}", myIP, ipinfo, header));
        //}


        public string ContactService(int platformID, int areaID, int userID, string Message, int questionType)
        {
            if (platformID <= 0 || areaID <= 0 || questionType <= 0 || Message.Trim().Length == 0 || userID < 0)
                return "Invalid_Params";

            string userIP = SiteHelper.GetIP();
            var data = $"platformID={platformID}&areaID={areaID}&userID={userID}&message={Message}&questionType={questionType}&userip={userIP}";
            string jResult = SiteHelper.HttpPost(SiteHelper.APIBaseUrl + "/Customer/ContactService", data);
            dynamic obj = JObject.Parse(jResult);
            return obj.message;
        }

    }
}