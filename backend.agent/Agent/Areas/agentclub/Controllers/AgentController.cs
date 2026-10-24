using System;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Mvc;
using Agent.Helper;
using com.show.api;
using Repository.Entity.agentclub;
using Repository.ViewModel;
using Service.Helper;
using Service.Service.AgentClub;

namespace Agent.Areas.agentclub.Controllers
{
    [CheckLogin(true)]
    public class AgentController : BaseController
    {
        private AgentService _service = new AgentService();
        private SystemService _systemService = new SystemService();

        // Post: Agent/GetAgentWithdraw
        [HttpPost]
        public ActionResult GetAgentWithdraw(int? user_id, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var result = _service.GetAgentWithdraw(AdminID, user_id, startDate, endDate);
                var query = from pro in result
                            select new
                            {
                                pro.ApplyID,
                                pro.UserID,
                                pro.LoginName,
                                WithdrawAmount = pro.WithdrawAmount.ToCash(),
                                RemainAmount = pro.RemainAmount.ToCash(),
                                ClassName = pro.Class.GetEnumDescription(),
                                pro.PlatformName,
                                pro.AreaName,
                                pro.Memo,
                                Status = pro.Status,
                                StatusName = pro.Status.GetEnumDescription(),
                                CreateTime = pro.CreateTime.Value.ToString("yyyy/MM/dd HH:mm:ss"),
                            };

                return Json(new
                {
                    data = query,
                    status = 200
                });
            }
            catch (Exception ex)
            {
                return OnError(ex.Message);
            }
        }


        // Post: Agent/AgentWithdraw
        [HttpPost]
        public ActionResult AgentWithdraw(long? remainAmount)
        {
            try
            {
                var msg = string.Empty;
                string IPAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.UserHostAddress;
                var result = _service.AgentWithdraw(AdminID, remainAmount, IPAddress, ref msg);

                return Json(new
                {
                    msg,
                    result,
                    status = result ? 20001 : 10010
                });
            }
            catch (Exception ex)
            {
                return OnError(ex.Message);
            }
        }

        // Post: Agent/CreateWithdrawApply
        [HttpPost]
        public ActionResult UpdateWithdrawApply(Guid applyUID, ApplyStatus status)
        {
            try
            {
                var msg = string.Empty;
                var result = _service.UpdateWithdrawApply(applyUID, status, ref msg);

                return Json(new
                {
                    msg,
                    result,
                    status = result ? 20001 : 10010
                });
            }
            catch (Exception ex)
            {
                return OnError(ex.Message);
            }
        }

