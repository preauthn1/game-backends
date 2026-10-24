using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FW.Core.AdminSys.Provider;
using FW.Core.AdminSys.Provider.APIProvider;
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
        public UserController(IAdminSysProvider provider )
        {
            _provider = provider;
        }

        [HttpGet]
        public ActionResult List(int? organizationID, string loginName = null, int p = 1)
        {
            int orgId = GetCurrentAdminUserOrgID();
            string orgIdText = GetOrganizationIdOrEmpty(orgId);

            string organizationIDs = organizationID.ToString();
            if (organizationID == null)
                organizationIDs = orgIdText;

            var queryOrgs = AdmiSysWebUiApiDataHelper.GetOrganizations(orgIdText);
            var data = AdmiSysWebUiApiDataHelper.GetAdminUsersPagingByOrgId(organizationIDs, loginName, pageIndex: p, pageSize: 20);

            var _userType = new Dictionary<int, string>()
            {
                {0, "一般"}, {8, "财务"},{9, "客服"}, {10, "联运"}
            };

            ViewBag.UserType = _userType;
            ViewBag.Organizations = AdmiSysWebUiApiDataHelper.GetOrganizations("");
            ViewBag.OrgDropdown = GetOrganizationsSelect(queryOrgs, organizationID.ToString());
            return ThemeableView("List", data);
        }

        private List<SelectListItem> GetOrganizationsSelect(List<Organization> Organization, string defaultSelected = "")
        {
            bool isDefaultSelect = false;
            var list = new List<SelectListItem>();
            foreach (var row in Organization)
            {
                list.Add(new SelectListItem()
                {
                    Text = row.Name,
                    Value = row.OrgID.ToString(),
                    Selected = row.OrgID.ToString() == defaultSelected
                });
            }
            //如果非系統管理員 選項只有一個 
            if (list != null && list.Count() > 1)
            {
                isDefaultSelect = true;
            }
            GetSelectHtml(list, defaultSelected, isDefaultSelect);
            return list;
        }


        public ActionResult Add()
        {
            var orgId = GetCurrentAdminUserOrgID();
            var orgIdText = GetOrganizationIdOrEmpty(orgId);
            var queryOrgs = AdmiSysWebUiApiDataHelper.GetOrganizations(orgIdText);

            ViewBag.OrgDropdown = GetOrganizationsSelect(queryOrgs, orgIdText);
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
            AdminUser user = _provider.GetUserByID(id);

            //檢查是否查到別組織的資料
            CheckIsOwnPermissionForOrg(user.OrgID);

            int _key = 0;

            if (user.SecretKey != 0)
            {
                _key = user.SecretKey;
                ViewBag.QrCodeUrl = this.getQRCodeUrl(ref _key);
                ViewBag.secretKey = _key;

                string otpauth = Server.UrlDecode(HttpUtility.ParseQueryString(ViewBag.QrCodeUrl).Get("chl"));
                string[] sArr = otpauth.Split('?');
                if (sArr.Length > 1)
                {
                    ViewBag.secretKey = HttpUtility.ParseQueryString(sArr[1]).Get("secret");
                }
                //ViewBag.secretKey = _key;
            }

            ViewBag.UserType = GetUserType(_provider.GetUserType(), ((int)user.UserType).ToString());
            return ThemeableView("Modify", _provider.GetUserByID(id));
        }

        private static List<SelectListItem> GetSelectHtml(List<SelectListItem> list, string defaultSelected = "", bool isDefaultSelect = false)
        {
            if (isDefaultSelect)
            {
                list.Insert(0, new SelectListItem()
                {
                    Value = "",
                    Text = "请选择",
                    Selected = string.IsNullOrEmpty(defaultSelected)
                });
            }
            return list;
        }


        public static List<SelectListItem> GetUserType(List<UserIdentity> UserType, string defaultSelected = "", bool isDefaultSelect = false)
        {
            var list = new List<SelectListItem>();
            foreach (var type in UserType)
            {
                list.Add(new SelectListItem()
                {
                    Text = type.TypeName,
                    Value = type.id.ToString(),
                    Selected = type.id.ToString() == defaultSelected
                });
            }

            GetSelectHtml(list, defaultSelected, isDefaultSelect);
            return list;
        }


        [HttpPost]
        public ActionResult Modify(AdminUser user, int status)
        {
            string message = "";
            int? _secretKey = null;
            int userType = 0; //userType欄位已無用處 預設值即可
            _secretKey = user.SecretKey == 0 ? 0 : user.SecretKey;
            _provider.ModifyUserInfo(user.UserID, user.LoginName, user.DisplayName, user.Password, user.Phone, userType, status, _secretKey, ref message);
            return Alert(message);
        }

        [HttpGet]
        public ActionResult Del(int id)
        {
            var queryUser = _provider.GetUserByID(id);

            //檢查是否查到別組織的資料
            CheckIsOwnPermissionForOrg(queryUser.OrgID);

            string message = "";
            _provider.DeleteUser(id, ref message);
            return Alert(message, Url.Action("List"));
        }

        [HttpGet]
        public ActionResult UserRight(int id)
        {
            var queryUser = _provider.GetUserByID(id);

            //檢查是否查到別組織的資料
            CheckIsOwnPermissionForOrg(queryUser.OrgID);

            ViewBag.user = queryUser;
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
            var queryUser = _provider.GetUserByID(id);

            //檢查是否查到別組織的資料
            CheckIsOwnPermissionForOrg(queryUser.OrgID);


            ViewBag.user = queryUser;
            ViewBag.roleAssignRightCount = AdmiSysWebUiApiDataHelper.GetTargetRoleAssignRightCount(queryUser.OrgID);
            ViewBag.userRoles = _provider.GetUserRole(id, null)
                .Select(rr => rr.RoleID)
                .ToList();

            //已改成只抓該用戶所屬組織的roles
            string orgId = queryUser.OrgID.ToString();
            ViewBag.roles = AdmiSysWebUiApiDataHelper.GetRolesByOrgId(orgId);

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

            return Alert("更新成功");
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