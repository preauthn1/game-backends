using System;
using System.Collections.Generic;
using System.Linq;
using FW.Core.AdminSys.Provider.Entity;
using FW.Core.Cache;

namespace FW.Core.AdminSys.Provider.EFProvider
{
    public class EFAdminSysProvider : IAdminSysProvider
    {
        private ICacheManager _cacheManager;

        public EFAdminSysProvider(ICacheManager cacheMamager)
        {
            _cacheManager = cacheMamager;
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

        public bool CreateUser(Entity.AdminUser user, ref string message)
        {
            using (var dbContext = new AdminDbContext())
            {
                if (dbContext.Set<AdminUser>().Where(u => u.LoginName == user.LoginName).FirstOrDefault() != null)
                {
                    message = "用户名已经存在";
                    return false;
                }

                user.Password = [REDACTED];

                dbContext.Set<AdminUser>().Add(user);
                dbContext.SaveChanges();

                message = "创建成功";

                return true;
            }
        }
        public void UpdateUserLoginInfo(AdminUser user)
        {
            using (var dbContext = new AdminDbContext())
            {
                var dbuser = dbContext.Set<AdminUser>().Where(u => u.UserID == user.UserID).FirstOrDefault();
                dbuser.LastLogonIPAddress = user.LastLogonIPAddress;
                dbuser.LastLogonTime = user.LastLogonTime;
                dbContext.SaveChanges();
            }
        }

        public bool ModifyUser(AdminUser user, int type, ref string message)
        {
            using (var dbContext = new AdminDbContext())
            {
                var users = dbContext.Set<AdminUser>().Where(u => u.UserID == user.UserID).FirstOrDefault();

                if (users == null)
                {
                    message = "用户名不存在";
                    return false;
                }
                else
                {
                    dbContext.SaveChanges();
                    message = "修改成功";
                    return true;
                }
            }
        }

        public bool ModifyUserLoginName(int userID, string loginName, string displayName, ref string message)
        {
            using (var dbContext = new AdminDbContext())
            {
                var user = dbContext.Set<AdminUser>().Where(u => u.UserID == userID).FirstOrDefault();

                if (user == null)
                {
                    message = "用户名不存在";
                    return false;
                }
                else
                {
                    if (!string.IsNullOrEmpty(loginName)) { user.LoginName = loginName; }
                    if (!string.IsNullOrEmpty(displayName)) { user.DisplayName = displayName; }
                    dbContext.SaveChanges();

                    message = "修改成功";
                    return true;
                }
            }

        }

        public bool ModifyUserPassword(int userID, string password, ref string message)
        {
            using (var dbContext = new AdminDbContext())
            {
                var user = dbContext.Set<AdminUser>().Where(u => u.UserID == userID).FirstOrDefault();

                if (user == null)
                {
                    message = "用户名不存在";
                    return false;
                }
                else
                {
                    password = [REDACTED];
                    user.Password = [REDACTED];
                    dbContext.SaveChanges();

                    message = "修改成功";
                    return true;
                }
            }
        }

        public bool DeleteUser(int userID, ref string message)
        {
            using (var dbContext = new AdminDbContext())
            {
                var user = dbContext.Set<AdminUser>().Where(u => u.UserID == userID).FirstOrDefault();

                if (user == null)
                {
                    message = "用户名不存在";
                    return false;
                }
                else
                {
                    dbContext.Set<AdminUser>().Remove(user);
                    dbContext.SaveChanges();

                    message = "删除成功";
                    return true;
                }
            }
        }

        public Entity.AdminUser GetUserByID(int userID)
        {
            var dbContext = new AdminDbContext();
            return dbContext.Set<AdminUser>().Where(u => u.UserID == userID).FirstOrDefault();
        }

        public Entity.AdminUser GetUserByLoginName(string loginName)
        {
            var dbContext = new AdminDbContext();
            return dbContext.Set<AdminUser>().Where(u => u.LoginName == loginName).FirstOrDefault();
        }

        public Helper.PagedList<Entity.AdminUser> GetUser(int? userID, string loginName, byte? status, DateTime? lastLogonTimeBegin, DateTime? lastLogonTimeEnd, int pageIndex, int pageSize)
        {
            var dbContext = new AdminDbContext();
            var listUser = dbContext.Set<AdminUser>().Where(
                                                   c => (userID == null || c.UserID == userID)
                                                && (loginName == null || loginName == "" || c.LoginName == loginName)
                                                && (status == null || c.Status == status)
                                                && (lastLogonTimeBegin == null || c.LastLogonTime >= lastLogonTimeBegin)
                                                && (lastLogonTimeEnd == null || c.LastLogonTime <= lastLogonTimeEnd)
                                                )
                                                .OrderByDescending(u => u.UserID);

            Helper.PagedList<AdminUser> result = new Helper.PagedList<AdminUser>(listUser, pageIndex, pageSize);

            return result;

        }

        public AdminUser ValidateUserInfo(string loginName, string password, ref string message)
        {
            password = [REDACTED];
            var dbContext = new AdminDbContext();
            var user = dbContext.Set<AdminUser>().Where(c => c.LoginName == loginName && c.Password == password).FirstOrDefault();

            if (user == null)
            {
                message = "账号或密码错误";
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
        }


        public bool AddAdminLog(Entity.AdminLog log, ref string message)
        {
            using (var dbContext = new AdminDbContext())
            {
                dbContext.Set<AdminLog>().Add(log);
                dbContext.SaveChanges();

                return true;
            }
        }

        public Helper.PagedList<Entity.AdminLog> GetAdminLog(int? userID, string ipAddress, DateTime? timeBegin, DateTime? timeEnd, string memo, int pageIndex, int pageSize)
        {
            var dbContext = new AdminDbContext();
            var listLog = dbContext.Set<AdminLog>().Where(
                                                   c => (userID == null || c.UserID == userID)
                                                && (ipAddress == null || ipAddress == "" || c.IPAddress == ipAddress)
                                                && (timeBegin == null || c.WriteTime >= timeBegin)
                                                && (timeEnd == null || c.WriteTime <= timeEnd)
                                                && (memo == null || memo == "" || c.Memo.Contains(memo))
                                                ).OrderByDescending(l => l.Id);

            Helper.PagedList<AdminLog> result = new Helper.PagedList<AdminLog>(listLog, pageIndex, pageSize);

            return result;
        }

        public bool CreateRole(string roleName, ref string message)
        {
            using (var dbContext = new AdminDbContext())
            {
                if (dbContext.Set<Role>().Where(c => c.RoleName == roleName).FirstOrDefault() != null)
                {
                    message = "用户已经存在";
                    return false;
                }

                Role role = new Entity.Role();
                role.RoleName = roleName;

                dbContext.Set<Role>().Add(role);
                dbContext.SaveChanges();

                message = "添加成功";
                return true;
            }
        }

        public bool ModifyRole(Entity.Role role, ref string message)
        {
            using (var dbContext = new AdminDbContext())
            {
                var oldRole = dbContext.Set<Role>().Where(c => c.RoleID == role.RoleID).FirstOrDefault();

                if (oldRole == null)
                {
                    message = "角色不存在";
                    return false;
                }
                else
                {
                    oldRole.RoleName = role.RoleName;
                    dbContext.SaveChanges();

                    message = "修改成功";
                    return true;
                }
            }
        }

        public bool DeleteRole(int roleID, ref string message)
        {
            using (var dbContext = new AdminDbContext())
            {
                var oldRole = dbContext.Set<Role>().Where(c => c.RoleID == roleID).FirstOrDefault();

                if (oldRole == null)
                {
                    message = "角色不存在";
                    return false;
                }
                else
                {
                    dbContext.Set<Role>().Remove(oldRole);
                    dbContext.SaveChanges();

                    message = "删除成功";
                    return true;
                }
            }
        }

        public Entity.Role GetRoleByID(int roleID)
        {
            var dbContext = new AdminDbContext();
            return dbContext.Set<Role>().Where(c => c.RoleID == roleID).FirstOrDefault();
        }

        public List<Entity.Role> GetRole(int? roleID, string roleName)
        {
            var dbContext = new AdminDbContext();
            return dbContext.Set<Role>().Where(c => (roleID == null || c.RoleID == roleID)
                && (roleName == "" || roleName == null || c.RoleName.Contains(roleName)))
                .OrderByDescending(r => r.RoleID)
                .ToList();
        }

        public bool AddRight(Entity.Right right, ref string message)
        {
            using (var dbContext = new AdminDbContext())
            {
                if (dbContext.Set<Right>().Where(c => c.RightToken == right.RightToken).FirstOrDefault() != null)
                {
                    message = "该权限已经存在";
                    return false;
                }

                dbContext.Set<Right>().Add(right);
                dbContext.SaveChanges();

                message = "添加成功";
                return true;
            }
        }

        public bool ModifyRight(Right right, ref string message)
        {
            using (var dbContext = new AdminDbContext())
            {
                if (dbContext.Set<Right>().Where(c => c.RightToken == right.RightToken && c.RightID != right.RightID).FirstOrDefault() != null)
                {
                    message = "该权限已经存在";
                    return false;
                }

                var exRight = dbContext.Set<Right>().Where(r => r.RightID == right.RightID).FirstOrDefault();
                exRight.RightName = right.RightName;
                //exRight.ParentID = right.ParentID;
                exRight.RightToken = [REDACTED];
                exRight.PageUrl = right.PageUrl;
                exRight.RightID = right.RightID;
                exRight.Sort = right.Sort;
                exRight.ShowInMenu = right.ShowInMenu;
                exRight.IconCls = right.IconCls;
                exRight.RightType = right.RightType;

                dbContext.SaveChanges();

                message = "修改成功";
                return true;
            }
        }

        public bool DeleteRight(int rightID, ref string message)
        {
            using (var dbContext = new AdminDbContext())
            {
                var oldRight = dbContext.Set<Right>().Where(c => c.RightID == rightID).FirstOrDefault();

                if (oldRight == null)
                {
                    message = "权限不存在";
                    return false;
                }
                else
                {
                    dbContext.Set<Right>().Remove(oldRight);
                    dbContext.SaveChanges();

                    message = "删除成功";
                    return true;
                }
            }
        }

        public List<Entity.Right> GetRight(int? rightID, int? parentID, Entity.RightType? rightType)
        {
            var dbContext = new AdminDbContext();
            var result = dbContext.Set<Right>().Where(c =>
                                                       (rightID == null || c.RightID == rightID)
                                                    && (parentID == null || c.ParentID == parentID)
                                                    && (rightType == null || c.RightType == rightType)
                                                    )
                                                    .OrderByDescending(r => r.Sort)
                                                    .ThenByDescending(r => r.RightID)
                                                    .ToList();

            return result;

        }

        public Entity.Right GetRightByID(int rightID)
        {
            var dbContext = new AdminDbContext();
            return dbContext.Set<Right>().Where(c => c.RightID == rightID).FirstOrDefault();
        }

        public Right GetRightByToken(string token)
        {
            return new AdminDbContext().Set<Right>().Where(r => r.RightToken == token).FirstOrDefault();
        }

        public bool AddRoleRight(int roleID, int rightID, ref string message)
        {
            using (var dbContext = new AdminDbContext())
            {
                var oldRoleRight = dbContext.Set<RoleRight>().Where(c => c.RoleID == roleID && c.RightID == rightID).FirstOrDefault();

                if (oldRoleRight != null)
                {
                    message = "该角色权限已经存在";
                    return false;
                }

                RoleRight roleRight = new RoleRight();
                roleRight.RoleID = roleID;
                roleRight.RightID = rightID;

                dbContext.Set<RoleRight>().Add(roleRight);
                dbContext.SaveChanges();

                message = "添加成功";
                return true;
            }
        }

        public bool RemoveRoleRight(int roleID, int? rightID, ref string message)
        {
            using (var dbContext = new AdminDbContext())
            {
                if (rightID != null)
                {
                    var oldRoleRight = dbContext.Set<RoleRight>().Where(c => c.RoleID == roleID && c.RightID == rightID.Value).FirstOrDefault();

                    if (oldRoleRight == null)
                    {
                        message = "该角色权限不存在";
                        return false;
                    }

                    dbContext.Set<RoleRight>().Remove(oldRoleRight);

                    message = "删除成功";
                    return true;
                }
                else
                {
                    var rights = dbContext.Set<RoleRight>().Where(c => c.RoleID == roleID).ToList();
                    rights.ForEach(r =>
                    {
                        dbContext.Set<RoleRight>().Remove(r);
                    });

                    dbContext.SaveChanges();
                    message = "删除成功";
                    return true;
                }
            }
        }

        public List<Entity.RoleRight> GetRoleRight(int? roleID, int? rightID)
        {
            var dbContext = new AdminDbContext();
            var listRoleRight = dbContext.Set<RoleRight>().Where(c =>
                                                                  (roleID == null || c.RoleID == roleID)
                                                                  && (rightID == null || c.RightID == rightID)
                                                                  ).ToList();


            return listRoleRight;

        }

        public bool AddUserRight(int userID, int rightID, ref string message)
        {
            clearUserRightCache(userID);

            using (var dbContext = new AdminDbContext())
            {
                var oldUserRight = dbContext.Set<UserRight>().Where(c => c.UserID == userID && c.RightID == rightID).FirstOrDefault();

                if (oldUserRight != null)
                {
                    message = "该用户权限已经存在";
                    return false;
                }

                UserRight userRight = new UserRight();
                userRight.UserID = userID;
                userRight.RightID = rightID;

                dbContext.Set<UserRight>().Add(userRight);
                dbContext.SaveChanges();

                message = "添加成功";
                return true;
            }
        }

        public bool RemoveUserRight(int userID, int? rightID, ref string message)
        {
            clearUserRightCache(userID);

            using (var dbContext = new AdminDbContext())
            {
                if (rightID != null)
                {
                    var oldUserRight = dbContext.Set<UserRight>().Where(c => c.UserID == userID && c.RightID == rightID).FirstOrDefault();

                    if (oldUserRight == null)
                    {
                        message = "该用户权限不存在";
                        return false;
                    }

                    dbContext.Set<UserRight>().Remove(oldUserRight);
                    dbContext.SaveChanges();

                    message = "删除成功";
                    return true;
                }
                else
                {
                    var urs = dbContext.Set<UserRight>().Where(ur => ur.UserID == userID).ToList();
                    urs.ForEach(ur =>
                    {
                        string msg = "";
                        RemoveUserRight(ur.UserID, ur.RightID, ref msg);
                    });

                    message = "操作成功";
                    return true;
                }
            }
        }

        public List<Entity.UserRight> GetUserRight(int? userID, int? rightID)
        {
            var dbContext = new AdminDbContext();
            var listUserRight = dbContext.Set<UserRight>().Where(c =>
                                                          (userID == null || c.UserID == userID)
                                                       && (rightID == null || c.RightID == rightID)
                                                       ).ToList();

            return listUserRight;
        }

        public List<Entity.Right> GetAllUserDisplayRight(int userID)
        {
            return this.GetAllUserRight(userID)
                .Where(r => r.ShowInMenu)
                .OrderByDescending(r => r.Sort)
                .ToList();

        }

        private void clearUserRightCache(int userID)
        {
            string cacheKey = String.Format("_USERRIGHT_{0}", userID);
            _cacheManager.Remove(cacheKey);
        }

        public List<Entity.Right> GetAllUserRight(int userID)
        {
            string cacheKey = String.Format("_USERRIGHT_{0}", userID);

            if (_cacheManager.IsCached(cacheKey))
            {
                return _cacheManager.GetObject<List<Entity.Right>>(cacheKey);
            }

            List<Right> rights = new List<Right>();

            var dbContext = new AdminDbContext();
            var urts = dbContext.Set<UserRight>().Where(ur => ur.UserID == userID)
                .Select(ur => ur.Right)
                .ToList();

            rights.AddRange(urts);

            var urrs = dbContext.Set<UserRole>().Where(ur => ur.UserID == userID)
                .Select(ur => ur.Role)
                .ToList();

            urrs.ForEach(rr =>
            {
                dbContext.Set<RoleRight>().Where(r => r.RoleID == rr.RoleID)
                    .Select(r => r.Right)
                    .ToList().ForEach(
                    r =>
                    {
                        if (rights.Where(rt => rt.RightID == r.RightID).Count() == 0)
                        {
                            rights.Add(r);
                        }
                    }
                    );
            });

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
            var dbContext = new AdminDbContext();
            return dbContext.Set<UserRole>().Where(ur => (userID == null || userID.Value == ur.UserID)
                && (roleID == null || roleID.Value == ur.RoleID))
                .ToList();
        }


        public List<UserIdentity> GetUserType()
        {
            return APIHelper.GetAPIResult<List<UserIdentity>>("/AdminSys/GetUserTypeList");
        }

        public bool AddUserRole(int userID, int roleID, ref string message)
        {
            clearUserRightCache(userID);

            if (GetUserRole(userID, roleID).Count > 0)
            {
                return true;
            }

            var ur = new UserRole();
            ur.UserID = userID;
            ur.RoleID = roleID;

            var dbContext = new AdminDbContext();
            dbContext.Set<UserRole>().Add(ur);
            dbContext.SaveChanges();

            message = "操作成功";
            return true;
        }

        public bool RemoveUserRole(int userID, int? roleID, ref string message)
        {
            clearUserRightCache(userID);

            if (roleID != null)
            {
                var dbContext = new AdminDbContext();
                var ur = dbContext.Set<UserRole>().Where(r => r.UserID == userID && (roleID == null || roleID.Value == r.RoleID)).FirstOrDefault();
                if (ur != null)
                {
                    dbContext.Set<UserRole>().Remove(ur);
                    dbContext.SaveChanges();

                    message = "操作成功";
                    return true;
                }

                message = "操作失败,用户角色不存在";
                return false;
            }
            else
            {
                var dbContext = new AdminDbContext();
                var urs = dbContext.Set<UserRole>().Where(r => r.UserID == userID).ToList();
                urs.ForEach(ur =>
                {
                    dbContext.Set<UserRole>().Remove(ur);
                });

                dbContext.SaveChanges();
                message = "操作成功";
                return true;
            }
        }


        public bool AddQuickAction(QuickAction quickAction, ref string message)
        {
            var dbContext = new AdminDbContext();
            dbContext.Set<QuickAction>().Add(quickAction);
            dbContext.SaveChanges();

            message = "操作成功";
            return true;
        }

        public List<QuickAction> GetQuickAction(int userID)
        {
            return new AdminDbContext().Set<QuickAction>()
                .Where(qa => qa.UserID == userID)
                .OrderByDescending(qa => qa.Sort)
                .ToList();
        }

        public QuickAction GetQuickActionByID(int id)
        {
            return new AdminDbContext().Set<QuickAction>()
                .Where(qa => qa.Id == id)
                .FirstOrDefault();
        }

        public bool DelQuickAction(int id, int userID, ref string message)
        {
            var dbContext = new AdminDbContext();
            var exqa = dbContext.Set<QuickAction>().Where(a => a.Id == id && a.UserID == userID).FirstOrDefault();
            if (exqa != null)
            {
                dbContext.Set<QuickAction>().Remove(exqa);
                dbContext.SaveChanges();
                message = "操作成功";
                return true;
            }

            message = "操作失败：数据不存在";
            return false;
        }

        public bool ModifyQuickAction(QuickAction quickAction, ref string message)
        {
            var dbContext = new AdminDbContext();
            var exqa = dbContext.Set<QuickAction>().Where(a => a.Id == quickAction.Id).FirstOrDefault();
            if (exqa != null)
            {
                exqa.Name = quickAction.Name;
                exqa.Sort = quickAction.Sort;
                exqa.Url = quickAction.Url;
                dbContext.SaveChanges();

                message = "操作成功";
                return true;
            }

            message = "操作失败：数据不存在";
            return false;

        }

        public QuickAction GetQuickActionByPage(int userID, string url)
        {
            return new AdminDbContext().Set<QuickAction>().Where(q => q.Url == url && q.UserID == userID).FirstOrDefault();
        }



        public bool ModifyUserPhone(int userID, string phone, ref string message)
        {
            using (var dbContext = new AdminDbContext())
            {
                var user = dbContext.Set<AdminUser>().Where(u => u.UserID == userID).FirstOrDefault();

                if (user == null)
                {
                    message = "用户名不存在";
                    return false;
                }
                else
                {
                    user.Phone = phone;
                    dbContext.SaveChanges();

                    message = "修改成功";
                    return true;
                }
            }
        }

        public bool ModifyUserPhoneCode(int userID, string phoneCode, DateTime phoneCodeTime, ref string message)
        {
            using (var dbContext = new AdminDbContext())
            {
                var user = dbContext.Set<AdminUser>().Where(u => u.UserID == userID).FirstOrDefault();

                if (user == null)
                {
                    message = "用户名不存在";
                    return false;
                }
                else
                {
                    user.PhoneCode = phoneCode;
                    user.PhoneCodeTime = phoneCodeTime;
                    dbContext.SaveChanges();

                    message = "修改成功";
                    return true;
                }
            }
        }

        public bool ModifyUserInfo(int userID, string loginName, string displayName, string password, string phone, int? type, int? status, int? secretKey, ref string message)
        {
            using (var dbContext = new AdminDbContext())
            {
                var _user = dbContext.Set<AdminUser>().Where(u => u.UserID == userID).FirstOrDefault();

                if (_user == null)
                {
                    message = "用户不存在";
                    return false;
                }
                else
                {
                    dbContext.SaveChanges();
                    message = "修改成功";
                    return true;
                }
            }
        }

        public APIResult GetTGCode(int chatID, string tgCode)
        {
            var _message = string.Format("请输入电报验证码: {0}", tgCode);
            var _result = APIHelper.GetAPIResult<APIResult>(string.Format("/Money/GetTGCode?chatID={0}&tgCode={1}", chatID, _message));

            return _result;
        }
    }
}
