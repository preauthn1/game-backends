using System;


namespace FW.Core.Auth.Authen
{
    public interface IAuthenService
    {
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        string Signin(AuthUser user);
        
        /// <summary>
        /// 用户注销
        /// </summary>
        void Signout();

        /// <summary>
        /// 当前用户是否登录
        /// </summary>
        bool IsLogon { get; }

        /// <summary>
        /// 获取当前用户
        /// </summary>
        AuthUser CurrentUser { get; }
    }
}
