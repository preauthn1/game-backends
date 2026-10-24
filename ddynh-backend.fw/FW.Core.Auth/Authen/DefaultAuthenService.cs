using System;


namespace FW.Core.Auth.Authen
{
    public class DefaultAuthenService : IAuthenService
    {
        /// <summary>
        /// cotr
        /// </summary>
        public DefaultAuthenService()
        {
            CurrentUser = AuthUser.Guest;
        }

        public string Signin(AuthUser user)
        {
            CurrentUser = user;
            return String.Empty;
        }

        public void Signout()
        {
            CurrentUser = null;
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
            get;
            private set;
        }
    }
}
