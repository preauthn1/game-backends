using FW.Core.Auth.Token;
using System;
using System.Collections.Generic;
using System.Linq;


namespace FW.Core.Auth.AuthorizeRule
{
    /// <summary>
    /// 授权规则集合
    /// </summary>
    public class AuthorizeRuleColl : List<AuthorizeRule>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public AuthorizeRuleColl()
        {
            LogonUrl = "~/Account/Logon";
        }

        /// <summary>
        /// 登录地址
        /// </summary>
        public string LogonUrl
        {
            get;
            set;
        }

        public bool CheckAccess(IRightToken rightToken, AuthUser user, AuthorizeType type)
        {
            //检查用户权限是否匹配,用户存在匹配的权限将被允许访问
            foreach (var token in user.Rights)
            {
                if (rightToken.ParseToken(token).IsMatch(rightToken))
                {
                    return true;
                }
            }

            //遍历所有规则
            foreach (var rule in this)
            {
                if (rule.RightToken.IsMatch(rightToken))
                {
                    //拒绝匿名用户
                    if (rule.DenyUsers.Contains(AuthorizeRule.GUESTUSER))
                    {
                        if (user.IsGuest)
                            return false;
                        else
                            return true;
                    }
                    

                    ///检查拒绝的用户
                    if (rule.DenyUsers.Where(du => String.Compare(du, user.LoginName, true) == 0).Count() > 0)
                    {
                        return false;
                    }

                    //允许所有
                    if (rule.AllowRoles.Contains(AuthorizeRule.ALLROLES))
                    {
                        return true;
                    }

                    //检查用户的角色
                    foreach (string ar in rule.AllowRoles)
                    {
                        foreach (string ur in user.Roles)
                        {
                            if (string.Compare(ar, ur, true) == 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            //开放的授权规则
            if (type == AuthorizeType.Opened)
            {
                return true;
            }
            //封闭的授权规则
            else
            {
                return false;
            }

        }
    }

    /// <summary>
    /// 授权类型
    /// </summary>
    public enum AuthorizeType
    {
        Opened,
        Closed
    }
}
