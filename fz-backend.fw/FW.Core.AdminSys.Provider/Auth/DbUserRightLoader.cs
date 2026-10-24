using FW.Core.Auth.UserRight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.AdminSys.Provider.Auth
{
    public class DbUserRightLoader : IUserRightLoader
    {
        private IAdminSysProvider _provider;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="provider"></param>
        public DbUserRightLoader(IAdminSysProvider provider)
        {
            _provider = provider;
        }

        public void LoadUserRightToken(Core.Auth.AuthUser user)
        {
            var rs = _provider.GetAllUserRight(Convert.ToInt32(user.UserID));
            if (rs != null && rs.Count > 0)
            {
                rs.ForEach(r => {
                    user.Roles.Add(r.RightToken);
                    user.Rights.Add(r.RightToken);
                });
            }
        }
    }
}
