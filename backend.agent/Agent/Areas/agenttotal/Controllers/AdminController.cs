
using System.Linq;
using System.Security.Cryptography;
using System.Web.Mvc;
using Agent.Helper;
using Repository.Entity;
using Repository.Models;
using Service.Service;

namespace Agent.Areas.agenttotal.Controllers
{
    public class AdminController : Controller
    {
        private AgentTotalService _service = new AgentTotalService();

        // GET: agenttotal/Admin
        public ActionResult Index()
        {
            return View();
        }

        // Post: agenttotal/Admin/login
        [HttpPost]
        public ActionResult login(string username, string password)
        {
            var user = _service.GetAgentTotal(username);
            if (user == null)
            {
                return Json(new
                {
                    msg = "帐号密码错误",
                    status = 401
                });
            }

            MD5 md5 = new MD5CryptoServiceProvider();
            var source = password + user.encrypt;

            using (MD5 md5Hash = MD5.Create())
            {
                string hash = verification.GetMd5Hash(md5Hash, source);
                if (!user.password.Equals(hash))
                {
                    return Json(new
                    {
                        msg = "帐号密码错误",
                        status = 401
                    });
                }
            }

            var userID = user.id;
            var token = [REDACTED], 60);
            return Json(new
            {
                msg = "登入成功",
                token,
                status = 200
            });
        }

        // Get: agenttotal/Admin/ValidateToken
        [HttpGet]
        public ActionResult validateToken()
        {
            var token = [REDACTED]"Authorization"];
            var userInfo = new Repository.Entity.agent_total();

            var IsAuthenticated = verification.ValidateToken(token, ref userInfo);

            return Json(new
            {
                status = 200,
                IsAuthenticated
            }, JsonRequestBehavior.AllowGet);
        }


        // Post: agenttotal/Admin/management
        [HttpPost]
        public ActionResult GetAgent(status? status, transfer? transfer, int? agent_id, string agent_name, int? minPrice = 0, int? maxPrice = 0)
        {
            var token = [REDACTED]"Authorization"];
            var userInfo = new Repository.Entity.agent_total();
            var IsAuthenticated = verification.ValidateToken(token, ref userInfo);

            var agent = _service.GetAgent(userInfo.id, status, transfer, agent_id, agent_name, minPrice, maxPrice);
            var query = from pro in agent
                        select new
                        {
                            pro.id,
                            status = pro.status.GetEnumDescription(),
                            pro.telphone,
                            transferName = pro.transfer.GetEnumDescription(),
                            transfer = pro.transfer,
                            pro.name,
                            pro.channel,
                            pro.player_id,
                            pro.level,
                            pro.min_money,
                            pro.money,
                            clock = pro.clock.GetEnumDescription(),
                            pro.total_recharge,
                            pro.total_turn_out,
                            pro.total_turn_in,
                            pro.weixin,
                            pro.qq,
                            pro.alipay,
                            created_at = pro.created_at.Value.ToString("yyyy/MM/dd HH:mm"),
                            pro.remark,
                            pro.auto_reply
                        };

            return Json(new
            {
                status = 200,
                data = query,
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult CreateAgent(string name, int player_id, int min_money, int? qq, string weixin
        , string alipay, long telphone, string auto_reply, string remark, transfer transfer, short level, string mailContent)
        {
            var token = [REDACTED]"Authorization"];
            var userInfo = new Repository.Entity.agent_total();
            var IsAuthenticated = verification.ValidateToken(token, ref userInfo);
            var msg = string.Empty;
            var result = _service.CreateAgent(userInfo.id, name, player_id, min_money
                , qq ?? 0, weixin ?? string.Empty
                , alipay ?? string.Empty, telphone
                , auto_reply ?? string.Empty
                , remark ?? string.Empty
                , transfer, level, mailContent, ref msg);

            return Json(new
            {
                msg = msg,
                data = result,
                status = 200
            });
        }


        [HttpPost]
        public ActionResult SaveAgent(int id, string name, int player_id, int min_money, int qq, string weixin
            , string alipay, long telphone, string auto_reply, string remark, transfer transfer, short level)
        {
            var token = [REDACTED]"Authorization"];
            var userInfo = new Repository.Entity.agent_total();
            var IsAuthenticated = verification.ValidateToken(token, ref userInfo);
            var msg = string.Empty;
            var result = _service.SaveAgent(id, name, player_id, min_money, qq, weixin, alipay, telphone, auto_reply, remark, transfer, level, ref msg);

            return Json(new
            {
                msg = msg,
                data = result,
                status = 200
            });
        }

        [HttpPost]
        public ActionResult GetLoginLog()
        {
            var token = [REDACTED]"Authorization"];
            var userInfo = new Repository.Entity.agent_total();
            var IsAuthenticated = verification.ValidateToken(token, ref userInfo);

            var rechargeLog = _service.GetLoginLog(userInfo.id);

            var query = from pro in rechargeLog
                        select new
                        {
                            pro.id,
                            pro.ip,
                            pro.area,
                            pro.remark,
                            created_at = pro.created_at.Value.ToString("yyyy/MM/dd HH:mm")
                        };
            return Json(new
            {
                msg = "success",
                data = query,
                status = 200
            });
        }

        [HttpPost]
        public ActionResult GetAgentLoginLog(int? agent_id, string ip)
        {
            var token = [REDACTED]"Authorization"];
            var userInfo = new Repository.Entity.agent_total();
            var IsAuthenticated = verification.ValidateToken(token, ref userInfo);

            var rechargeLog = _service.GetAgentLoginLog(userInfo.id, agent_id, ip);

            var query = from pro in rechargeLog
                        select new
                        {
                            pro.agent_id,
                            pro.ip,
                            pro.area,
                            pro.remark,
                            created_at = pro.created_at.Value.ToString("yyyy/MM/dd HH:mm:ss")
                        };
            return Json(new
            {
                msg = "success",
                data = query,
                status = 200
            });
        }

        [HttpPost]
        public ActionResult GetAgentUserMoney(int order_id)
        {
            var token = [REDACTED]"Authorization"];
            var userInfo = new Repository.Entity.agent_total();
            var IsAuthenticated = verification.ValidateToken(token, ref userInfo);
            var result = _service.GetAgentUserMoney(order_id);
            var data = new
            {
                order_id = result.id,
                result.agent_id,
                balance = result.type== 2 ? result.balance : ((float)result.balance) /100,
                result.id,
                result.money,
                result.user_id,
                created_at = result.created_at.Value.ToString("yyyy/MM/dd HH:mm:ss"),
                result.type
            };

            return Json(new
            {
                msg = "success",
                data = data,
                status = 200
            });
        }
    }
}