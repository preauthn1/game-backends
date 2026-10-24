using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Users;

namespace game.Service
{
    
    public interface IAdminRoleService
    {
        List<Role> GetRolesByFilter(QueryRoleRequestViewModel filter);

        List<TargetRolesAssinRightCountModel> GetTargetRolesAssignRightCount(int orgId);

        void CreateRoleAndRightByTemplateSetting(AutoGenerateRoleAndRightRequestViewModel model);

    }
}
