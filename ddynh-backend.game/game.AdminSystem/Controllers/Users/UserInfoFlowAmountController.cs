using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Web.Mvc;
using DataTables.Mvc;
using FW.Core.Helper;
using FW.Core.Helper.Enum;
using game.Entity;
using game.Entity.Game;
using game.Entity.Money;
using game.Entity.Pay;
using game.Entity.PerLog;
using game.Entity.Users;
using game.Infrastructure;
using Newtonsoft.Json;
using NPOI.XSSF.UserModel;

namespace game.AdminSystem.Controllers.Users
{
  
    public class UserInfoFlowAmountController : BaseController
    {

        [HttpPost]
        [NoAuthorization]
        public ActionResult DeleteFlowAmountLimit(int id , string memo)
        {
            string url = "User/UpdateFlowAmountLimitLogStatus";

            var postModel = new CommonUpdateStatusRequestModel
            {
                TargetId = id,
                Memo = memo,
                UpdateBy = GetCurrentUserName(),
                TargetStatus = (int)FlowLimitStatus.Delete
            };
            
            var apiResult= SiteHelper.PutGameApi<CRUDResultModel>(url , postModel);
            return Json(apiResult.ResultObject, JsonRequestBehavior.AllowGet);
        }
        
    }
}