        // Post: Agent/GetAgentApply
        [HttpPost]
        public ActionResult GetAgentApply(int? user_id, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var msg = string.Empty;
                var result = _service.GetAgentApply(AdminID, user_id, startDate, endDate);
                var query = from pro in result
                            select new
                            {
                                pro.ApplyID,
                                pro.UserID,
                                pro.LoginName,
                                ApplyClassName = pro.ApplyClass.GetEnumDescription(),
                                pro.PlatformName,
                                pro.AreaName,
                                pro.Tel,
                                pro.Weixin,
                                pro.QQ,
                                pro.Email,
                                pro.Memo,
                                Status = pro.Status,
                                StatusName = pro.Status.GetEnumDescription(),
                                CreateTime = pro.CreateTime.Value.ToString("yyyy/MM/dd HH:mm"),
                            };

                return Json(new
                {
                    msg = msg,
                    data = query,
                    status = 200
                });
            }
            catch (Exception ex)
            {
                return OnError(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult AgentPassed(Guid applyUID, ApplyStatus status)
        {
            try
            {
                var msg = string.Empty;
                string inviteCode = null;

                if (status == ApplyStatus.Passed)
                {
                    inviteCode = DemoRandom.GetRandomString(6);
                }

                var isSuccess = _service.AgentPassed(applyUID, status, inviteCode, ref msg);

                return Json(new
                {
                    msg = msg,
                    isSuccess,
                    status = 200
                });
            }
            catch (Exception ex)
            {
                return OnError(ex.Message);
            }
        }

        public ActionResult GetAgentUserAll()
        {
            var result = _service.GetAgentUserAll();
            var query = from pro in result
                        select new
                        {
                            pro.UserID,
                            pro.InviteCode
                        };

            return Json(new
            {
                data = query,
                status = 200
            });
        }

        // Post: Agent/GetAgentUser
        [HttpPost]
        public ActionResult GetAgentUser(Guid? agent_id, int? user_id, string loginName, AgentClass? agentClass)
        {
            try
            {
                var msg = string.Empty;
                long sumTotal = 0;
                var isAdmin = false;
                if (agent_id == null)
                {
                    agent_id = AdminID;
                    isAdmin = true;
                }

                var result = _service.GetAgentUser(agent_id.Value, user_id, loginName, agentClass, ref msg, ref sumTotal, isAdmin);
                var query = from pro in result
                            select new
                            {
                                pro.ID,
                                pro.UserID,
                                pro.LoginName,
                                pro.Tel,
                                pro.Weixin,
                                pro.QQ,
                                pro.Email,
                                Class = pro.Class,
                                pro.Level,
                                ClassName = pro.Class.GetEnumDescription(),
                                StatusText = pro.Status.GetEnumDescription(),
                                Status = pro.Status,
                                pro.PlatformName,
                                pro.AreaName,
                                TotalFlows = pro.TotalFlows.Value.ToCash(),
                                TotalIncome = pro.TotalIncome.Value.ToCash(),
                                RemainAmount = pro.RemainAmount.Value.ToCash(),
                                UserTotalFlows = pro.UserTotalFlows.Value.ToCash(),
                                pro.InviteCode,
                                CreateTime = pro.CreateTime.Value.ToString("yyyy/MM/dd HH:mm"),
                                pro.DistributeRate,
                                pro.Class1RateRange,
                                pro.Class2RateRange,
                                pro.Class3RateRange,
                                pro.Class4RateRange,
                                pro.Domain,
                                pro.QRUrl,
                                pro.AllowWithdraw
                            };

                return Json(new
                {
                    msg = msg,
                    sumTotal = sumTotal.ToCash(),
                    data = query,
                    status = 200
                });
            }
            catch (Exception ex)
            {
                return OnError(ex.Message);
            }
        }

        // Get: /Agent/GetGrandConfigItems
        [HttpGet]
        public ActionResult GetGrandConfigItems(Guid id)
        {
            var grandConfigItems = _service.GetGrandConfigItems(id);

            var result = grandConfigItems.Select(c => new
            {
                level = c.Level,
                value = c.Level,
                text = c.GradeName
            });

            return Json(new
            {
                status = 200,
                result
            }, JsonRequestBehavior.AllowGet);
        }

        // Post: Agent/UpdateAgentUser
        [HttpPost]
        public ActionResult UpdateAgentUser(Guid id, long? remainAmount, int? userID, string email, string tel, string weixin, string qq, int? grandConfigSelect,
            string QRUrl, vRateRange rateRange, bool? isInviteCode, bool? isPassword, int? agentStatus, int? allowWithdraw)
        {
            try
            {
                string password = [REDACTED];
                string encrypt = null;
                string inviteCode = null;
                var msg = string.Empty;

                if (isPassword != null && isPassword.Value)
                {
                    MD5 md5 = new MD5CryptoServiceProvider();
                    encrypt = DemoRandom.GetRandomString(6);
                    var source = "123456";

                    using (MD5 md5Hash = MD5.Create())
                    {
                        password = [REDACTED], source);
                        password = [REDACTED], password + encrypt);
                    }
                }
                if (isInviteCode != null && isInviteCode.Value) inviteCode = DemoRandom.GetRandomString(6);

                var result = _service.UpdateAgentUser(AdminID, id, remainAmount, userID, email, tel, weixin, qq, grandConfigSelect, QRUrl, rateRange, inviteCode, password, encrypt, agentStatus, allowWithdraw, ref msg);

                return Json(new
                {
                    msg,
                    result,
                    status = result ? 20001 : 10010
                });
            }
            catch (Exception ex)
            {
                return OnError(ex.Message);
            }
        }

