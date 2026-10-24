using game.Entity.Users;
using System;
using System.Security.Claims;
using System.Web.Mvc;
using System.Linq;
using FW.Core.Helper.Enum;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using game.Infrastructure;
using System.Web;
using example.invalid;
using System.Web.Helpers;

namespace game.WebSite.Controllers
{
    public class ServiceTalkController : Controller
    {
        int MaxSizeBytes = 12 * 1024 * 1024; // max 6Mb
        // GET: ServiceTalk
        public ActionResult Index()
        {
            return View();
        }

        // GET: ServiceTalk
        [HttpGet]
        public ActionResult Index(string cerCode, bool isUploadFile = false)
        {
            int? userID = null;
            int? platformID = null;
            int? areaID = null;

            if (CheckUser(cerCode, ref userID, ref platformID, ref areaID))
            {
                var token = [REDACTED], platformID.Value, areaID.Value, 60);
                ViewBag.token = [REDACTED];
            }

            ViewBag.isUploadFile = isUploadFile;
            return View();
        }

        public bool CheckUser(string cerCode, ref int? userID, ref int? platformID, ref int? areaID)
        {
            var api = SiteHelper.GetAPIResult<UserInfoToken>(string.Format("/User/GetUserInfoByCerCode?cerCode={0}", cerCode));

            if (api == null) return false;

            userID = api.userID;
            platformID = api.platformID;
            areaID = api.areaID;
            return true;
        }

        [HttpPost]
        public ActionResult GetReplayMessage()
        {
            var StartTime = Convert.ToDateTime(DateTime.Now.AddDays(-7).ToShortDateString());
            var handelPerson = string.Empty;

            var token = [REDACTED]"Authorization"];
            var userInfo = new UserInfoToken();

            var IsAuthenticated = ValidateToken(token, ref userInfo);

            //清除通知
            SiteHelper.GetAPIResult(
                string.Format("User/ServiceTalkCleanNotifyCount?userid={0}", userInfo.userID));


            var api = SiteHelper.GetAPIResult<List<QuestionLog>>(
                string.Format("User/GetUserMessageById?UserID={0}", userInfo.userID));

            var query = from pro in api
                        orderby pro.Id
                        select new
                        {
                            QId = pro.Id,
                            pro.UserID,
                            pro.User.FaceID,
                            pro.Question,
                            pro.ReplyContent,
                            CreateTime = pro.CreateTime.ToString("yyyy-MM-dd HH:mm"),
                            pro.DisplayName,
                            HandleTime = pro.HandleTime.ToString("yyyy-MM-dd HH:mm"),
                            Status = pro.Status.GetDisplayName(),
                            StatusCode = pro.Status,
                            image = string.IsNullOrEmpty(pro.UploadImage) ? string.Empty : string.Format("/{0}/{1}{2}", "Upload", "QuestionImage", pro.UploadImage)
                        };

            return Json(new
            {
                success = true,
                data = query
            });
        }

        [HttpPost]
        public ActionResult CreateQuestion(string msg)
        {
            if (ContainsInvalidChar(msg))
            {
                return Json(new
                {
                    status = false,
                    msg = "操作失败：不允许出现特殊字符！"
                });
            }

            var token = [REDACTED]"Authorization"];
            var data = sendQuestion(token, msg);

            return Json(data);
        }

        public ActionResult UploadFile()
        {
            foreach (string file in Request.Files)
            {
                HttpPostedFileBase f = Request.Files[file] as HttpPostedFileBase;

                if (f.ContentLength == 0 || f.ContentLength > MaxSizeBytes)
                    return Content("无法取得图片,拍照后请等待五秒再按确认");

                if (f.ContentLength > MaxSizeBytes)
                    return Content("您的图片大小已超出，请小于12兆( " + f.ContentLength + " )");

                if (!f.FileName.EndsWith("jpg") && !f.FileName.EndsWith("jpeg") && !f.FileName.EndsWith("png"))
                    return Content("图片类型请使用jpg,jpeg,png ( " + f.FileName + " )");

                var guid = GetRandomStringByGuid(21);

                var updateFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Upload", "QuestionImage", DateTime.Now.ToString("yyyyMMdd"));

                if (!Directory.Exists(updateFile))
                {
                    Directory.CreateDirectory(updateFile);
                }                                

                string savedFileName = Path.Combine(updateFile, Path.GetFileName(guid + Path.GetExtension(Request.Files[0].FileName)));
                string savedFileName2 = Path.Combine(updateFile, Path.GetFileName("ORIG-" + guid + Path.GetExtension(Request.Files[0].FileName)));
                
                Stream stream = new MemoryStream();
                f.InputStream.CopyTo(stream);
              
                var origImage = new WebImage(f.InputStream);
                origImage.Resize(800, 800, true, true);
                origImage.Save(savedFileName2, "jpg", true);                
               
                var thumbImage = new WebImage(stream);
                thumbImage.Resize(144, 256, true, true);
                thumbImage.Save(savedFileName, "jpg", true);

                var token = [REDACTED]"Authorization"];
                var data = sendQuestion(token, "image", string.Format("/{0}/{1}", DateTime.Now.ToString("yyyyMMdd"), Path.GetFileName(thumbImage.FileName)));

                return Json(data);

            }
            return Json(new
            {
                status = 200
            });
        }

        [HttpPost]
        public ActionResult CheckUser()
        {
            var token = [REDACTED]"Authorization"];
            var userInfo = new UserInfoToken();

            var IsAuthenticated = ValidateToken(token, ref userInfo);

            return Json(new
            {
                status = 200,
                IsAuthenticated
            }, JsonRequestBehavior.AllowGet);
        }

        private static object sendQuestion(string token, string msg, string img = "")
        {
            var userInfo = new UserInfoToken();
            var IsAuthenticated = ValidateToken(token, ref userInfo);
            var IPAddress = SiteHelper.GetIP();
            if (string.IsNullOrEmpty(IPAddress)) IPAddress = "0.0.0.0";

            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/User/CreateQuestion?userid={0}&platformId={1}&areaID={2}&msg={3}&IPAddress={4}&imageUrl={5}", userInfo.userID, userInfo.platformID, userInfo.areaID, HttpUtility.UrlEncode(msg), IPAddress, img));

            return new
            {
                status = api.Result,
                msg = api.Message
            };
        }

        private static bool ValidateToken(string token, ref UserInfoToken userInfo)
        {
            var simplePrinciple = JwtManager.GetPrincipal(token);

            if (simplePrinciple == null) return false;

            var identity = simplePrinciple.Identity as ClaimsIdentity;

            if (identity == null)
                return false;

            if (!identity.IsAuthenticated)
                return false;

            var usernameClaim = identity.FindFirst(ClaimTypes.Name);
            var userData = identity.FindFirst(ClaimTypes.UserData);

            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            userInfo = json_serializer.Deserialize<UserInfoToken>(userData.Value);

            if (userInfo == null)
                return false;

            return true;
        }

        private static string GetRandomStringByGuid(int length)
        {
            var str = Guid.NewGuid().ToString().Replace("-", "");
            return str.Substring(0, length);
        }

        /// <summary>
        /// 是否包含特殊字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static bool ContainsInvalidChar(string str)
        {

            string[] charString = new string[] { "{", "}", "[", "]", "\"", "\\", "'", "=", "null" };

            str = str.ToLower();
            foreach (var c in charString)
            {
                if (str.Contains(c) == true)
                {
                    return true;
                }
            }
            return false;
        }
    }
}