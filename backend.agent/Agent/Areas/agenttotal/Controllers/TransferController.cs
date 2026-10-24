
using System;
using System.Linq;
using System.Web.Mvc;
using Agent.Helper;
using Repository.Entity;
using Repository.Models;
using Service.Service;

namespace Agent.Areas.agenttotal.Controllers
{
    public class TransferController : Controller
    {
        private AgentTotalService _service = new AgentTotalService();
        private TransferService _transferService = new TransferService();
        // GET: agenttotal/Transfer
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetTransferLog(deviceType? deviceType, transferType? transferType, int? user_agent_id, int? minPrice, int? maxPrice, DateTime? startDate, DateTime? endDate)
        {
            var transferLog = _service.GetTransferLog(deviceType, transferType, user_agent_id, minPrice, maxPrice, startDate, endDate);
            var query = from pro in transferLog
                        select new
                        {
                            pro.id,
                            pro.total_agent_id,
                            type = pro.type.GetEnumDescription(),
                            pro.user_agent_id,
                            pro.money,
                            pro.old_money,
                            pro.new_money,
                            created_at = pro.created_at.Value.ToString("yyyy/MM/dd HH:mm"),
                            device = pro.device.GetEnumDescription(),
                            pro.remark
                        };
            return Json(new
            {
                msg = "success",
                data = query,
                status = 200
            });
        }

        [HttpPost]
        public ActionResult transferMoney(int agentId, int turnInMoney)
        {
            var token = [REDACTED]"Authorization"];
            var userInfo = new Repository.Entity.agent_total();
            var IsAuthenticated = verification.ValidateToken(token, ref userInfo);

            var msg = string.Empty;
            var result = _transferService.transferMoney(userInfo.id, agentId, turnInMoney, ref msg);
            return Json(new
            {
                msg = msg,
                data = result,
                status = 200
            });
        }


        [HttpPost]
        public ActionResult AgentOrderBack(int order_id, int backMoney, int user_id, int agent_id)
        {
            var token = [REDACTED]"Authorization"];
            var userInfo = new Repository.Entity.agent_total();
            var IsAuthenticated = verification.ValidateToken(token, ref userInfo);

            var msg = string.Empty;
            var result = _transferService.AgentOrderBack(order_id, backMoney, user_id, agent_id, ref msg);
            return Json(new
            {
                msg = msg,
                data = new { },
                status = 200
            });
        }


        [HttpPost]
        public ActionResult GetRechargeLog()
        {
            var token = [REDACTED]"Authorization"];
            var userInfo = new Repository.Entity.agent_total();
            var IsAuthenticated = verification.ValidateToken(token, ref userInfo);

            var rechargeLog = _transferService.GetRechargeLog(userInfo.id);

            var query = from pro in rechargeLog
                        select new
                        {
                            pro.id,
                            pro.total_agent_id,
                            pro.username,
                            pro.num,
                            created_at = pro.created_at.ToString("yyyy/MM/dd HH:mm"),
                            pro.remark
                        };
            return Json(new
            {
                msg = "success",
                data = query,
                status = 200
            });
        }

        [HttpPost]
        public ActionResult GetAgentRechargeLog(int? agent_id, DateTime? startDate, DateTime? endDate)
        {

            var token = [REDACTED]"Authorization"];
            var userInfo = new Repository.Entity.agent_total();
            var IsAuthenticated = verification.ValidateToken(token, ref userInfo);

            var rechargeLog = _transferService.GetAgentRechargeLog(agent_id, startDate, endDate);

            var query = from pro in rechargeLog
                        select new
                        {
                            pro.id,
                            pro.agent_id,
                            pro.name,
                            pro.recharge,
                            created_at = pro.day.Value.ToString("yyyy/MM/dd")
                        };
            return Json(new
            {
                msg = "success",
                data = query,
                status = 200
            });
        }


        [HttpPost]
        public ActionResult GetAgentBackLog(int? id, int? agent_id, DateTime? startDate, DateTime? endDate, int? error_user_id)
        {
            var token = [REDACTED]"Authorization"];
            var userInfo = new Repository.Entity.agent_total();
            var IsAuthenticated = verification.ValidateToken(token, ref userInfo);

            var rechargeLog = _transferService.GetAgentBackLog(userInfo.id, id, agent_id, startDate, endDate, error_user_id);

            var query = from pro in rechargeLog
                        select new
                        {
                            pro.id,
                            status = pro.status.GetEnumDescription(),
                            type = pro.type.GetEnumDescription(),
                            pro.agent_id,
                            pro.money,
                            pro.agent_old_money,
                            pro.agent_new_money,
                            pro.error_user_id,
                            created_at = pro.created_at.Value.ToString("yyyy/MM/dd HH:mm"),
                            updated_at = pro.updated_at.Value.ToString("yyyy/MM/dd HH:mm")
                        };
            return Json(new
            {
                msg = "success",
                data = query,
                status = 200
            });
        }
    }
}