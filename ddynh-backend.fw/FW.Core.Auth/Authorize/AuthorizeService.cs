using System;
using FW.Core.Auth.AuthorizeRule;
using FW.Core.Auth.Token;

namespace FW.Core.Auth.Authorize
{
    public abstract class AuthorizeServiceBase : IAuthorizeService
    {
        private IRightTokenBuilder _tokenBuilder;
        private IAuthorizeRuleProvider _ruleProvider;
        protected AuthorizeRuleColl _rules;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tokenBuilder"></param>
        /// <param name="ruleProvider"></param>
        public AuthorizeServiceBase(IRightTokenBuilder tokenBuilder, IAuthorizeRuleProvider ruleProvider)
        {
            FW.Core.Helper.Check.NotEmpty<IRightTokenBuilder>(tokenBuilder, "tokenBuilder");
            FW.Core.Helper.Check.NotEmpty<IAuthorizeRuleProvider>(ruleProvider, "ruleProvider");

            _tokenBuilder = tokenBuilder;
            _ruleProvider = ruleProvider;

            try
            {
                _rules = _ruleProvider.LoadRules();
            }
            catch (System.Exception ex)
            {
                throw new AuthException("加载授权规则错误," + ex.Message, ex);
            }
            LogonUrl = _rules.LogonUrl;
        }

        public bool CheckAccess(AuthUser user, string cmd)
        {
            //创建当前资源的访问凭证
            IRightToken token = [REDACTED];

            //检查对资源的访问凭证
            return checkAccessInternal(token, user);
        }

        protected abstract bool checkAccessInternal(IRightToken token, AuthUser user);


        public string LogonUrl
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// 开放的认证服务实现
    /// 默认对所有资源都是开放访问的
    /// </summary>
    public class OpenedAuthorizeService : AuthorizeServiceBase
    {
        public OpenedAuthorizeService(IRightTokenBuilder tokenBuilder, IAuthorizeRuleProvider ruleProvider)
            :base(tokenBuilder, ruleProvider)
        { }

        protected override bool checkAccessInternal(IRightToken token, AuthUser user)
        {
            return _rules.CheckAccess(token, user, AuthorizeType.Opened);
        }
    }

    /// <summary>
    /// 封闭的认证服务实现
    /// 默认对所有资源都是拒绝访问的
    /// </summary>
    public class ClosedAuthorizeService : AuthorizeServiceBase
    {
        public ClosedAuthorizeService(IRightTokenBuilder tokenBuilder, IAuthorizeRuleProvider ruleProvider)
            : base(tokenBuilder, ruleProvider)
        { }

        protected override bool checkAccessInternal(IRightToken token, AuthUser user)
        {
            return _rules.CheckAccess(token, user, AuthorizeType.Closed);
        }

    }

}
