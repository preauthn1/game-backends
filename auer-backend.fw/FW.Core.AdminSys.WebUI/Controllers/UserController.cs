using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using FW.Core.AdminSys.Provider;
using FW.Core.AdminSys.Provider.Entity;
using FW.Core.Helper;
using Google.Authenticator;

namespace FW.Core.AdminSys.WebUI.Controllers
{
    public class UserController : AdminSysBaseController
    {
        private IAdminSysProvider _provider;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="provider"></param>
        public UserController(IAdminSysProvider provider)
        {
            _provider = provider;
        }

        public ActionResult List(int p = 1)
        {
            var data = _provider.GetUser(null, null, null, null, null, p, 20);
            var _userType = new Dictionary<int, string>()
            {
                {0, "一般"}, {8, "财务"},{9, "客服"}, {10, "联运"}
            };

            ViewBag.UserType = _userType;
            return ThemeableView("List", data);
        }

        public ActionResult Add()
        {
            ViewBag.UserType = GetUserType(_provider.GetUserType());
            return ThemeableView("Add");
        }

        [HttpPost]
        public ActionResult Add(AdminUser user)
        {
            user.CreateTime = DateTime.Now;
            user.LastLogonTime = DateTime.Now;
            user.LastLogonIPAddress = IPHelper.GetIP();

            string message = "";
            _provider.CreateUser(user, ref message);

            return Alert(message);
        }

        [HttpGet]
        public ActionResult Modify(int id)
        {
            var user = _provider.GetUserByID(id);
            int _key = 0;

            if (user.SecretKey != 0)
            {
                _key = user.SecretKey;
                ViewBag.QrCodeUrl = this.getQRCodeUrl(ref _key);
                ViewBag.secretKey = _key;
            }

            ViewBag.UserType = GetUserType(_provider.GetUserType(), ((int)user.UserType).ToString());
            return ThemeableView("Modify", _provider.GetUserByID(id));
        }


        public static List<SelectListItem> GetUserType(List<UserIdentity> UserType, string selected = "", bool all = false)
        {
            var list = new List<SelectListItem>();
            foreach (var type in UserType)
            {
                list.Add(new SelectListItem()
                {
                    Text = type.TypeName,
                    Value = type.id.ToString(),
                    Selected = type.id.ToString() == selected
                });
            }
            if (all) list.Insert(0, new SelectListItem()
            {
                Value = "",
                Text = "请选择",
                Selected = string.IsNullOrEmpty(selected)
            });
            return list;
        }


        [HttpPost]
        public ActionResult Modify(AdminUser user, int type, int status)
        {
            string message = "";
            int? _secretKey = null;
            _secretKey = user.SecretKey == 0 ? 0 : user.SecretKey;
            _provider.ModifyUserInfo(user.UserID, user.LoginName, user.DisplayName, user.Password, user.Phone, type, status, _secretKey, ref message);
            return Alert(message);
        }

        [HttpGet]
        public ActionResult Del(int id)
        {
            string message = "";
            _provider.DeleteUser(id, ref message);
            return Alert(message, Url.Action("List"));
        }

        [HttpGet]
        public ActionResult UserRight(int id)
        {
            ViewBag.user = _provider.GetUserByID(id);
            ViewBag.userRights = _provider.GetUserRight(id, null)
                .Select(rr => rr.RightID)
                .ToList();

            return ThemeableView("UserRight");
        }

        [HttpPost]
        public ActionResult UserRight(int id, string[] ck)
        {
            string message = "";
            _provider.RemoveUserRight(id, null, ref message);

            if (ck != null && ck.Length > 0)
            {
                ck.ToList().ForEach(s =>
                {
                    int rid = Convert.ToInt32(s);

                    _provider.AddUserRight(id, rid, ref message);
                });
            }

            return RedirectToAction("UserRight", new { id = id });

        }

        [HttpGet]
        public ActionResult UserRole(int id)
        {
            ViewBag.user = _provider.GetUserByID(id);
            ViewBag.userRoles = _provider.GetUserRole(id, null)
                .Select(rr => rr.RoleID)
                .ToList();
            ViewBag.roles = _provider.GetRole(null, null);

            return ThemeableView("UserRole");
        }

        [HttpPost]
        public ActionResult UserRole(int id, string[] ck)
        {
            string message = "";
            _provider.RemoveUserRole(id, null, ref message);

            if (ck != null && ck.Length > 0)
            {
                ck.ToList().ForEach(s =>
                {
                    int rid = Convert.ToInt32(s);

                    _provider.AddUserRole(id, rid, ref message);
                });
            }

            return RedirectToAction("UserRole", new { id = id });
        }

        public ActionResult Log(string loginName, string ipAddress, DateTime? beginTime, DateTime? endTime, string memo, int p = 1)
        {
            int? userID = null;

            if (beginTime == null) beginTime = DateTime.Today;

            if (!string.IsNullOrWhiteSpace(loginName))
            {
                var user = _provider.GetUserByLoginName(loginName);
                if (user == null)
                {
                    return ThemeableView("Log", null);
                }

                userID = user.UserID;
            }

            var logs = _provider.GetAdminLog(userID, ipAddress, beginTime, endTime, memo, p, 20);
            return ThemeableView("Log", logs);
        }

        [HttpGet]
        public ActionResult GetQRCode()
        {
            int _key = 0;
            var _resultUrl = this.getQRCodeUrl(ref _key);
            return Json(new { success = true, url = _resultUrl, key = _key }, JsonRequestBehavior.AllowGet);
        }

        private string getQRCodeUrl(ref int secretKey)
        {

            if (secretKey == 0)
            {
                secretKey = new Random().Next(10000000, 999999999);
            }

            var _codeTag = ConfigurationManager.AppSettings["QRCodeTag"];

            //產生QR Code.
            var _tfa = new TwoFactorAuthenticator();
            var _setupInfo = _tfa.GenerateSetupCode(_codeTag, secretKey.ToString(), 150, 150);

            return _setupInfo.QrCodeSetupImageUrl;
        }

    }
}