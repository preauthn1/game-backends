using System;
using System.Configuration;


namespace FW.Core.Auth.UserRight
{
    public class ConfigUserRightLoader : IUserRightLoader
    {
        private UserRightCollection _userRights;

        /// <summary>
        /// ctor
        /// </summary>
        public ConfigUserRightLoader()
        {
            try
            {
                _userRights = System.Configuration.ConfigurationManager.GetSection("userRightConfig") as UserRightCollection;
            }
            catch (System.Exception ex)
            {
                throw new AuthException("加载用户权限配置错误,"+ ex.Message);
            }
        }

        public void LoadUserRightToken(AuthUser user)
        {
            _userRights.LoadUserRight(user);
        }
    }
}
