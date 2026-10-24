using FW.Core.Auth.Token;
using System;
using System.Linq;
using System.Collections.Generic;


namespace FW.Core.Auth.AuthorizeRule
{
    /// <summary>
    /// 授权规则,定义权限凭证对角色和用户的许可信息
    /// </summary>
    public class AuthorizeRule
    {
        /// <summary>
        /// 所有角色
        /// </summary>
        public const string ALLROLES = "*";
        /// <summary>
        /// 匿名用户
        /// </summary>
        public const string GUESTUSER = "?";

        /// <summary>
        /// ctor
        /// </summary>
        public AuthorizeRule()
        {
            AllowRoles = new List<string>();
            DenyUsers = new List<string>();
        }

        /// <summary>
        /// 权限标识
        /// </summary>
        public IRightToken RightToken
        {
            get;
            set;
        }

        /// <summary>
        /// 允许的角色
        /// </summary>
        public List<string> AllowRoles
        {
            get;
            set;
        }

        /// <summary>
        /// 拒绝的用户
        /// </summary>
        public List<string> DenyUsers
        {
            get;
            set;
        }

    }
}
