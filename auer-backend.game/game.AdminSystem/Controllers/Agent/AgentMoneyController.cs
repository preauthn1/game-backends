using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FW.Core.Helper;
using game.AdminSystem.Filters;
using game.Entity.Agent;
using game.Entity.Stat;
using game.Entity.Users;
using game.Infrastructure;

namespace game.AdminSystem.Controllers.Agent
{
    public class AgentMoneyController : BaseController
    {
        /// <summary>
        /// 转账
        /// </summary>
        /// <returns></returns>

        public ActionResult Transfer()
        {
            return View();
        }

        /// <summary>
        /// 取得总代理资讯
        /// </summary>
        /// <returns></returns>

        public ActionResult GetAgentInfoById(int agentID)
        {
            var operatorID = CurrentUser.UserID;
            var result = SiteHelper.GetAPIResult<AgentTotal>($"/Agent/GetAgentInfoById?operatorID={operatorID}&agentID={agentID}");
            return Json(new AjaxResult()
            {
                success = result == null ? false : true,
                data = result
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 确认转账
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        public ActionResult Transfer(int agentID, [ModelBinder(typeof(MoneyModeBinder))]long amount)
        {
            var UserID = CurrentUser.UserID;
            var LoginName = CurrentUser.LoginName;
            var result = SiteHelper.GetAPIResult<APIResult>(string.Format("/Agent/AgentTotalTransfer?agentID={0}&amount={1}&UserID={2}&LoginName={3}", agentID, amount, UserID, LoginName));
            return Alert(result.Message);
        }

        /// <summary>
        /// 当前登录代理交易记录列表
        /// </summary>
        /// <returns></returns>
        [IsAgent]
        public ActionResult AgentTransferLog(int? sourceID, DateTime? createBeginTime, DateTime? createEndTime, int p = 1)
        {
            var sourceList = SiteHelper.GetAPIResult<List<AgentTransferType>>("/Agent/GetAgentTransferTypeList");
            ViewBag.SourceList = sourceList;

            var list = SiteHelper.GetAPIResult<PagedList<TransferLog>>(string.Format("/Agent/GetTransferLogList?agentID={0}&sourceID={1}&createBeginTime={2}&createEndTime={3}&pageIndex={4}&pageSize={5}", SiteService.GetAgentID(), sourceID, createBeginTime, createEndTime, p, 20));
            return View(list);
        }

        /// <summary>
        /// 当前代理交易记录日报表
        /// </summary>
        /// <returns></returns>
        [IsAgent]
        public ActionResult AgentStatTransferLogDay(int? sourceID, DateTime? createBeginTime, DateTime? createEndTime, int p = 1)
        {
            var sourceList = SiteHelper.GetAPIResult<List<AgentTransferType>>("/Agent/GetAgentTransferTypeList");
            ViewBag.SourceList = sourceList;

            if (createBeginTime == null) createBeginTime = DateTime.Now.AddDays(-7);

            var list = SiteHelper.GetAPIResult<PagedList<TransferSourceLog>>(string.Format("/Agent/GetTransferSourceLogList?agentID={0}&sourceID={1}&createBeginTime={2}&createEndTime={3}&pageIndex={4}&pageSize={5}", SiteService.GetAgentID(), sourceID, createBeginTime, createEndTime, p, 20));
            return View(list);
        }

        /// <summary>
        /// 根据id获取玩家信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUserInfoById(string userid)
        {
            var userinfo = new UserInfo();
            try
            {
                userinfo = SiteHelper.GetAPIResult<UserInfo>(string.Format("/User/GetUserByID?userID={0}", userid));
                return Json(new AjaxResult()
                {
                    success = true,
                    data = userinfo.NickName
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new AjaxResult()
                {
                    success = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 所有代理交易记录列表
        /// </summary>
        /// <returns></returns>
        public ActionResult TransferLog(int? platformID, int? areaID, int? sourceID, DateTime? createBeginTime, DateTime? createEndTime, string name = "", int p = 1)
        {
            var sourceList = SiteHelper.GetAPIResult<List<AgentTransferType>>("/Agent/GetAgentTransferTypeList");
            ViewBag.SourceList = sourceList;

            var list = SiteHelper.GetAPIResult<PagedList<TransferLog>>(string.Format("/Agent/GetTransferLogList?platformID={0}&areaID={1}&name={2}&sourceID={3}&createBeginTime={4}&createEndTime={5}&pageIndex={6}&pageSize={7}", platformID, areaID, name, sourceID, createBeginTime, createEndTime, p, 20));
            return View(list);
        }

        /// <summary>
        /// 所有代理交易记录日报表
        /// </summary>
        /// <returns></returns>
        public ActionResult StatTransferLogDay(int? platformID, int? areaID, string name, int? sourceID, DateTime? statBeginTime, DateTime? statEndTime, int p = 1)
        {
            var sourceList = SiteHelper.GetAPIResult<List<AgentTransferType>>("/Agent/GetAgentTransferTypeList");
            ViewBag.SourceList = sourceList;

            if (statBeginTime == null) statBeginTime = DateTime.Now.AddDays(-7);

            var list = SiteHelper.GetAPIResult<PagedList<TransferSourceLog>>(string.Format("/Agent/GetTransferSourceLogList?platformID={0}&areaID={1}&name={2}&sourceID={3}&statBeginTime={4}&statEndTime={5}&pageIndex={6}&pageSize={7}", platformID, areaID, name, sourceID, statBeginTime, statEndTime, p, 20));
            return View(list);
        }

        /// <summary>
        /// 查詢總代理轉帳紀錄
        /// </summary>
        /// <returns></returns>
        public ActionResult TransferInfo()
        {
            return View();
        }

        /// <summary>
        /// 查詢總代理轉帳紀錄
        /// </summary>
        /// <param name="agentid"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="draw"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetTransferInfo(int? agentid, DateTime? beginTime, DateTime? endTime, int? draw, int? start, int length)
        {
            var _pageIndex = Math.Ceiling((decimal)start.Value / length) + 1;
            var operatorID = CurrentUser.UserID;

            var _api = SiteHelper.GetAPIResult<PagedList<AgentTransferInfo>>($"/Agent/GetTransferInfo?operatorID={operatorID}&agentid={agentid}&beginTime={beginTime}"
                + $"&endTime={endTime}&pageSize={length}&pageIndex={_pageIndex}");

            var _query = _api.PageData.Select(x => new
            {
                Created_at = x.Created_at.ToString("yyyy/MM/dd HH:mm:ss"),
                x.Money,
                x.New_Money,
                x.Old_Money,
                x.Remark,
                x.Total_Agent_Id,
                x.Total_Agent_Name,
                x.Name,
                x.Type
            });

            return Json(new
            {
                draw = draw,
                recordsTotal = _api.TotalCount,
                recordsFiltered = _api.TotalCount,
                success = true,
                data = _query
            });
        }
    }
}