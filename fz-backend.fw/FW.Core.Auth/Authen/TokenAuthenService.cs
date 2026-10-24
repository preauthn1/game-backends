using System;
using System.Web.Script.Serialization;
using System.Web.Security;


namespace FW.Core.Auth.Authen
{
    public abstract class TokenAuthenService : IAuthenService
    {
        public virtual string Signin(AuthUser user)
        {
            user.LogonTime = DateTime.Now;
            return generateTicketData(user);
        }

        public virtual void Signout()
        {
            
        }

        public virtual bool IsLogon
        {
            get 
            {
                return CurrentUser != AuthUser.Guest;
            }
        }

        public virtual AuthUser CurrentUser
        {
            get 
            {
                string userToken = [REDACTED];
                if (String.IsNullOrWhiteSpace(userToken))
                {
                    return AuthUser.Guest;
                }

                FormsAuthenticationTicket ticket = null;
                try
                {
                    ticket = FormsAuthentication.Decrypt(userToken);
                    if (ticket == null)
                    {
                        return AuthUser.Guest;
                    }
                }
                catch
                {
                    return AuthUser.Guest;
                }

                var user = new JavaScriptSerializer().Deserialize<AuthUser>(ticket.UserData);
                if (user == null)
                {
                    return AuthUser.Guest;
                }

                return user;

            }
        }
        
        /// <summary>
        /// 生成用户标识
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        protected virtual string generateTicketData(AuthUser user)
        {
            string userInfo = new JavaScriptSerializer().Serialize(user);
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                1,
                generateTokenName(),
                DateTime.Now,
                DateTime.Now.AddMinutes(int.MaxValue),
                true,
                userInfo);
            string encryptTicket = FormsAuthentication.Encrypt(ticket);

            return encryptTicket;
        }

        /// <summary>
        /// 生成用户标识名称
        /// </summary>
        /// <returns></returns>
        protected  virtual string generateTokenName()
        {
            return "UserToken";
        }

        /// <summary>
        /// 加载用户标识
        /// </summary>
        /// <returns></returns>
        protected abstract string loadUserToken();

    }
}
