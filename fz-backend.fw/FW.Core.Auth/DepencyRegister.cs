using FW.Core.Auth.Authen;
using FW.Core.Auth.Authorize;
using FW.Core.Auth.AuthorizeRule;
using FW.Core.Auth.AuthorizeRule.Mvc;
using FW.Core.Auth.Token;
using FW.Core.Auth.UserRight;
using FW.Core.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Auth
{
    public class DepencyRegister : SystemDepencyRegister
    {
        public override void Register(IEngine engine)
        {
            var dctAuthParams = new Dictionary<string, object>() { { "persistTime", 0 }, { "cookieDomain", "" } };
            EngineContext.Current.RegisterWithParameters<IAuthenService, CookieAuthenService>(dctAuthParams, "", LifeStyle.Transient);

            EngineContext.Current.Register<IRightTokenBuilder, MvcRightTokenBuilder>();
            EngineContext.Current.Register<IAuthorizeRuleProvider, ConfigMvcTokenAuthorizeRuleProvider>();
            //EngineContext.Current.Register<IAuthenService, CookieAuthenService>();
            EngineContext.Current.Register<IAuthorizeService, ClosedAuthorizeService>();
            EngineContext.Current.Register<IUserRightLoader, ConfigUserRightLoader>();
            EngineContext.Current.Register<AuthManager>(LifeStyle.Singleton);

        }
    }
}
