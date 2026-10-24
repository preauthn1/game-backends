using game.Infrastructure.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid;
using example.invalid.Http;
using System.Web.Http;
using game.Infrastructure.Extensions;
using gitlab_hook.Models.MergeRequest;
using game.Infrastructure.Utility;
using gitlab_hook.Models;

namespace gitlab_hook.Controllers
{

    /// <summary>
    /// 透過gitlab自動開啟MR的API
    /// </summary>
    public partial class GitAutoCreateMRController : ApiController
    {

        [HttpGet, HttpPost]
        [Route("test/testMR")]
        public string TestCloseMR(string type = "")
        {
            CloseMR((int)GitlabRepositoryEnum.DDYNH_Using, 13);
            return "OK";
        }


    }



}
