using FW.Core.Auth.AuthorizeRule;
using FW.Core.Auth.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.AdminSys.Provider.Auth
{
    public class MvcAuthRuleDbProvider : IAuthorizeRuleProvider
    {
        private IAdminSysProvider _provider;
        public static List<string> NonAuthTokenCollection = new List<string>();

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="provider"></param>
        public MvcAuthRuleDbProvider(IAdminSysProvider provider)
        {
            _provider = provider;
        }

        public AuthorizeRuleColl LoadRules()
        {
            var rules = new AuthorizeRuleColl();

            NonAuthTokenCollection.ForEach(r =>
            {
                rules.Add(new AuthorizeRule()
                {
                    RightToken = [REDACTED] MvcSegmentRightToken(r),
                    AllowRoles = new List<string>() { "*" }
                });
            });

            var rights = _provider.GetRight(null, null, null);

            if (rights != null)
            {
                rights.ForEach(r =>
                {
                    rules.Add(new AuthorizeRule()
                    {
                        RightToken = [REDACTED] MvcSegmentRightToken(r.RightToken),
                        AllowRoles = new List<string>() {  r.RightToken }
                    });
                });
            }

           

            return rules;
        }
    }
}
