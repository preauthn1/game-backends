using FW.Core.Helper;
using game.Entity.Agent;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace game.AdminSystem.Controllers.Agent
{
    public class AgentReportLogController : BaseController
    {
        /// <summary>
        /// 代理举报列表
        /// </summary>
        /// <returns></returns>
        public ActionResult List(int? platformID, string name, DateTime? createBeginTime, DateTime? createEndTime, int p = 1)
        {

            var adminId = SiteService.GetAdminUserID();

            var list = SiteHelper.GetAPIResult<PagedList<AgentReportLog>>(string.Format("/Agent/GetAgentReportLogList?adminId={6}&platformID={5}&name={0}&createBeginTime={1}&createEndTime={2}&pageIndex={3}&pageSize={4}", name, createBeginTime, createEndTime, p, 20, platformID, adminId));

            return View(list);
        }

        /// <summary>
        /// 删除代理举报
        /// </summary>
        /// <returns></returns>
        public ActionResult Delete(int id)
        {
            var result = SiteHelper.GetAPIResult<APIResult>(string.Format("/Agent/DeleteAgentReportLog?reportLogId={0}", id));

            return Alert(result.Message, "/AgentReportLog/List?_r=" + new Random().NextDouble());
        }
    }
}