using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.AdminSys;
using game.Entity.Users;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace game.Service
{
    public class AdminSysService : IAdminSysService
    {
        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFactory;

        public AdminSysService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbFactory;
        }

        public List<UserIdentity> GetUserTypeList()
        {
            var list = _dbFactory.GetDatabase(Infrastructure.DBNameConfig.DBName_AdminSys).ExecuteStoreProcedureForDataList<UserIdentity>("[dbo].[PrWeb_UserType_Get]").ToList();
            return list;
        }

        public bool CreateUser(AdminUser user, ref string message)
        {
            SqlParameter[] _paras = {
                                    new SqlParameter("@chvUserName",user.LoginName),
                                    new SqlParameter("@chvUserPW",user.Password),
                                    new SqlParameter("@tnyUserType",user.UserType),
                                    new SqlParameter("@chvDisplayName",user.DisplayName),
                                    new SqlParameter("@chvIPAddress",user.LastLogonIPAddress),
                                    new SqlParameter("@intTgchatid", user.TGChatid),
                                    new SqlParameter("@intSecretKey",user.SecretKey),
                                    new SqlParameter("@intRtnCode",0),
                                   };
            var _returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_AdminSys).ExecuteStoreProcedureForResult("[dbo].[PrWeb_CreateAdminUser]", _paras);

            #region 錯誤判斷

            if (_returnValue == -1)
            {
                message = "用户名已经存在";
                return false;
            }

            if (_returnValue == -2)
            {
                message = "创建SP發生錯誤";
                return false;
            }
            #endregion

            message = "创建成功";
            return true;
        }

        public void UpdateUserLoginInfo(int userID, string loginIPAddress, DateTime loginTime)
        {
            var _r = _repoFactory.CreateRepository<AdminUser>();

            var dbuser = _r.GetById(userID);

            dbuser.LastLogonIPAddress = loginIPAddress;
            dbuser.LastLogonTime = loginTime;

            _r.Update(dbuser);
        }

        public bool ModifyUserLoginName(int userID, string loginName, string displayName, ref string message)
        {
            var _r = _repoFactory.CreateRepository<AdminUser>();

            var user = _r.GetById(userID);

            if (user == null)
            {
                message = "用户不存在";
                return false;
            }
            else
            {
                if (!string.IsNullOrEmpty(loginName)) { user.LoginName = loginName; }
                if (!string.IsNullOrEmpty(displayName)) { user.DisplayName = displayName; }
                _r.Update(user);

                message = "修改成功";
                return true;
            }
        }

        public bool ModifyUserPassword(int userID, string password, ref string message)
        {
            var _r = _repoFactory.CreateRepository<AdminUser>();

            var user = _r.GetById(userID);

            if (user == null)
            {
                message = "用户不存在";
                return false;
            }
            else
            {
                user.Password = [REDACTED];
                _r.Update(user);

                message = "修改成功";
                return true;
            }
        }

        public bool ModifyUserPhone(int userID, string phone, ref string message)
        {
            var _r = _repoFactory.CreateRepository<AdminUser>();

            var user = _r.GetById(userID);

            if (user == null)
            {
                message = "用户不存在";
                return false;
            }
            else
            {
                user.Phone = phone;
                _r.Update(user);

                message = "修改成功";
                return true;
            }
        }

        public bool ModifyUserPhoneCode(int userID, string phoneCode, DateTime phoneCodeTime, ref string message)
        {
            var _r = _repoFactory.CreateRepository<AdminUser>();

            var user = _r.GetById(userID);

            if (user == null)
            {
                message = "用户不存在";
                return false;
            }
            else
            {
                user.PhoneCode = phoneCode;
                user.PhoneCodeTime = phoneCodeTime;
                _r.Update(user);

                message = "修改成功";
                return true;
            }
        }

        public bool ModifyUserInfo(int userID, string loginName, string displayName, string password, string phone, int? userType, int? status, int? secretKey, ref string message)
        {
            var _r = _repoFactory.CreateRepository<AdminUser>();
            var user = _r.GetById(userID);

            if (user == null)
            {
                message = "用户不存在";
                return false;
            }
            else
            {
                if (!string.IsNullOrEmpty(loginName)) { user.LoginName = loginName; }
                if (!string.IsNullOrEmpty(displayName)) { user.DisplayName = displayName; }
                if (!string.IsNullOrEmpty(password)) { user.Password = [REDACTED]; }
                if (!string.IsNullOrEmpty(phone)) { user.Phone = phone; }

                if (status.HasValue) { user.Status = Convert.ToByte(status); }
                if (secretKey.HasValue) { user.SecretKey = secretKey.Value; }
                if (userType.HasValue) { user.UserType = Convert.ToByte(userType); }

                _r.Update(user);

                message = "修改成功";
                return true;
            }
        }

        public bool DeleteUser(int userID, ref string message)
        {
            var _r = _repoFactory.CreateRepository<AdminUser>();
            var _role = _repoFactory.CreateRepository<UserRole>();
            var _right = _repoFactory.CreateRepository<UserRight>();

            var user = _r.GetById(userID);
            var userRole = _role.Entities.Where(m => m.UserID == userID).ToList();
            var userRight = _right.Entities.Where(c => c.UserID == userID).ToList();

            if (user == null)
            {
                message = "用户不存在";
                return false;
            }
            else
            {
                _r.Delete(user);

                foreach (var right in userRole)
                {
                    _role.Delete(right);
                }

                foreach (var right in userRight)
                {
                    _right.Delete(right);
                }

                message = "删除成功";
                return true;
            }
        }

        public AdminUser GetUserByID(int userID)
        {
            return _repoFactory.CreateRepository<AdminUser>().GetById(userID);
        }

        public AdminUser GetUserByLoginName(string loginName)
        {
            return _repoFactory.CreateRepository<AdminUser>().Entities.FirstOrDefault(m => m.LoginName == loginName);
        }

        public PagedList<AdminUser> GetUser(int? userID, string loginName, byte? status, DateTime? lastLogonTimeBegin, DateTime? lastLogonTimeEnd, int pageIndex, int pageSize)
        {
            var listUser = _repoFactory.CreateRepository<AdminUser>().Entities.Where(
                c => (userID == null || c.Id == userID)
                    && (loginName == null || loginName == "" || c.LoginName == loginName)
                    && (status == null || c.Status == status)
                    && (lastLogonTimeBegin == null || c.LastLogonTime >= lastLogonTimeBegin)
                    && (lastLogonTimeEnd == null || c.LastLogonTime <= lastLogonTimeEnd))
                    .OrderByDescending(u => u.Id);

            PagedList<AdminUser> result = new PagedList<AdminUser>(listUser, pageIndex, pageSize);

            return result;
        }

        public AdminUser ValidateUserInfo(string loginName, string password)
        {
            return _repoFactory.CreateRepository<AdminUser>().Entities.FirstOrDefault(c => c.LoginName == loginName && c.Password == password);
        }

        public bool AddAdminLog(AdminLog log, ref string message)
        {
            _repoFactory.CreateRepository<AdminLog>().Insert(log);
            return true;
        }

        public PagedList<AdminLog> GetAdminLog(int? userID, string ipAddress, DateTime? timeBegin, DateTime? timeEnd, string memo, int pageIndex, int pageSize)
        {
            var listLog = _repoFactory.CreateRepository<AdminLog>().Entities.Where(
                c => (userID == null || c.UserID == userID)
                    && (ipAddress == null || ipAddress == "" || c.IPAddress == ipAddress)
                    && (timeBegin == null || c.WriteTime >= timeBegin)
                    && (timeEnd == null || c.WriteTime <= timeEnd)
                    && (memo == null || memo == "" || c.Memo.Contains(memo)))
                    .OrderByDescending(l => l.Id);

            PagedList<AdminLog> result = new PagedList<AdminLog>(listLog, pageIndex, pageSize);

            return result;
        }

        public bool CreateRole(string roleName, ref string message)
        {
            var _r = _repoFactory.CreateRepository<Role>();

            if (_r.Entities.FirstOrDefault(c => c.RoleName == roleName) != null)
            {
                message = "角色已经存在";
                return false;
            }

            var role = new Role();
            role.RoleName = roleName;

            _r.Insert(role);

            message = "添加成功";
            return true;
        }

        public bool ModifyRole(Role role, ref string message)
        {
            var _r = _repoFactory.CreateRepository<Role>();

            var oldRole = _r.GetById(role.Id);
            if (oldRole == null)
            {
                message = "角色不存在";
                return false;
            }
            else
            {
                oldRole.RoleName = role.RoleName;
                _r.Update(oldRole);

                message = "修改成功";
                return true;
            }
        }

        public bool DeleteRole(int roleID, ref string message)
        {
            var _r = _repoFactory.CreateRepository<Role>();

            var role = _r.GetById(roleID);

            if (role == null)
            {
                message = "角色不存在";
                return false;
            }
            else
            {
                _r.Delete(role);

                message = "删除成功";
                return true;
            }
        }

        public Role GetRoleByID(int roleID)
        {
            return _repoFactory.CreateRepository<Role>().GetById(roleID);
        }

        public List<Role> GetRole(int? roleID, string roleName)
        {
            return _repoFactory.CreateRepository<Role>().Entities.Where(
                c => (roleID == null || c.Id == roleID)
                    && (roleName == "" || roleName == null || c.RoleName.Contains(roleName)))
                    .OrderByDescending(r => r.Id)
                    .ToList();
        }

        public bool AddRight(Right right, ref string message)
        {
            var _r = _repoFactory.CreateRepository<Right>();

            if (_r.Entities.FirstOrDefault(c => c.RightToken == right.RightToken) != null)
            {
                message = "该权限已经存在";
                return false;
            }

            _r.Insert(right);

            message = "添加成功";
            return true;
        }

        public bool ModifyRight(Right right, ref string message)
        {
            var _r = _repoFactory.CreateRepository<Right>();

            if (_r.Entities.FirstOrDefault(c => c.RightToken == right.RightToken && c.Id != right.Id) != null)
            {
                message = "该权限已经存在";
                return false;
            }

            var exRight = _r.GetById(right.Id);
            exRight.RightName = right.RightName;
            //exRight.ParentID = right.ParentID;
            exRight.RightToken = [REDACTED];
            exRight.PageUrl = right.PageUrl;
            exRight.Sort = right.Sort;
            exRight.ShowInMenu = right.ShowInMenu;
            exRight.IconCls = right.IconCls;
            exRight.RightType = right.RightType;

            _r.Update(exRight);

            message = "修改成功";
            return true;
        }

        public bool DeleteRight(int rightID, ref string message)
        {
            var _r = _repoFactory.CreateRepository<Right>();
            var oldRight = _r.GetById(rightID);

            if (oldRight == null)
            {
                message = "权限不存在";
                return false;
            }
            else
            {
                SqlParameter[] paras = {
                   new SqlParameter("@intRightID",rightID)
                };

                _dbFactory.GetDatabase(Infrastructure.DBNameConfig.DBName_AdminSys).ExecuteStoreProcedureForResult("dbo.PrWeb_Right_Delete", paras);

                message = "删除成功";
                return true;
            }
        }

        public List<Right> GetRight(int? rightID, int? parentID, RightType? rightType)
        {
            return _repoFactory.CreateRepository<Right>().Entities.Where(
                c => (rightID == null || c.Id == rightID)
                    && (parentID == null || c.ParentID == parentID)
                    && (rightType == null || c.RightType == rightType))
                    .OrderByDescending(c => c.Sort)
                    .ThenByDescending(c => c.Id)
                    .ToList();
        }

        public Right GetRightByID(int rightID)
        {
            return _repoFactory.CreateRepository<Right>().GetById(rightID);
        }

        public Right GetRightByToken(string token)
        {
            return _repoFactory.CreateRepository<Right>().Entities.FirstOrDefault(r => r.RightToken == token);
        }

        public bool AddRoleRight(int roleID, int rightID, ref string message)
        {
            var _r = _repoFactory.CreateRepository<RoleRight>();
            var oldRoleRight = _r.Entities.FirstOrDefault(c => c.RoleID == roleID && c.RightID == rightID);

            if (oldRoleRight != null)
            {
                message = "该角色权限已经存在";
                return false;
            }

            RoleRight roleRight = new RoleRight();
            roleRight.RoleID = roleID;
            roleRight.RightID = rightID;

            _r.Insert(roleRight);

            message = "添加成功";
            return true;
        }

        public bool RemoveRoleRight(int roleID, int? rightID, ref string message)
        {
            var _r = _repoFactory.CreateRepository<RoleRight>();
            if (rightID != null)
            {
                var oldRoleRight = _r.Entities.FirstOrDefault(c => c.RoleID == roleID && c.RightID == rightID.Value);

                if (oldRoleRight == null)
                {
                    message = "该角色权限不存在";
                    return false;
                }

                _r.Delete(oldRoleRight);

                message = "删除成功";
                return true;
            }
            else
            {
                var rights = _r.Entities.Where(c => c.RoleID == roleID).ToList();
                rights.ForEach(rr =>
                {
                    _r.Delete(rr);
                });

                message = "删除成功";
                return true;
            }
        }

        public List<RoleRight> GetRoleRight(int? roleID, int? rightID)
        {
            return _repoFactory.CreateRepository<RoleRight>().Entities.Where(
                c => (roleID == null || c.RoleID == roleID)
                    && (rightID == null || c.RightID == rightID))
                    .ToList();
        }

        public bool AddUserRight(int userID, int rightID, ref string message)
        {
            var _r = _repoFactory.CreateRepository<UserRight>();
            var oldUserRight = _r.Entities.FirstOrDefault(c => c.UserID == userID && c.RightID == rightID);

            if (oldUserRight != null)
            {
                message = "该用户权限已经存在";
                return false;
            }

            UserRight userRight = new UserRight();
            userRight.UserID = userID;
            userRight.RightID = rightID;

            _r.Insert(userRight);

            message = "添加成功";
            return true;
        }

        public bool RemoveUserRight(int userID, int? rightID, ref string message)
        {
            var _r = _repoFactory.CreateRepository<UserRight>();
            if (rightID != null)
            {
                var oldUserRight = _r.Entities.FirstOrDefault(c => c.UserID == userID && c.RightID == rightID.Value);

                if (oldUserRight == null)
                {
                    message = "该用户权限不存在";
                    return false;
                }

                _r.Delete(oldUserRight);

                message = "删除成功";
                return true;
            }
            else
            {
                var urs = _r.Entities.Where(ur => ur.UserID == userID).ToList();
                urs.ForEach(ur =>
                {
                    _r.Delete(ur);
                });

                message = "操作成功";
                return true;
            }
        }

        public List<UserRight> GetUserRight(int? userID, int? rightID)
        {
            return _repoFactory.CreateRepository<UserRight>().Entities.Where(
                c => (userID == null || c.UserID == userID)
                    && (rightID == null || c.RightID == rightID))
                    .ToList();
        }

        public List<Right> GetAllUserRight(int userID)
        {
            List<Right> rights = new List<Right>();

            var urts = _repoFactory.CreateRepository<UserRight>().Entities.Where(ur => ur.UserID == userID)
                .Select(ur => ur.Right)
                .ToList();

            rights.AddRange(urts);

            var urrs = _repoFactory.CreateRepository<UserRole>().Entities.Where(ur => ur.UserID == userID)
                .Select(ur => ur.Role)
                .ToList();

            urrs.ForEach(rr =>
            {
                _repoFactory.CreateRepository<RoleRight>().Entities.Where(r => r.RoleID == rr.Id)
                    .Select(r => r.Right)
                    .ToList().ForEach(
                    r =>
                    {
                        if (rights.Where(rt => rt.Id == r.Id).Count() == 0)
                        {
                            rights.Add(r);
                        }
                    }
                    );
            });

            return rights;
        }

        public List<UserRole> GetUserRole(int? userID, int? roleID)
        {
            return _repoFactory.CreateRepository<UserRole>().Entities.Where(
                ur => (userID == null || userID.Value == ur.UserID)
                    && (roleID == null || roleID.Value == ur.RoleID))
                    .ToList();
        }

        public bool AddUserRole(int userID, int roleID, ref string message)
        {
            if (GetUserRole(userID, roleID).Count > 0)
            {
                return true;
            }

            var ur = new UserRole();
            ur.UserID = userID;
            ur.RoleID = roleID;

            _repoFactory.CreateRepository<UserRole>().Insert(ur);

            message = "操作成功";
            return true;
        }

        public bool RemoveUserRole(int userID, int? roleID, ref string message)
        {
            var _r = _repoFactory.CreateRepository<UserRole>();
            if (roleID != null)
            {
                var ur = _r.Entities.FirstOrDefault(m => m.UserID == userID && (roleID == null || roleID.Value == m.RoleID));
                if (ur == null)
                {
                    message = "用户角色不存在";
                    return false;
                }

                _r.Delete(ur);

                message = "操作成功";
                return false;
            }
            else
            {
                var urs = _r.Entities.Where(r => r.UserID == userID).ToList();
                urs.ForEach(ur =>
                {
                    _r.Delete(ur);
                });

                message = "操作成功";
                return true;
            }
        }

        public bool AddQuickAction(QuickAction quickAction, ref string message)
        {
            _repoFactory.CreateRepository<QuickAction>().Insert(quickAction);

            message = "操作成功";
            return true;
        }

        public List<QuickAction> GetQuickAction(int userID)
        {
            return _repoFactory.CreateRepository<QuickAction>().Entities.
                Where(qa => qa.UserID == userID)
                .OrderByDescending(qa => qa.Sort)
                .ToList();
        }

        public QuickAction GetQuickActionByID(int id)
        {
            return _repoFactory.CreateRepository<QuickAction>().GetById(id);
        }

        public bool DelQuickAction(int id, int userID, ref string message)
        {
            var _r = _repoFactory.CreateRepository<QuickAction>();
            var exqa = _r.Entities.FirstOrDefault(a => a.Id == id && a.UserID == userID);
            if (exqa == null)
            {
                _r.Delete(exqa);

                message = "操作失败：数据不存在";
                return false;
            }

            message = "操作成功";
            return true;
        }

        public bool ModifyQuickAction(QuickAction quickAction, ref string message)
        {
            var _r = _repoFactory.CreateRepository<QuickAction>();
            var exqa = _r.GetById(quickAction.Id);
            if (exqa == null)
            {
                _r.Delete(exqa);

                message = "操作失败：数据不存在";
                return false;
            }

            exqa.Name = quickAction.Name;
            exqa.Sort = quickAction.Sort;
            exqa.Url = quickAction.Url;

            _r.Update(exqa);

            message = "操作成功";
            return true;
        }

        public QuickAction GetQuickActionByPage(int userID, string url)
        {
            return _repoFactory.CreateRepository<QuickAction>().Entities.FirstOrDefault(q => q.Url == url && q.UserID == userID);
        }

        public DomainConfig GetDomainConfig()
        {
            return _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteSqlQuery<DomainConfig>("SELECT TOP 1 * FROM [CenterDB].[ServerInfo].[DomainConfig]").FirstOrDefault();
        }

        public DomainConfig GetDomainConfigByID(int id)
        {
            SqlParameter[] paras = {
                   new SqlParameter("@intCompany",id)
                };

            return _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteSqlQuery<DomainConfig>("SELECT TOP 1 * FROM [CenterDB].[ServerInfo].[DomainConfig] WHERE Company=@intCompany", paras).FirstOrDefault();
        }

        public bool UpdateDomainConfig(string payment, string update, string club, string customer, int company, ref string message)
        {

            SqlParameter[] paras = {
                   new SqlParameter("@chvPayment",payment),
                   new SqlParameter("@chvDLUpdate",update),
                   new SqlParameter("@chvClub",club),
                   new SqlParameter("@chvCustomer",customer),
                   new SqlParameter("@intCompany",company)
                };

            _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_DomainConfig_Update", paras);

            message = "删除成功";
            return true;

        }
    }
}
