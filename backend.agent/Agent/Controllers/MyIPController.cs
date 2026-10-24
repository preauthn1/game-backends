
using System.Web.Mvc;

namespace Agent.Controllers
{
    public class MyIPController : Controller
    {
        // GET: MyIP
        public ActionResult Index()
        {
            var r = HttpContext.Request;
            var REMOTE_ADDR = r.ServerVariables["REMOTE_ADDR"];
            var HTTP_X_FORWARDED_FOR = r.ServerVariables["HTTP_X_FORWARDED_FOR"];
            var HTTP_ALI_CDN_REAL_IP = r.ServerVariables["HTTP_ALI_CDN_REAL_IP"];
            string ipinfo = string.Format("REMOTE_ADDR={0}<BR>HTTP_X_FORWARDED_FOR={1}<BR>HTTP_ALI_CDN_REAL_IP={2}", REMOTE_ADDR, HTTP_X_FORWARDED_FOR, HTTP_ALI_CDN_REAL_IP);


            string header = string.Empty;
            foreach (string var in Request.ServerVariables)
            {
                header += var + "=" + Request[var] + "<p>";
            }

            return Content(string.Format("[IP Data]<br>{0}<p>[Header]<br>{1}", ipinfo, header));
        }
    }
}