using System;
using System.Collections.Generic;


namespace FW.Core.Auth.UserRight
{
    /// <summary>
    /// 用户权限
    /// </summary>
    public class UserRight
    {
        /// <summary>
        /// ctor
        /// </summary>
        public UserRight()
        {
            UserRoles = new List<string>();
            UserRights = new List<string>();
        }

        public string UserName
        {
            get;
            set;
        }

        public IList<string> UserRoles
        {
            get;
            set;
        }

        public IList<string> UserRights
        {
            get;
            set;
        }

    }

}
