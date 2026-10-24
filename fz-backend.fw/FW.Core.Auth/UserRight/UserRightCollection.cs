using System;
using System.Linq;
using System.Collections.Generic;


namespace FW.Core.Auth.UserRight
{
    public class UserRightCollection : List<UserRight>
    {
        /// <summary>
        /// 加载用户权限
        /// </summary>
        /// <param name="user"></param>
        public void LoadUserRight(AuthUser user)
        {
            var ur = this.Where(r => r.UserName == user.UserID).FirstOrDefault();
            if (ur != null)
            {
                user.Rights.AddRange(ur.UserRights);
                user.Roles.AddRange(ur.UserRoles);
            }
        }

    }
}
