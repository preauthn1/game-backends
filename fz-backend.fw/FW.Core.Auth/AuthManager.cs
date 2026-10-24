using FW.Core.Auth.Authen;
using FW.Core.Auth.Authorize;
using FW.Core.Auth.Token;
using FW.Core.Auth.UserRight;
using System;


namespace FW.Core.Auth
{
    public  class AuthManager
    {
        private IAuthenService _authenService;
        private IAuthorizeService _authorizeService;
        private IUserRightLoader _userTokenLoader;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="authenService"></param>
        /// <param name="authorizeService"></param>
        /// <param name="userTokenLoader"></param>
        public AuthManager(IAuthenService authenService, IAuthorizeService authorizeService, IUserRightLoader userTokenLoader)
        {
            _authenService = authenService;
            _authorizeService = authorizeService;
            _userTokenLoader = userTokenLoader;
        }
        
        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public string Signin(AuthUser user)
        {
            return _authenService.Signin(user);
        }

        /// <summary>
        /// 注销登陆
        /// </summary>
        public void Signout()
        {
            _authenService.Signout();
        }

        /// <summary>
        /// 当前用户是否登陆
        /// </summary>
        public bool IsLogon 
        {
            get
            {
                return _authenService.IsLogon;
            }
        }

        /// <summary>
        /// 当前用户
        /// </summary>
        public AuthUser CurrentUser
        {
            get
            {
                var user = _authenService.CurrentUser;
                if (user != AuthUser.Guest)
                {
                    _userTokenLoader.LoadUserRightToken(user);
                }

                return user;
            }
        }

        /// <summary>
        /// 检查用户访问权限
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public bool CheckAccess(AuthUser user, string cmd)
        {
            return _authorizeService.CheckAccess(user, cmd);
        }

        /// <summary>
        /// 检查用户访问权限
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool CheckAccess(AuthUser user)
        {
            return _authorizeService.CheckAccess(user, String.Empty);
        }

        /// <summary>
        /// 检查用户访问权限
        /// </summary>
        /// <returns></returns>
        public bool CheckAccess()
        {
            return _authorizeService.CheckAccess(CurrentUser, String.Empty);
        }

        /// <summary>
        /// 检查用户访问权限
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public bool CheckAccess(string cmd)
        {
            return _authorizeService.CheckAccess(CurrentUser, cmd);
        }

        /// <summary>
        /// 登录地址
        /// </summary>
        public string LogonUrl
        {
            get
            {
                return _authorizeService.LogonUrl;
            }
        }

    }
}
