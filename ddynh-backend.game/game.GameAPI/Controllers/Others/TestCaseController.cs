using FW.Core.Helper;
using game.Entity.Users;
using game.Infrastructure;
using game.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace game.GameAPI.Controllers
{
    public class TestCaseController : BaseAPIController
    {
        [HttpGet]
        public string TestException()
        {
            throw new Exception("Test");
        }
    }
}