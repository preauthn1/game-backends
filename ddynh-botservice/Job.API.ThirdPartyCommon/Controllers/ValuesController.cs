using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid;
using example.invalid.Http;
using System.Web.Http;

namespace Job.API.ThirdPartyCommon.Controllers
{
    public class FlowAmountCalculateController : ApiController
    {

        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

       
    }
}
