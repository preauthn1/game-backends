using System;
using System.Drawing;
using example.invalid;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using Agent.Helper;
using Repository.Entity.agentclub;
using Service.Service.AgentClub;
using ZXing;
using ZXing.QrCode;

namespace Agent.Areas.agentclub.Controllers
{
    [CheckLogin(false)]
    public class AdminController : BaseController
    {
        private AgentService _service = new AgentService();
        private SystemService _systemService = new SystemService();

        // GET: agentclub/Admin
        [OutputCache(Duration = 10000, Location = OutputCacheLocation.ServerAndClient)]
        public ActionResult Index()
        {
            Response.Cache.SetOmitVaryStar(true);
            return View();
        }

        // Post: agentclub/Admin/login
        [HttpPost]
        public ActionResult login(string username, string password)
        {
            var user = _service.GetAgentUser(username);

            if (user == null) return Json(new { status = 10001 });
            if (user.Status == 0) return Json(new
            {
                msg = "帐号未启用",
                status = 10010
            });

            var source = password + user.Encrypt;
            var isCheck = HelpExtensions.CheckPasswordMD5(user.Password, source);

            if (!isCheck) return Json(new { status = 10001 });

            string IPAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.UserHostAddress;
            _systemService.CreateLogonLog(user.ID, IPAddress);

            var userID = user.ID;
            var userType = user.Class;
            var name = user.LoginName;
            var allowWithdraw = user.AllowWithdraw;
            var token = [REDACTED], userType, name, allowWithdraw, 360);

            return Json(new
            {
                token,
                status = 200
            });
        }

        // Get: agentclub/Admin/ValidateToken
        [HttpGet]
        public ActionResult validateToken()
        {
            var token = [REDACTED]"Authorization"];
            var userInfo = new AgentUsers();

            var IsAuthenticated = verification.ValidateToken(token, ref userInfo);
            var Class = userInfo.Class;
            var LoginName = userInfo.LoginName;
            var AllowWithdraw = userInfo.AllowWithdraw;

            return Json(new
            {
                status = 200,
                IsAuthenticated,
                Class,
                LoginName,
                AllowWithdraw
            }, JsonRequestBehavior.AllowGet);
        }


        // Get: /Admin/GetSystemConfig
        [HttpGet]
        public ActionResult GetSystemConfig()
        {
            var result = _systemService.GetSystemConfig();

            var data = new
            {
                Class1ChargeFee = result.Class1ChargeFee / 100,
                Class2ChargeFee = result.Class2ChargeFee / 100,
                Class3ChargeFee = result.Class3ChargeFee / 100,
                Class4ChargeFee = result.Class4ChargeFee / 100,
                result.OfficialUser1,
                result.OfficialUser2,
                result.OfficialUser3,
                result.User1Type,
                result.User2Type,
                result.User3Type,
                result.OfficialSite
            };

            return Json(new
            {
                status = 200,
                data = data
            }, JsonRequestBehavior.AllowGet);
        }

        // Post: /Admin/UpdateSystemConfig
        [HttpPost]
        public ActionResult UpdateSystemConfig(SystemConfig systemConfig)
        {
            var msg = string.Empty;
            var result = _systemService.UpdateSystemConfig(systemConfig, AdminID, AdminID, ref msg);
            _service.UpdateAgentUserQRUrl(AdminID, systemConfig.OfficialSite);

            return Json(new
            {
                msg,
                status = result ? 20001 : 10010,
                data = result
            }, JsonRequestBehavior.AllowGet);
        }

        // Post: /Admin/GetDomainConfig
        [HttpPost]
        public ActionResult GetDomainConfig(string domain)
        {
            var result = _systemService.GetDomainConfig(domain);

            return Json(new
            {
                status = 200,
                result
            }, JsonRequestBehavior.AllowGet);
        }


        // Post: /Admin/UpdateDomainConfig
        [HttpPost]
        public ActionResult UpdateDomainConfig(DomainConfig domainConfig)
        {
            var msg = string.Empty;
            var result = _systemService.UpdateDomainConfig(domainConfig, ref msg);

            return Json(new
            {
                msg,
                status = result ? 20001 : 10010,
                result
            }, JsonRequestBehavior.AllowGet);
        }

        // Get: /Admin/GetPlatform
        [HttpGet]
        public ActionResult GetPlatform()
        {
            var platform = _systemService.GetPlatform();

            var res = platform.Select(c => new
            {
                value = c.PlatformID,
                text = c.Name
            });

            return Json(new
            {
                status = 200,
                data = res
            }, JsonRequestBehavior.AllowGet);
        }

        // Get: /Admin/GetArea
        [HttpGet]
        public ActionResult GetArea(int platformID)
        {
            var area = _systemService.GetArea(platformID);

            var res = area.Select(c => new
            {
                value = c.AreaID,
                text = c.AreaName
            });

            return Json(new
            {
                status = 200,
                data = res
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: /Admin/GetLiveQRCode
        public ActionResult GetLiveQRCode(string url)
        {
            try
            {
                if (!string.IsNullOrEmpty(url))
                {
                    var writer = new BarcodeWriter
                    {
                        Format = BarcodeFormat.QR_CODE,
                        Options = new QrCodeEncodingOptions
                        {
                            Height = 300,
                            Width = 300,
                            Margin = 1
                        }
                    };
                    Bitmap m_Bitmap = writer.Write(url);

                    MemoryStream ms = new MemoryStream();
                    m_Bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                    byte[] buff = ms.ToArray();

                    return File(buff, "image/jpeg");
                }
            }
            catch (Exception e)
            {
                return new EmptyResult();
            }

            return new EmptyResult();
        }
    }
}