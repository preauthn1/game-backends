using System;
using System.Linq;
using System.Web.Mvc;
using Agent.Helper;
using Repository.Entity.agentclub;
using Service.Service.AgentClub;

namespace Agent.Areas.agentclub.Controllers
{
    public class ReportController : BaseController
    {
        private ReportService _service = new ReportService();

        // Post: Report/GetChangeLog
        [HttpPost]
        public ActionResult GetChangeLog(int? user_id, string loginName, DateTime? startDate, DateTime? endDate, string sys_Order, string sortDirection, int PageIndex = 1, int PageSize = 10)
        {
            var msg = string.Empty;
            var pageCount = 0;
            var result = _service.GetChangeLog(AdminID, user_id, loginName, startDate, endDate, sys_Order, sortDirection, ref pageCount, PageIndex, PageSize);
            var query = from pro in result
                        select new
                        {
                            pro.OperateName,
                            pro.UserID,
                            pro.TargetName,
                            pro.OldData,
                            pro.NewData,
                            ChangeType = pro.ChangeType.GetEnumDescription(),
                            pro.Memo,
                            CreateTime = pro.CreateTime.ToString("yyyy/MM/dd HH:mm:ss")
                        };

            return Json(new
            {
                pageCount,
                msg = msg,
                data = query,
                status = 200
            });
        }

        // Post: Report/GetLogonLog
        [HttpPost]
        public ActionResult GetLogonLog(int? user_id, string loginName, DateTime? startDate, DateTime? endDate)
        {
            var result = _service.GetLogonLog(AdminID, user_id, loginName, startDate, endDate);
            
            var query = from pro in result
                        select new
                        {
                            pro.LogID,
                            pro.UserID,
                            pro.LoginName,
                            pro.IPAddress,
                            CreateTime = pro.CreateTime.ToString("yyyy/MM/dd HH:mm:ss")
                        };

            return Json(new
            {
                data = query,
                status = 200
            });
        }

        // Post: Report/GetAgentProfitLog
        [HttpPost]
        public ActionResult GetAgentProfitLog(int? user_id, string loginName, int? platformID, int? areaID, DateTime? startDate, DateTime? endDate)
        {
            var msg = string.Empty;
            var result = _service.GetAgentProfitLog(AdminID, user_id, loginName, platformID, areaID, startDate, endDate);

            var query = from pro in result
                        select new
                        {
                            pro.YYYYMMDD,
                            pro.UserID,
                            pro.LoginName,
                            ClassName = pro.Class.GetEnumDescription(),
                            pro.PlatformName,
                            pro.AreaName,
                            SumFlows = pro.SumFlows.ToCash(),
                            SumIncome = pro.SumIncome.ToCash()
                        };

            return Json(new
            {
                msg = msg,
                data = query,
                status = 200
            });
        }

        // Post: Report/GetAgentProfitMonthlyLog
        [HttpPost]
        public ActionResult GetAgentProfitMonthlyLog(int? user_id, string loginName, int? platformID, int? areaID, DateTime? startDate, DateTime? endDate)
        {
            var msg = string.Empty;
            var result = _service.GetAgentProfitMonthlyLog(AdminID, user_id, loginName, platformID, areaID, startDate, endDate);

            var query = from pro in result
                        select new
                        {
                            pro.YYYYMM,
                            pro.UserID,
                            pro.LoginName,
                            ClassName = pro.Class.GetEnumDescription(),
                            pro.PlatformName,
                            pro.AreaName,
                            SumFlows = pro.SumFlows.ToCash(),
                            SumIncome = pro.SumIncome.ToCash()
                        };

            return Json(new
            {
                msg = msg,
                data = query,
                status = 200
            });
        }

        // Post: Report/GetAgentWithdrawLog
        [HttpPost]
        public ActionResult GetAgentWithdrawLog(int? user_id, string loginName, AgentClass? agentClass, DateTime? startDate, DateTime? endDate)
        {
            var msg = string.Empty;
            var result = _service.GetAgentWithdrawLog(AdminID, user_id, loginName, agentClass, startDate, endDate);

            var query = from pro in result
                        select new
                        {
                            pro.YYYYMMDD,
                            pro.UserID,
                            pro.LoginName,
                            ClassName = pro.Class.GetEnumDescription(),
                            pro.PlatformName,
                            pro.AreaName,
                            CurrentAmount = pro.CurrentAmount.ToCash(),
                            UpdateAmount = pro.UpdateAmount.ToCash(),
                            RemainAmount = pro.RemainAmount.ToCash(),
                            pro.Memo,
                            CreateTime = pro.CreateTime.Value.ToString("yyyy/MM/dd HH:mm")
                        };

            return Json(new
            {
                msg = msg,
                data = query,
                status = 200
            });
        }
    }
}