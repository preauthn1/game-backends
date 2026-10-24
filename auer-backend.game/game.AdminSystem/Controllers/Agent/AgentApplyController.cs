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
    public class AgentApplyController : BaseController
    {
        /// <summary>
        /// 代理申请列表
        /// </summary>
        /// <returns></returns>
        public ActionResult List(int? platformID, string name, string tel, string qq, DateTime? createBeginTime, DateTime? createEndTime, int p = 1)
        {
            var list = SiteHelper.GetAPIResult<PagedList<AgentApplyInfo>>(string.Format("/Agent/GetAgentApplyList?platformID={7}&name={0}&tel={1}&qq={2}&createBeginTime={3}&createEndTime={4}&pageIndex={5}&pageSize={6}", name, tel, qq, createBeginTime, createEndTime, p, 20, platformID));

            return View(list);
        }

        /// <summary>
        /// 删除代理申请
        /// </summary>
        /// <returns></returns>
        public ActionResult Delete(int id)
        {
            var result = SiteHelper.GetAPIResult<APIResult>(string.Format("/Agent/DeleteAgentApply?applyId={0}", id));

            return Alert(result.Message, "/AgentApply/List?_r=" + new Random().NextDouble());
        }
    }
}