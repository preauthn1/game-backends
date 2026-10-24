using FW.Core.Helper;
using game.Infrastructure;
using game.Service;
using game.Entity.AdminSys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using game.Entity.Users;
using game.Entity;

namespace game.GameAPI.Controllers
{
    public class AdminSysController : BaseAPIController
    {
        private IAdminSysService _adminSysService;
        public AdminSysController(IAdminSysService service)
        {
            _adminSysService = service;
        }

        [HttpGet]
        public List<UserIdentity> GetUserTypeList()
        {
            string message = string.Empty;
            return _adminSysService.GetUserTypeList();
        }

        [HttpPost]
        public APIResult CreateUser(AdminUser user)
        {
            string message = "";
            bool result = _adminSysService.CreateUser(user, ref message);
            return new APIResult(result, message, user.Id);
        }

        [HttpGet]
        public void UpdateUserLoginInfo(int userID, string loginIPAddress, DateTime loginTime)
        {
            _adminSysService.UpdateUserLoginInfo(userID, loginIPAddress, loginTime);
        }

        [HttpGet]
        public APIResult ModifyUserLoginName(int userID, string loginName, string displayName)
        {
            string message = "";
            bool result = _adminSysService.ModifyUserLoginName(userID, loginName, displayName, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult ModifyUserPassword(int userID, string password)
        {
            string message = "";
            bool result = _adminSysService.ModifyUserPassword(userID, password, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult ModifyUserPhone(int userID, string phone)
        {
            string message = "";
            bool result = _adminSysService.ModifyUserPhone(userID, phone, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult ModifyUserPhoneCode(int userID, string phoneCode, DateTime phoneCodeTime)
        {
            string message = "";
            bool result = _adminSysService.ModifyUserPhoneCode(userID, phoneCode, phoneCodeTime, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult DeleteUser(int userID)
        {
            string message = "";
            bool result = _adminSysService.DeleteUser(userID, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public AdminUser GetUserByID(int userID)
        {
            return _adminSysService.GetUserByID(userID);
        }

        [HttpGet]
        public List<AdminUser> GetUsersByOrgID(int orgId)
        {
            return _adminSysService.GetUsersByOrgID(orgId);
        }
        
        [HttpGet]
        public PagedList<AdminUser> GetUsersPagingByOrgID([FromUri]QueryAdminUserRequestViewModel flter)
        {
            return _adminSysService.GetUsersPagingByOrgID(flter);
        }

        [HttpGet]
        public AdminUser GetUserByLoginName(string loginName)
        {
            return _adminSysService.GetUserByLoginName(loginName);
        }

        [HttpGet]
        public PagedList<AdminUser> GetUser(int? userID, string loginName, byte? status, DateTime? lastLogonTimeBegin, DateTime? lastLogonTimeEnd, int pageIndex, int pageSize)
        {
            return _adminSysService.GetUser(userID, loginName, status, lastLogonTimeBegin, lastLogonTimeEnd, pageIndex, pageSize);
        }

        [HttpGet]
        public AdminUser ValidateUserInfo(string loginName, string password)
        {
            return _adminSysService.ValidateUserInfo(loginName, password);
        }

        [HttpPost]
        public APIResult AddAdminLog(AdminLog log)
        {
            string message = "";
            bool result = _adminSysService.AddAdminLog(log, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public PagedList<AdminLog> GetAdminLog(int? userID, string ipAddress, DateTime? timeBegin, DateTime? timeEnd, string memo, int pageIndex, int pageSize)
        {
            return _adminSysService.GetAdminLog(userID, ipAddress, timeBegin, timeEnd, memo, pageIndex, pageSize);
        }

        [HttpGet]
        public APIResult CreateRole(string roleName)
        {
            string message = "";
            bool result = _adminSysService.CreateRole(roleName, ref message);
            return new APIResult(result, message);
        }

        [HttpPost]
        public APIResult ModifyRole(Role role)
        {
            string message = "";
            bool result = _adminSysService.ModifyRole(role, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult DeleteRole(int roleID)
        {
            string message = "";
            bool result = _adminSysService.DeleteRole(roleID, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public Role GetRoleByID(int roleID)
        {
            return _adminSysService.GetRoleByID(roleID);
        }

        [HttpGet]
        public List<Role> GetRole(int? roleID, string roleName)
        {
            return _adminSysService.GetRole(roleID, roleName);
        }

        [HttpPost]
        public APIResult AddRight(Right right)
        {
            string message = "";
            bool result = _adminSysService.AddRight(right, ref message);
            return new APIResult(result, message);
        }

        [HttpPost]
        public APIResult ModifyRight(Right right)
        {
            string message = "";
            bool result = _adminSysService.ModifyRight(right, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult DeleteRight(int rightID)
        {
            string message = "";
            bool result = _adminSysService.DeleteRight(rightID, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public List<Right> GetRight(int? rightID, int? parentID, RightType? rightType)
        {
            return _adminSysService.GetRight(rightID, parentID, rightType);
        }

        [HttpGet]
        public Right GetRightByID(int rightID)
        {
            return _adminSysService.GetRightByID(rightID);
        }

        [HttpGet]
        public Right GetRightByToken(string token)
        {
            return _adminSysService.GetRightByToken(token);
        }

        [HttpGet]
        public APIResult AddRoleRight(int roleID, int rightID)
        {
            string message = "";
            bool result = _adminSysService.AddRoleRight(roleID, rightID, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult RemoveRoleRight(int roleID, int? rightID)
        {
            string message = "";
            bool result = _adminSysService.RemoveRoleRight(roleID, rightID, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public List<RoleRight> GetRoleRight(int? roleID, int? rightID)
        {
            return _adminSysService.GetRoleRight(roleID, rightID);
        }

        [HttpGet]
        public APIResult AddUserRight(int userID, int rightID)
        {
            string message = "";
            bool result = _adminSysService.AddUserRight(userID, rightID, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult RemoveUserRight(int userID, int? rightID)
        {
            string message = "";
            bool result = _adminSysService.RemoveUserRight(userID, rightID, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public List<UserRight> GetUserRight(int? userID, int? rightID)
        {
            return _adminSysService.GetUserRight(userID, rightID);
        }

        [HttpGet]
        public List<Right> GetAllUserRight(int userID)
        {
            return _adminSysService.GetAllUserRight(userID);
        }

        [HttpGet]
        public List<UserRole> GetUserRole(int? userID, int? roleID)
        {
            return _adminSysService.GetUserRole(userID, roleID);
        }

        [HttpGet]
        public APIResult AddUserRole(int userID, int roleID)
        {
            string message = "";
            bool result = _adminSysService.AddUserRole(userID, roleID, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult RemoveUserRole(int userID, int? roleID)
        {
            string message = "";
            bool result = _adminSysService.RemoveUserRole(userID, roleID, ref message);
            return new APIResult(result, message);
        }

        [HttpPost]
        public APIResult AddQuickAction(QuickAction quickAction)
        {
            string message = "";
            bool result = _adminSysService.AddQuickAction(quickAction, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public List<QuickAction> GetQuickAction(int userID)
        {
            return _adminSysService.GetQuickAction(userID);
        }

        [HttpGet]
        public QuickAction GetQuickActionByID(int id)
        {
            return _adminSysService.GetQuickActionByID(id);
        }

        [HttpGet]
        public APIResult DelQuickAction(int id, int userID)
        {
            string message = "";
            bool result = _adminSysService.DelQuickAction(id, userID, ref message);
            return new APIResult(result, message);
        }

        [HttpPost]
        public APIResult ModifyQuickAction(QuickAction quickAction)
        {
            string message = "";
            bool result = _adminSysService.ModifyQuickAction(quickAction, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public QuickAction GetQuickActionByPage(int userID, string url)
        {
            return _adminSysService.GetQuickActionByPage(userID, url);
        }

        [HttpGet]
        public APIResult ModifyUserInfo(int userID, string loginName, string displayName, string password, string phone, int? userType, int? status, int? secretKey)
        {
            string message = "";
            bool result = _adminSysService.ModifyUserInfo(userID, loginName, displayName, password, phone, userType, status, secretKey, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public DomainConfig GetDomainConfig()
        {
            var result = _adminSysService.GetDomainConfig();
            return result;
        }

        [HttpGet]
        public DomainConfig GetDomainConfigByID(int id)
        {
            var result = _adminSysService.GetDomainConfigByID(id);
            return result;
        }

        [HttpGet]
        public APIResult UpdateDomainConfig(string payment, string update, string club, string customer, int company)
        {
            string message = "";
            var result = _adminSysService.UpdateDomainConfig(payment, update, club, customer, company, ref message);
            return new APIResult(result, message);
        }

        
    }
}