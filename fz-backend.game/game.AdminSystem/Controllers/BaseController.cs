using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace game.AdminSystem.Controllers
{
    public class BaseController : FW.Core.Web.Mvc.BaseController
    {

        /// <summary>
        /// 根据ID/登录名称/昵称进行查询
        /// </summary>
        /// <returns></returns>
        public string GetFindData()
        {
            string userIDType = Request["userIDType"];
            string TextIdOrName = Request["TextIdOrName"];
            if (!string.IsNullOrWhiteSpace(userIDType) && !string.IsNullOrWhiteSpace(TextIdOrName))
                return userIDType + "=" + TextIdOrName;
            else
                return string.Empty;
        }
    }
}