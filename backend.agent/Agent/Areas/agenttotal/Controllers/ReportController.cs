using System;
using System.Linq;
using System.Web.Mvc;
using Agent.Helper;
using Repository.Entity;
using Repository.Models;
using Repository.ViewModel;
using Service.Service;

namespace Agent.Areas.agenttotal.Controllers
{
    public class ReportController : Controller
    {
        private ReportService _reportService = new ReportService();

        [HttpPost]
        public ActionResult GetReportDay(int? agent_id, int? minPrice, int? maxPrice, DateTime? startDate, DateTime? endDate)
        {
            var token = [REDACTED]"Authorization"];
            var userInfo = new Repository.Entity.agent_total();
            var IsAuthenticated = verification.ValidateToken(token, ref userInfo);

            var msg = string.Empty;
            var result = _reportService.GetReportDay(agent_id, minPrice, maxPrice, startDate, endDate);

            var query = from pro in result
                        select new
                        {
                            pro.agent_id,
                            pro.name,
                            pro.channel,
                            pro.remark,
                            pro.recharge,
                            pro.circulate,
                            pro.profit,
                            pro.sale,
                            created_at = pro.day.Value.ToString("yyyy/MM/dd")
                        };

            return Json(new
            {
                msg = msg,
                data = query,
                status = 200
            });
        }

        [HttpPost]
        public ActionResult GetAgentDetails(int? agent_id, int? user_id, int? agentMoneyType, int? minPrice, int? maxPrice, DateTime? startDate, DateTime? endDate)
        {
            var token = [REDACTED]"Authorization"];
            var userInfo = new Repository.Entity.agent_total();
            var IsAuthenticated = verification.ValidateToken(token, ref userInfo);

            var msg = string.Empty;
            var result = _reportService.GetAgentDetails(agent_id, user_id, agentMoneyType, minPrice, maxPrice, startDate, endDate);

            var query = from pro in result
                        select new
                        {
                            pro.agent_id,
                            type = pro.type.GetEnumDescription(),
                            pro.user_id,
                            pro.money,
                            pro.new_money,
                            created_at = pro.created_at.Value.ToString("yyyy/MM/dd"),
                            pro.ip,
                            pro.area,
                            pro.status,
                            pro.remark,
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