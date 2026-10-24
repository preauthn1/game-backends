using FW.Core.Helper;
using game.Entity.AdminSys;
using game.Entity.Users;
using System;
using System.Collections.Generic;

namespace game.Service
{
    /// <summary>
    /// 系统服务
    /// </summary>
    public interface IAdminSysService
    {
        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool CreateUser(AdminUser user, ref string message);

        List<UserIdentity> GetUserTypeList();
        /// <summary>
        /// 更新用户登录信息
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="loginIPAddress"></param>
        /// <param name="loginTime"></param>
        void UpdateUserLoginInfo(int userID, string loginIPAddress, DateTime loginTime);

        /// <summary>
        /// 修改用户名
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="loginName"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ModifyUserLoginName(int userID, string loginName, string displayName, ref string message);

        /// <summary>
        /// 修改用户密码
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="password"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ModifyUserPassword(int userID, string password, ref string message);

        /// <summary>
        /// 修改用户手机号
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="phoneCode"></param>
        /// <param name="phoneCodeTime"></param>
        /// <returns></returns>
        bool ModifyUserPhone(int userID, string phone, ref string message);

        /// <summary>
        /// 修改用户手机验证码
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="phoneCode"></param>
        /// <param name="phoneCodeTime"></param>
        /// <returns></returns>
        bool ModifyUserPhoneCode(int userID, string phoneCode, DateTime phoneCodeTime, ref string message);

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool DeleteUser(int userID, ref string message);

        /// <summary>
        /// 根据用户ID获取用户
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        AdminUser GetUserByID(int userID);

        /// <summary>
        /// 根据用户名获取用户
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        AdminUser GetUserByLoginName(string loginName);

        /// <summary>
        /// 获取用户
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="loginName"></param>
        /// <param name="status"></param>
        /// <param name="lastLogonTimeBegin"></param>
        /// <param name="lastLogonTimeEnd"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<AdminUser> GetUser(int? userID, string loginName, byte? status, DateTime? lastLogonTimeBegin, DateTime? lastLogonTimeEnd, int pageIndex, int pageSize);

        /// <summary>
        /// 校验用户信息
        /// </summary>
        /// <param name="loginName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        AdminUser ValidateUserInfo(string loginName, string password);

        /// <summary>
        /// 添加操作记录
        /// </summary>
        /// <param name="log"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool AddAdminLog(AdminLog log, ref string message);

        /// <summary>
        /// 获取管理员操作记录
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="ipAddress"></param>
        /// <param name="timeBegin"></param>
        /// <param name="timeEnd"></param>
        /// <param name="memo"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<AdminLog> GetAdminLog(int? userID, string ipAddress, DateTime? timeBegin, DateTime? timeEnd, string memo, int pageIndex, int pageSize);

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool CreateRole(string roleName, ref string message);

        /// <summary>
        /// 修改角色
        /// </summary>
        /// <param name="role"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ModifyRole(Role role, ref string message);

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool DeleteRole(int roleID, ref string message);

        /// <summary>
        /// 根据角色ID获取角色信息
        /// </summary>
        /// <param name="roleID"></param>
        /// <returns></returns>
        Role GetRoleByID(int roleID);

        /// <summary>
        /// 获取角色
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        List<Role> GetRole(int? roleID, string roleName);

        /// <summary>
        /// 添加权限
        /// </summary>
        /// <param name="right"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool AddRight(Right right, ref string message);

        /// <summary>
        /// 修改权限
        /// </summary>
        /// <param name="right"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ModifyRight(Right right, ref string message);

        /// <summary>
        /// 删除权限
        /// </summary>
        /// <param name="rightID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool DeleteRight(int rightID, ref string message);

        /// <summary>
        /// 获取权限
        /// </summary>
        /// <param name="rightID"></param>
        /// <param name="parentID"></param>
        /// <param name="rightType"></param>
        /// <returns></returns>
        List<Right> GetRight(int? rightID, int? parentID, RightType? rightType);

        /// <summary>
        /// 根据ID获取权限
        /// </summary>
        /// <param name="rightID"></param>
        /// <returns></returns>
        Right GetRightByID(int rightID);

        /// <summary>
        /// 根据token获取权限
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Right GetRightByToken(string token);

        /// <summary>
        /// 添加角色权限
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="rightID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool AddRoleRight(int roleID, int rightID, ref string message);

        /// <summary>
        /// 移除角色权限
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="rightID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool RemoveRoleRight(int roleID, int? rightID, ref string message);

        /// <summary>
        /// 获取角色权限
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="rightID"></param>
        /// <returns></returns>
        List<RoleRight> GetRoleRight(int? roleID, int? rightID);

        /// <summary>
        /// 添加用户权限
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="rightID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool AddUserRight(int userID, int rightID, ref string message);

        /// <summary>
        /// 移除用户权限
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="rightID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool RemoveUserRight(int userID, int? rightID, ref string message);

        /// <summary>
        /// 获取用户权限
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="rightID"></param>
        /// <returns></returns>
        List<UserRight> GetUserRight(int? userID, int? rightID);

        /// <summary>
        /// 获取用户所有权限
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        List<Right> GetAllUserRight(int userID);

        /// <summary>
        /// 获取用户角色
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="roleID"></param>
        /// <returns></returns>
        List<UserRole> GetUserRole(int? userID, int? roleID);

        /// <summary>
        /// 添加用户角色
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="roleID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool AddUserRole(int userID, int roleID, ref string message);

        /// <summary>
        /// 移除用户角色
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="roleID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool RemoveUserRole(int userID, int? roleID, ref string message);

        /// <summary>
        /// 添加快捷操作
        /// </summary>
        /// <param name="quickAction"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool AddQuickAction(QuickAction quickAction, ref string message);

        /// <summary>
        /// 获取快捷操作
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        List<QuickAction> GetQuickAction(int userID);

        /// <summary>
        /// 根据ID获取快捷操作
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        QuickAction GetQuickActionByID(int id);

        /// <summary>
        /// 删除快捷操作
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool DelQuickAction(int id, int userID, ref string message);

        /// <summary>
        /// 修改快捷操作
        /// </summary>
        /// <param name="quickAction"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ModifyQuickAction(QuickAction quickAction, ref string message);

        /// <summary>
        /// 根据页面地址获取收快捷操作
        /// </summary>
        /// <param name="url"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        QuickAction GetQuickActionByPage(int userID, string url);

        /// <summary>
        /// 修改用戶資訊
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="loginName"></param>
        /// <param name="displayName"></param>
        /// <param name="password"></param>
        /// <param name="phone"></param>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ModifyUserInfo(int userID, string loginName, string displayName, string password, string phone, int? type, int? status, int? secretKey, ref string message);


        /// <summary>
        /// 更新域名设定
        /// </summary>
        /// <returns></returns>
        DomainConfig GetDomainConfig();

        DomainConfig GetDomainConfigByID(int id);

        bool UpdateDomainConfig(string payment, string update, string club, string customer, int company, ref string message);
    }
}
