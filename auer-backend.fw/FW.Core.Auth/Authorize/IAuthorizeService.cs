using System;
using System.Web;


namespace FW.Core.Auth.Authorize
{
    public interface IAuthorizeService
    {
        /// <summary>
        /// 检查用户访问权限
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        bool CheckAccess(AuthUser user, string cmd);

        /// <summary>
        /// 登录地址
        /// </summary>
        string LogonUrl { get; }
    }
}
