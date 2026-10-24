using FW.Core.Helper;
using game.Entity.Warning;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace game.AdminSystem.Controllers.Warning
{
    public class WarningController : BaseController
    {
        // GET: Warning
        public ActionResult UserFocus(int? adminUserID, int? typeID, int p=1, int pageSize=20)
        {
            ViewBag.Type = SiteHelper.GetAPIResult<List<WarningType>>(string.Format("/Warning/GetWarningType"));
            return View(SiteHelper.GetAPIResult<PagedList<UserFocus>>(string.Format("/Warning/GetUserFocus?adminUserID={0}&typeID={1}&pageIndex={2}&pageSize={3}", adminUserID, typeID, p, pageSize)));
        }
        public ActionResult AddUserFocus()
        {
            ViewBag.Type = SiteHelper.GetAPIResult<List<WarningType>>(string.Format("/Warning/GetWarningType"));
            return View();
        }
        [HttpPost]
        public ActionResult AddUserFocus(int adminUserID, int typeID)
        {
           var result=SiteHelper.GetAPIResult<APIResult>(string.Format("/Warning/AddUserFocus?adminUserID={0}&typeID={1}",adminUserID,typeID));
           if (result.Result)
               return ConfirmReturnList("/Warning/UserFocus");
           return Alert(result.Message);
        }
        public ActionResult DelUserFocus(int adminUserID, int typeID)
        {
            var result=SiteHelper.GetAPIResult<APIResult>(string.Format("/Warning/RemoveUserFocus?adminUserID={0}&typeID={1}", adminUserID, typeID));
            if (result.Result)
                return Back();
            return Alert(result.Message);
        }
        /// <summary>
        /// 报警记录
        /// </summary>
        /// <param name="adminUserID"></param>
        /// <param name="typeID"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="p"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult WarningLog(int? adminUserID, int? typeID, DateTime? begin, DateTime? end, int p = 1, int pageSize = 20)
        {
            //移除报警
            SiteHelper.GetAPIResult(string.Format("/Warning/ReadedWarning?adminUserID={0}", SiteService.GetAdminUserID()));
            ViewBag.Type = SiteHelper.GetAPIResult<List<WarningType>>(string.Format("/Warning/GetWarningType"));
            if (adminUserID == null) adminUserID = SiteService.GetAdminUserID();
            if (adminUserID == -1) adminUserID = null;
            if (begin == null) begin = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd 00:00"));
            return View(SiteHelper.GetAPIResult<PagedList<WarningLog>>(string.Format("/Warning/GetWarningLog?adminUserID={0}&typeID={1}&begin={2}&end={3}&pageIndex={4}&pageSize={5}", adminUserID, typeID, begin, end, p, pageSize)));
        }
        /// <summary>
        /// 报警状态
        /// </summary>
        /// <returns></returns>
        public ActionResult WarningType()
        {
            return View(SiteHelper.GetAPIResult<List<WarningType>>(string.Format("/Warning/GetWarningType")));
        }
        public ActionResult EditWarningType(int id)
        {
            var type = SiteHelper.GetAPIResult<WarningType>(string.Format("/Warning/GetWarningTypeByID?typeID={0}", id));
            type.Setting = ToJSON(type.Setting);
            return View(type);
        }


        private string ToJSON(string setting) {
            string json = "[";
            foreach (Match config in Regex.Matches(setting, @"<Setting(?<setting>[^/]*)/>", RegexOptions.Compiled | RegexOptions.IgnoreCase))
            {
                json = json + "{";
                foreach (Match attr in Regex.Matches(config.Groups["setting"].ToString(), @"(?<key>\w*)=""(?<value>[^""]*)""", RegexOptions.Compiled | RegexOptions.IgnoreCase))
                {
                    json += string.Format(@"""{0}"":""{1}"",", attr.Groups["key"].ToString(), attr.Groups["value"].ToString());
                }
                if (json.EndsWith(",")) json = json.Substring(0, json.Length - 1);
                json = json + "},";
            };
            if (json.EndsWith(",")) json = json.Substring(0, json.Length - 1);
            return json + "]";
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditWarningType(WarningType warningType)
        {
            var resultModel = SiteHelper.GetAPIResult<WarningType>(string.Format("/Warning/GetWarningTypeByID?typeID={0}", warningType.Id));
            if (resultModel==null)
            {
                return Alert("操作失败！数据不存在！");
            }
            resultModel.Setting = warningType.Setting;
            resultModel.Status = warningType.Status;
            resultModel.TypeName = warningType.TypeName;

            var result = SiteHelper.PostAPIResult<WarningType>(string.Format("/Warning/ModifyWarningType"), resultModel);
            if (result.Result)
                return ConfirmReturnList("/Warning/WarningType");
            return Alert(result.Message);
        }
        [HttpGet]
        public ActionResult ExistWarning()
        {
            var result = SiteHelper.GetAPIResult<APIResult>(string.Format("/Warning/ExistWarning?adminUserID={0}", SiteService.GetAdminUserID())); 
                return Content(result.Result.ToString());
        }
    }
}