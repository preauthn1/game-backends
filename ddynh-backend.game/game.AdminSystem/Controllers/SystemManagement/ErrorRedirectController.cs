using System;
using System.Collections.Generic;
using System.Web.Mvc;
using game.Entity.AdminSys;
using game.Entity.Money;
using game.Infrastructure.Extensions;
using game.Entity.Stat;
using game.Entity.Area;
using System.Linq;
using game.Entity;
using FW.Core.Helper;

namespace game.AdminSystem.Controllers.SystemManagement
{
    public class ErrorRedirectController : BaseController
    {
        [NoAuthorizationAttribute]
        [HttpGet]
        public ActionResult GoToLogin()
        {
            var content = new ContentResult();
            content.Content = string.Format("<script type='text/javascript'>alert('权限不足，将导到登入页！');window.location.href='{0}';</script>", "/Admin/Default/Logon");
            return content;
        }

    }
}