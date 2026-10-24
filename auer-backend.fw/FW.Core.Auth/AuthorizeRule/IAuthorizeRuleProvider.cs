using System;


namespace FW.Core.Auth.AuthorizeRule
{
    public interface IAuthorizeRuleProvider
    {
        /// <summary>
        /// 加载授权规则
        /// </summary>
        /// <returns></returns>
        AuthorizeRuleColl LoadRules();
    }
}
