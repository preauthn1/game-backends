using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.RawEntity;
using game.Entity.Users;

namespace game.Service
{
    
    public interface IAdminRightService
    {

        List<SystemAdminRightSetting> GetSystemAdminRightFunctions();


        List<UserRightEntity> GetUserRightByRightID(List<int> rightIDs);


        List<RoleRightEntity> GetRoleRightByRightID(List<int> rightIDs);

    }
}
