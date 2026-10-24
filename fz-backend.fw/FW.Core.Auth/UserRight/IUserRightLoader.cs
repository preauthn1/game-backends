using System;
using System.Collections.Generic;


namespace FW.Core.Auth.UserRight
{
    public interface IUserRightLoader
    {
        void LoadUserRightToken(AuthUser user);
    }

}