        // Post: Agent/GetAgentCustomer
        [HttpPost]
        public ActionResult GetAgentCustomer(int? user_id, string loginName, AgentClass? agentClass, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var msg = string.Empty;
                var result = _service.GetAgentCustomer(AdminID, user_id, loginName, agentClass, startDate, endDate);

                var query = from pro in result
                            select new
                            {
                                pro.UserID,
                                pro.LoginName,
                                Class = pro.Class.GetEnumDescription(),
                                pro.Tel,
                                pro.Weixin,
                                pro.QQ,
                                pro.Email,
                                pro.PlatformName,
                                pro.AreaName,
                                UserTotalFlows = pro.UserTotalFlows.Value.ToCash(),
                                CreateTime = pro.CreateTime.Value.ToString("yyyy/MM/dd HH:mm"),
                            };

                return Json(new
                {
                    msg = msg,
                    data = query,
                    status = 200
                });
            }
            catch (Exception ex)
            {
                return OnError(ex.Message);
            }
        }

        // Post: Agent/GetGrandConfig
        [HttpPost]
        public ActionResult GetGrandConfig(int? user_id, string loginName)
        {
            try
            {
                var msg = string.Empty;
                var result = _service.GetGrandConfig(AdminID, user_id, loginName);
                var query = result.GroupBy(item => new { item.UserID, item.AgentID, item.ParentID, item.Class })
                .Select(group => new
                {
                    agentID = group.Key.ParentID,
                    userID = group.Key.AgentID,
                    playerID = group.Key.UserID,
                    className = group.Key.Class.GetEnumDescription(),
                    classID = group.Key.Class,
                    level = group.Select(item => new
                    {
                        grandID = item.ID,
                        agentID = item.ParentID,
                        userID = item.AgentID,
                        level = item.Level,
                        gradeName = item.GradeName,
                        distributeRate = item.DistributeRate,
                        upGradeLimit = item.UpGradeLimit / 100,
                        upGradeLimitToCash = item.UpGradeLimit.ToCash()
                    }).OrderBy(x => x.level)
                }).OrderBy(x => x.classID);

                return Json(new
                {
                    msg = msg,
                    data = query,
                    status = 200
                });
            }
            catch (Exception ex)
            {
                return OnError(ex.Message);
            }
        }

        // Post: Agent/GetAgentMaxMinRate
        [HttpPost]
        public ActionResult GetAgentMaxMinRate(Guid agentID)
        {
            try
            {
                var msg = string.Empty;
                var result = _service.GetAgentMaxMinRate(AdminID, agentID);

                return Json(new
                {
                    result,
                    status = 200
                });
            }
            catch (Exception ex)
            {
                return OnError(ex.Message);
            }
        }

        // Post: Agent/UpdateGrandConfig
        [HttpPost]
        public ActionResult UpdateGrandConfig(Guid? grandID, Guid agentID, Guid userID, int distributeRate)
        {
            try
            {
                var msg = string.Empty;
                var result = _service.UpdateGrandConfig(AdminID, grandID, agentID, userID, distributeRate, ref msg);

                return Json(new
                {
                    msg,
                    result,
                    status = result ? 20001 : 10010
                });
            }
            catch (Exception ex)
            {
                return OnError(ex.Message);
            }
        }

        // Post: Agent/DeleteGrandConfig
        [HttpPost]
        public ActionResult DeleteGrandConfig(Guid? grandID)
        {
            try
            {
                var msg = string.Empty;
                var result = _service.DeleteGrandConfig(AdminID, grandID, ref msg);

                return Json(new
                {
                    msg,
                    result,
                    status = result ? 20001 : 10010
                });
            }
            catch (Exception ex)
            {
                return OnError(ex.Message);
            }
        }

        // Post: Agent/GetInformation
        [HttpPost]
        public ActionResult GetInformation()
        {
            try
            {
                var result = _service.GetInformation(AdminID);
                var registeredCountResult = _service.GetRregisteredCount(AdminID);
                var authority = Request.Url.Authority;
                var url = authority + "/agentclub/admin/ShareQRCode?inviteCode=" + result.InviteCode;

                return Json(new
                {
                    data = new
                    {
                        result.UserID,
                        result.LoginName,
                        result.Tel,
                        result.Weixin,
                        result.QQ,
                        result.Email,
                        result.QRUrl,
                        result.Domain,
                        result.Class,
                        ClassName = result.Class.GetEnumDescription(),
                        result.InviteCode,
                        result.PlatformName,
                        result.AreaName,
                        result.GradeName,
                        result.DistributeRate,
                        TotalFlows = result.TotalFlows.ToCash(),
                        TotalIncome = result.TotalIncome.ToCash(),
                        RemainAmount = result.RemainAmount.ToCash(),
                        CheckAmount = result.RemainAmount,
                        ShareQRCode = url,
                        RegisteredCount = registeredCountResult,
                        result.OfficialUser1,
                        result.OfficialUser2,
                        result.OfficialUser3,
                        result.User1Type,
                        result.User2Type,
                        result.User3Type,
                    },
                    status = 200
                });
            }
            catch (Exception ex)
            {
                return OnError(ex.Message);
            }
        }

        // Post: Agent/UpdateInformation
        [HttpPost]
        public ActionResult UpdateInformation(int userID, string email, string tel, string weixin, string qq, string QRUrl,
            string OfficialUser1, string OfficialUser2, string OfficialUser3, int User1Type, int User2Type, int User3Type)
        {
            try
            {
                var msg = string.Empty;
                var result = _service.UpdateInformation(AdminID, AdminID, userID, email, tel, weixin, qq, QRUrl, OfficialUser1, 
                    OfficialUser2, OfficialUser3, User1Type, User2Type, User3Type, ref msg);

                return Json(new
                {
                    msg,
                    result,
                    status = result ? 20001 : 10010
                });
            }
            catch (Exception ex)
            {
                return OnError(ex.Message);
            }
        }

        // Post: Agent/GetRateRange
        [HttpPost]
        public ActionResult GetRateRange()
        {
            try
            {
                var result = _service.GetRateRange(AdminID);
                return Json(new
                {
                    result,
                    status = 200
                });
            }
            catch (Exception ex)
            {
                return OnError(ex.Message);
            }
        }

        // Post: Agent/CreateAgent
        [HttpPost]
        public ActionResult CreateAgent(int? platformID, int? userID, string loginName, string email, string tel, string weixin, string qq, string vCode)
        {
            try
            {
                string password = [REDACTED];
                string encrypt = null;
                string inviteCode = null;
                string inviteCodeMD5 = null;
                var msg = string.Empty;
                MD5 md5 = new MD5CryptoServiceProvider();
                encrypt = DemoRandom.GetRandomString(6);
                var source = "123456";

                inviteCode = DemoRandom.GetRandomString(6);

                using (MD5 md5Hash = MD5.Create())
                {
                    password = [REDACTED], source);
                    password = [REDACTED], password + encrypt);
                    inviteCodeMD5 = verification.GetMd5Hash(md5Hash, inviteCode);
                }

                var LiveCodeURL = HelpExtensions.getConfig("LiveCodeURL");

                var result = _service.CreateAgent(AdminID, platformID, userID, loginName, inviteCode, password, encrypt, email, tel, weixin, qq, $"{LiveCodeURL}{inviteCodeMD5}", vCode, ref msg);

                if (result)
                {
                    var domainConfig = _systemService.GetDomainConfig();
                    var showApiURL = HelpExtensions.getConfig("ShowApiURL");
                    var showapi_appid = HelpExtensions.getConfig("showapi_appid");
                    var showapi_sign = HelpExtensions.getConfig("showapi_sign");

                    string res = new ShowApiRequest(showApiURL, showapi_appid, showapi_sign)
                         .addTextPara("code", inviteCodeMD5)
                         .addTextPara("url", $"{domainConfig.Domain}/?a={inviteCode}")
                         .post();
                }

                return Json(new
                {
                    msg,
                    result,
                    status = result ? 20001 : 10010
                });
            }
            catch (Exception ex)
            {
                return OnError(ex.Message);
            }
        }

        // Post: Agent/CustomerUpgrade
        [HttpPost]
        public ActionResult CustomerUpgrade(int userID)
        {
            try
            {
                var msg = string.Empty;
                var inviteCodeMD5 = string.Empty;
                MD5 md5 = new MD5CryptoServiceProvider();
                var inviteCode = DemoRandom.GetRandomString(6);
                var LiveCodeURL = HelpExtensions.getConfig("LiveCodeURL");

                using (MD5 md5Hash = MD5.Create())
                {
                    inviteCodeMD5 = verification.GetMd5Hash(md5Hash, inviteCode);
                }

                var result = _service.CustomerUpgrade(AdminID, userID, inviteCode, $"{LiveCodeURL}{inviteCodeMD5}", ref msg);

                if (result)
                {
                    var domainConfig = _systemService.GetDomainConfig();
                    var showApiURL = HelpExtensions.getConfig("ShowApiURL");
                    var showapi_appid = HelpExtensions.getConfig("showapi_appid");
                    var showapi_sign = HelpExtensions.getConfig("showapi_sign");

                    string res = new ShowApiRequest(showApiURL, showapi_appid, showapi_sign)
                         .addTextPara("code", inviteCodeMD5)
                         .addTextPara("url", $"{domainConfig.Domain}/?a={inviteCode}")
                         .post();
                }

                return Json(new
                {
                    msg,
                    result,
                    status = result ? 20001 : 10010
                });
            }
            catch (Exception ex)
            {
                return OnError(ex.Message);
            }
        }

        // Post: Agent/UpdatePassword
        [HttpPost]
        public ActionResult UpdatePassword(string oldPassword, string newPassword)
        {
            try
            {
                var user = _service.GetAgentUser(AdminID);
                if (user == null) return Json(new { status = 10009 });

                var source = oldPassword + user.Encrypt;
                var isCheck = HelpExtensions.CheckPasswordMD5(user.Password, source);
                if (!isCheck) return Json(new { status = 10008 });

                string password = [REDACTED];
                string encrypt = null;
                var msg = string.Empty;
                MD5 md5 = new MD5CryptoServiceProvider();
                encrypt = DemoRandom.GetRandomString(6);
                var newpass = newPassword;

                using (MD5 md5Hash = MD5.Create())
                {
                    password = [REDACTED], newpass);
                    password = [REDACTED], password + encrypt);
                }

                var result = _service.UpdatePassword(AdminID, password, encrypt, ref msg);
                return Json(new
                {
                    result,
                    status = result ? 20001 : 10010
                });
            }
            catch (Exception ex)
            {
                return OnError(ex.Message);
            }
        }


        // Get: Agent/GetAgentRecursiveDown
        [HttpGet]
        public ActionResult GetAgentRecursiveDown(Guid id)
        {
            try
            {
                var result = _service.GetAgentRecursiveDown(id);
                return Json(new
                {
                    result,
                    status = 200
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return OnError(ex.Message, HttpMethod.Get);
            }
        }

        // Get: Agent/GetAgentApplyCount
        [HttpGet]
        public ActionResult GetAgentApplyCount()
        {
            try
            {
                var result = _service.GetAgentApplyCount(AdminID);
                return Json(new
                {
                    result,
                    status = 200
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return OnError(ex.Message, HttpMethod.Get);
            }
        }
    }
}