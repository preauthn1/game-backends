using System;
using System.Web.Mvc;
using FW.Core.AdminSys.Provider;
using FW.Core.Auth;

namespace FW.Core.AdminSys.WebUI.Controllers
{
    public class AccountController : AdminSysBaseController
    {
        private IAdminSysProvider _adminSysProvider;
        private AuthManager _authManager;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="adminSysProvider"></param>
        /// <param name="authManager"></param>
        public AccountController(IAdminSysProvider adminSysProvider, AuthManager authManager)
        {
            _adminSysProvider = adminSysProvider;
            _authManager = authManager;
        }

        [HttpGet]
        public ActionResult ModifyLoginName()
        {
            var _userID = Convert.ToInt32(_authManager.CurrentUser.UserID);
            var _user = _adminSysProvider.GetUserByID(_userID);

            return ThemeableView("ModifyLoginName", _user);
        }

        [HttpPost]
        public ActionResult ModifyLoginName(string loginName, string displayName)
        {
            string message = "";
            int userID = Convert.ToInt32(_authManager.CurrentUser.UserID);

            if (_adminSysProvider.ModifyUserLoginName(userID, loginName, displayName, ref message))
            {
                var _user = _adminSysProvider.GetUserByID(userID);
                ViewBag.Success = "修改成功！";

                return ThemeableView("ModifyLoginName", _user);
            }

            return Alert(message);
        }

        [HttpGet]
        public ActionResult ModifyPassword()
        {
            return ThemeableView("ModifyPassword");
        }

        [HttpPost]
        public ActionResult ModifyPassword(string password)
        {
            string message = "";
            int userID = Convert.ToInt32(_authManager.CurrentUser.UserID);

            var _result = _adminSysProvider.ModifyUserInfo(userID, null, null, password, null, null, null, null, ref message);
            return Alert(message);
        }

    }

}
