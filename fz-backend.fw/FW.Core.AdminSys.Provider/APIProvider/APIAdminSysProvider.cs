using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using example.invalid;
using FW.Core.AdminSys.Provider.Entity;
using FW.Core.Cache;
using FW.Core.Helper;

namespace FW.Core.AdminSys.Provider.APIProvider
{
    public class APIAdminSysProvider : IAdminSysProvider
    {
        private ICacheManager _cacheManager;

        public APIAdminSysProvider(ICacheManager cacheMamager)
        {
            _cacheManager = cacheMamager;
        }

        private void clearUserRightCache(int userID)
        {
            string cacheKey = String.Format("_USERRIGHT_{0}", userID);
            _cacheManager.Remove(cacheKey);
        }

        private string base64(string code)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(code.ToLower()));
        }

        private string md5(string code)
        {
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(code.ToLower(), "md5");
        }

        public string encrypt(string code)
        {
            return md5(base64(md5(base64(code))));
        }

        public bool CreateUser(AdminUser user, ref string message)
        {
            user.Password = [REDACTED];
            var ret = APIHelper.PostAPIResult<AdminUser>("/AdminSys/CreateUser", user);
            if (ret.Result)
                user.UserID = Convert.ToInt32(ret.Extend);
            message = ret.Message;
            return ret.Result;
        }

        public bool ModifyUser(AdminUser user, int type, ref string message)
        {
            var userID = user.UserID;
            var loginName = user.LoginName;
            var password = [REDACTED];
            var phone = user.Phone;
            var status = Convert.ToBoolean(user.Status);
            var userType = type.ToString();
            var ret = APIHelper.GetAPIResult<APIResult>(string.Format("/AdminSys/ModifyUser?userID={0}&loginName={1}&password=[REDACTED]", userID, loginName, password, phone, status, userType));
            message = ret.Message;
            return ret.Result;
        }

        public void UpdateUserLoginInfo(AdminUser user)
        {
            APIHelper.GetAPIResult(string.Format("/AdminSys/UpdateUserLoginInfo?userID={0}&loginIPAddress={1}&loginTime={2}", user.UserID, user.LastLogonIPAddress, user.LastLogonTime));
        }

        public bool ModifyUserLoginName(int userID, string loginName, string displayName, ref string message)
        {
            var ret = APIHelper.GetAPIResult<APIResult>(string.Format("/AdminSys/ModifyUserLoginName?userID={0}&loginName={1}&displayName={2}", userID, loginName, displayName));
            message = ret.Message;
            return ret.Result;
        }

        public bool ModifyUserPassword(int userID, string password, ref string message)
        {
            password = [REDACTED];
            var ret = APIHelper.GetAPIResult<APIResult>(string.Format("/AdminSys/ModifyUserPassword?userID={0}&password=[REDACTED]", userID, password));
            message = ret.Message;
            return ret.Result;
        }

        public bool ModifyUserPhone(int userID, string phone, ref string message)
        {
            var ret = APIHelper.GetAPIResult<APIResult>(string.Format("/AdminSys/ModifyUserPhone?userID={0}&phone={1}", userID, phone));
            message = ret.Message;
            return ret.Result;
        }

        public bool ModifyUserPhoneCode(int userID, string phoneCode, DateTime phoneCodeTime, ref string message)
        {
            var ret = APIHelper.GetAPIResult<APIResult>(string.Format("/AdminSys/ModifyUserPhoneCode?userID={0}&phoneCode={1}&phoneCodeTime={2}", userID, phoneCode, phoneCodeTime));
            message = ret.Message;
            return ret.Result;
        }

        public bool DeleteUser(int userID, ref string message)
        {
            var ret = APIHelper.GetAPIResult<APIResult>(string.Format("/AdminSys/DeleteUser?userID={0}", userID));
            message = ret.Message;
            return ret.Result;
        }

        public AdminUser GetUserByID(int userID)
        {
            return APIHelper.GetAPIResult<AdminUser>(string.Format("/AdminSys/GetUserByID?userID={0}", userID));
        }

        public AdminUser GetUserByLoginName(string loginName)
        {
            return APIHelper.GetAPIResult<AdminUser>(string.Format("/AdminSys/GetUserByLoginName?loginName={0}", loginName));
        }

        public PagedList<AdminUser> GetUser(int? userID, string loginName, byte? status, DateTime? lastLogonTimeBegin, DateTime? lastLogonTimeEnd, int pageIndex, int pageSize)
        {
            return APIHelper.GetAPIResult<PagedList<AdminUser>>(string.Format("/AdminSys/GetUser?userID={0}&loginName={1}&status={2}&lastLogonTimeBegin={3}&lastLogonTimeEnd={4}&pageIndex={5}&pageSize={6}", userID, loginName, status, lastLogonTimeBegin, lastLogonTimeEnd, pageIndex, pageSize));
        }

        public AdminUser ValidateUserInfo(string loginName, string password, ref string message)
        {
            password = [REDACTED];
            var user = APIHelper.GetAPIResult<AdminUser>(string.Format("/AdminSys/ValidateUserInfo?loginName={0}&password=[REDACTED]", loginName, password));
            if (user == null)
            {
                message = "账号或密码错误";
                return null;
            }
            else if (user.Status == 0)
            {
                message = "账号已经被冻结,请联系管理员";
                return null;
            }
            else
            {
                message = "登录成功";
                return user;
            }
        }

        public void UpdateLastLogon(string loginName)
        {
            APIHelper.GetAPIResult<AdminUser>(string.Format("/AdminSys/UpdateLastLogon?loginName={0}", loginName));
        }

        public bool AddAdminLog(AdminLog log, ref string message)
        {
            var ret = APIHelper.PostAPIResult<AdminLog>("/AdminSys/AddAdminLog", log);
            message = ret.Message;
            return ret.Result;
        }

        public PagedList<AdminLog> GetAdminLog(int? userID, string ipAddress, DateTime? timeBegin, DateTime? timeEnd, string memo, int pageIndex, int pageSize)
        {
            return APIHelper.GetAPIResult<PagedList<AdminLog>>(string.Format("/AdminSys/GetAdminLog?userID={0}&ipAddress={1}&memo={2}&timeBegin={3}&timeEnd={4}&pageIndex={5}&pageSize={6}", userID, ipAddress, memo, timeBegin, timeEnd, pageIndex, pageSize));
        }

        public bool CreateRole(string roleName, ref string message)
        {
            var ret = APIHelper.GetAPIResult<APIResult>(string.Format("/AdminSys/CreateRole?roleName={0}", roleName));
            message = ret.Message;
            return ret.Result;
        }

        public bool ModifyRole(Role role, ref string message)
        {
            var ret = APIHelper.PostAPIResult<Role>("/AdminSys/ModifyRole", role);
            message = ret.Message;
            return ret.Result;
        }

        public bool DeleteRole(int roleID, ref string message)
        {
            var ret = APIHelper.GetAPIResult<APIResult>(string.Format("/AdminSys/DeleteRole?roleID={0}", roleID));
            message = ret.Message;
            return ret.Result;
        }

        public Role GetRoleByID(int roleID)
        {
            return APIHelper.GetAPIResult<Role>(string.Format("/AdminSys/GetRoleByID?roleID={0}", roleID));
        }

        public List<Role> GetRole(int? roleID, string roleName)
        {
            return APIHelper.GetAPIResult<List<Role>>(string.Format("/AdminSys/GetRole?roleID={0}&roleName={1}", roleID, roleName));
        }

        public bool AddRight(Right right, ref string message)
        {
            var ret = APIHelper.PostAPIResult<Right>("/AdminSys/AddRight", right);
            message = ret.Message;
            return ret.Result;
        }

        public bool ModifyRight(Right right, ref string message)
        {
            var ret = APIHelper.PostAPIResult<Right>("/AdminSys/ModifyRight", right);
            message = ret.Message;
            return ret.Result;
        }

        public bool DeleteRight(int rightID, ref string message)
        {
            var ret = APIHelper.GetAPIResult<APIResult>(string.Format("/AdminSys/DeleteRight?rightID={0}", rightID));
            message = ret.Message;
            return ret.Result;
        }

        public List<Right> GetRight(int? rightID, int? parentID, Entity.RightType? rightType)
        {
            return APIHelper.GetAPIResult<List<Right>>(string.Format("/AdminSys/GetRight?rightID={0}&parentID={1}&rightType={2}", rightID, parentID, rightType));
        }

        public Right GetRightByID(int rightID)
        {
            return APIHelper.GetAPIResult<Right>(string.Format("/AdminSys/GetRightByID?rightID={0}", rightID));
        }

        public Right GetRightByToken(string token)
        {
            return APIHelper.GetAPIResult<Right>(string.Format("/AdminSys/GetRightByToken?token=[REDACTED]", token));
        }

        public bool AddRoleRight(int roleID, int rightID, ref string message)
        {
            var ret = APIHelper.GetAPIResult<APIResult>(string.Format("/AdminSys/AddRoleRight?roleID={0}&rightID={1}", roleID, rightID));
            message = ret.Message;
            return ret.Result;
        }

        public bool RemoveRoleRight(int roleID, int? rightID, ref string message)
        {
            var ret = APIHelper.GetAPIResult<APIResult>(string.Format("/AdminSys/RemoveRoleRight?roleID={0}&rightID={1}", roleID, rightID));
            message = ret.Message;
            return ret.Result;
        }

        public List<RoleRight> GetRoleRight(int? roleID, int? rightID)
        {
            return APIHelper.GetAPIResult<List<RoleRight>>(string.Format("/AdminSys/GetRoleRight?roleID={0}&rightID={1}", roleID, rightID));
        }

        public bool AddUserRight(int userID, int rightID, ref string message)
        {
            clearUserRightCache(userID);

            var ret = APIHelper.GetAPIResult<APIResult>(string.Format("/AdminSys/AddUserRight?userID={0}&rightID={1}", userID, rightID));
            message = ret.Message;
            return ret.Result;
        }

        public bool RemoveUserRight(int userID, int? rightID, ref string message)
        {
            clearUserRightCache(userID);

            var ret = APIHelper.GetAPIResult<APIResult>(string.Format("/AdminSys/RemoveUserRight?userID={0}&rightID={1}", userID, rightID));
            message = ret.Message;
            return ret.Result;
        }

        public List<UserRight> GetUserRight(int? userID, int? rightID)
        {
            return APIHelper.GetAPIResult<List<UserRight>>(string.Format("/AdminSys/GetUserRight?userID={0}&rightID={1}", userID, rightID));
        }

        public List<Right> GetAllUserDisplayRight(int userID)
        {
            return this.GetAllUserRight(userID)
                .Where(r => r.ShowInMenu)
                .OrderByDescending(r => r.Sort)
                .ToList();
        }

        public List<Right> GetAllUserRight(int userID)
        {
            string cacheKey = String.Format("_USERRIGHT_{0}", userID);

            if (_cacheManager.IsCached(cacheKey))
            {
                return _cacheManager.GetObject<List<Right>>(cacheKey);
            }

            var rights = APIHelper.GetAPIResult<List<Right>>(string.Format("/AdminSys/GetAllUserRight?userID={0}", userID));

            _cacheManager.Add(cacheKey, rights, 1000 * 30);

            return rights;
        }

        public bool CheckUserRight(int userID, string token)
        {
            return this.GetAllUserRight(userID)
                .Where(r => String.Compare(r.RightToken, token, true) == 0)
                .Count() > 0;
        }

        public List<UserRole> GetUserRole(int? userID, int? roleID)
        {
            return APIHelper.GetAPIResult<List<UserRole>>(string.Format("/AdminSys/GetUserRole?userID={0}&roleID={1}", userID, roleID));
        }

        public List<UserIdentity> GetUserType()
        {
            return APIHelper.GetAPIResult<List<UserIdentity>>("/AdminSys/GetUserTypeList");
        }

        public bool AddUserRole(int userID, int roleID, ref string message)
        {
            clearUserRightCache(userID);

            var ret = APIHelper.GetAPIResult<APIResult>(string.Format("/AdminSys/AddUserRole?userID={0}&roleID={1}", userID, roleID));
            message = ret.Message;
            return ret.Result;
        }

        public bool RemoveUserRole(int userID, int? roleID, ref string message)
        {
            clearUserRightCache(userID);

            var ret = APIHelper.GetAPIResult<APIResult>(string.Format("/AdminSys/RemoveUserRole?userID={0}&roleID={1}", userID, roleID));
            message = ret.Message;
            return ret.Result;
        }

        public bool AddQuickAction(QuickAction quickAction, ref string message)
        {
            var ret = APIHelper.PostAPIResult<QuickAction>("/AdminSys/AddQuickAction", quickAction);
            message = ret.Message;
            return ret.Result;
        }

        public List<QuickAction> GetQuickAction(int userID)
        {
            return APIHelper.GetAPIResult<List<QuickAction>>(string.Format("/AdminSys/GetQuickAction?userID={0}", userID));
        }

        public QuickAction GetQuickActionByID(int id)
        {
            return APIHelper.GetAPIResult<QuickAction>(string.Format("/AdminSys/GetQuickActionByID?id={0}", id));
        }

        public bool DelQuickAction(int id, int userID, ref string message)
        {
            var ret = APIHelper.GetAPIResult<APIResult>(string.Format("/AdminSys/DelQuickAction?id={0}&userID={1}", id, userID));
            message = ret.Message;
            return ret.Result;
        }

        public bool ModifyQuickAction(Entity.QuickAction quickAction, ref string message)
        {
            var ret = APIHelper.PostAPIResult<QuickAction>("/AdminSys/ModifyQuickAction", quickAction);
            message = ret.Message;
            return ret.Result;
        }

        public QuickAction GetQuickActionByPage(int userID, string url)
        {
            return APIHelper.GetAPIResult<QuickAction>(string.Format("/AdminSys/GetQuickActionByPage?userID={0}&url={1}", userID, System.Web.HttpContext.Current.Server.UrlEncode(url)));
        }

        public bool ModifyUserInfo(int userID, string loginName, string displayName, string password, string phone, int? type, int? status, int? secretKey, ref string message)
        {
            if (!string.IsNullOrEmpty(password)) { password = [REDACTED]; }

            var ret = APIHelper.GetAPIResult<APIResult>(string.Format("/AdminSys/ModifyUserInfo?userID={0}&loginName={1}&displayName={2}&password=[REDACTED]", userID, loginName, displayName, password, phone, type, status, secretKey));
            message = ret.Message;
            return ret.Result;
        }

        public APIResult GetTGCode(int chatID, string tgCode)
        {
            var _message = string.Format("请输入电报验证码: {0}", tgCode);
            var _apiIP = ConfigurationManager.AppSettings["TGServerIP"];
            var _apiToken = [REDACTED]"TGServerToken"];
            var _url = string.Format("{0}?bottoken=[REDACTED]", _apiIP, _apiToken, chatID, _message);

            try
            {
                var _request = (HttpWebRequest)WebRequest.Create(_url);
                using (var response = (HttpWebResponse)_request.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        return new APIResult(false, "发送验证码发生错误");
                    }
                    return new APIResult(true, "发送成功");
                }
            }
            catch (Exception ex)
            {
                return new APIResult(false, "发送验证码发生例外错误");
            }

        }
    }
}
