using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IPermissionService
    {
            
        UserPermissionViewModel GetAvailablePermissionsForMenu(int userID);

        UserPermissionViewModel GetPermissionsNestedFormat(int userID);

        List<PermissionViewModel> GetPermissions(int userID);

        RolePermissionCompareViewModel CompareRolePermissionsDiff(int userID, LazyCommonRequestModel model);

        bool IsPermissionExist(int userID, List<AdminSysPermissionKey> permissionKeys);


    }
}
