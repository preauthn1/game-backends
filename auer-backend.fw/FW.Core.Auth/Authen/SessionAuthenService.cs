using System;
using System.Web;


namespace FW.Core.Auth.Authen
{
    public class SessionAuthenService : IAuthenService
    {
        private const string SESSIONKEY_USERINFO = "SessionAuthenService_SESSIONKEY_USERINFO";

        public string Signin(AuthUser user)
        {
            var userinfostr = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(user);
            HttpContext.Current.Session[SESSIONKEY_USERINFO] = userinfostr;
            return String.Empty;
        }

        public void Signout()
        {
            HttpContext.Current.Session[SESSIONKEY_USERINFO] = null;
        }

        public bool IsLogon
        {
            get 
            {
                return CurrentUser != null && !CurrentUser.IsGuest;
            }
        }

        public AuthUser CurrentUser
        {
            get 
            {
                if (HttpContext.Current.Session[SESSIONKEY_USERINFO] == null)
                {
                    return AuthUser.Guest;
                }

                var userstr = HttpContext.Current.Session[SESSIONKEY_USERINFO].ToString();
                if (String.IsNullOrWhiteSpace(userstr))
                {
                    return AuthUser.Guest;
                }
                return new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<AuthUser>(userstr);
            }
        }
    }
}
