using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IRoleService
    {
        PagedList<RoleViewModel> GetByPagingFilter(QueryRoleApiRequestParameterModel filter);

        RoleViewModel GetByID(int targetID);

        void Create(int userID, RoleCreateRequestModel model);

        void Update(int userID, RoleUpdateRequestModel model);

        void Delete(int targetID);

        void UpdateRolePermissions(int userID , RolePermissionUpdateModel model);

    }
}
