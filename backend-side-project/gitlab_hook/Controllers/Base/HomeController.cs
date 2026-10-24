using System;
using System.Web.Http;
using System.Linq;

namespace gitlab_hook.Controllers
{
    public class HomeController : ApiController
    {

        [HttpGet]
        [Route("")]
        public string Home()
        {
            long qqqq = 270;

            string text = qqqq.ToString("f2");

            var finalValue = decimal.Parse(text);





            return "hello , gitlab";
        }


        [HttpGet, HttpPost]
        [Route("warmup")]
        public string WarmUpWebsite()
        {
            return "warm";
        }

    }
}
