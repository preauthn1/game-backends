using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using example.invalid;
using FW.Core.AdminSys.Provider.Entity;
using FW.Core.Cache;
using FW.Core.Helper;
using FW.Core.Web.ViewModels;

namespace FW.Core.AdminSys.Provider
{
    public class AdmiSysWebUiApiDataHelper
    {
        public static List<Organization> GetOrganizations(string orgId)
        {
            var apiResult = APIHelper.GetAPIResult<List<Organization>>($"/Organization/GetByOrgByIds?ids={orgId}");
            return apiResult;
        }

        public static List<TargetRolesAssinRightCountModel> GetTargetRoleAssignRightCount(int orgId)
        {
            var apiResult = APIHelper.GetAPIResult<List<TargetRolesAssinRightCountModel>>($"/AdminRole/GetTargetRoleAssignRightCount?OrgId={orgId}");
            return apiResult;
        }


        public static PagedList<AdminUser> GetAdminUsersPagingByOrgId(string orgIds,string loginName, int pageIndex , int pageSize)
        {
            var apiResult = APIHelper.GetAPIResult<PagedList<AdminUser>>($"/AdminSys/GetUsersPagingByOrgID?orgId={orgIds}&UserNameKeyword={loginName}&PageIndex={pageIndex}&PageSize={pageSize}");
            return apiResult;
        }


        public static List<Role> GetRolesByOrgId(string orgId)
        {
            var apiResult = APIHelper.GetAPIResult<List<Role>>($"/AdminRole/GetRolesByFilter?OrgId={orgId}");
            return apiResult;
        }

        public static List<ApiAssignRightModel> GetUserRightsByRightIDs(List<int> rightIDs)
        {
            string rightIDsText = string.Join("," , rightIDs);
            var apiResult = APIHelper.GetAPIResult<List<ApiAssignRightModel>>($"/AdminRight/GetUserRightByRightID?rightIDs={rightIDsText}");
            return apiResult;
        }

        public static List<ApiAssignRightModel> GetRoleRightsByRightIDs(List<int> rightIDs)
        {
            string rightIDsText = string.Join(",", rightIDs);
            var apiResult = APIHelper.GetAPIResult<List<ApiAssignRightModel>>($"/AdminRight/GetRoleRightByRightID?rightIDs={rightIDsText}");
            return apiResult;
        }
        

        public static APIResult CreateRole(RoleUpdateRequestViewModel model)
        {
            var apiResult = APIHelper.PostAPIResult($"/AdminRole/CreateRole", model);
            return apiResult;
        }


        public static List<int> GetSystemAdminRightIds()
        {
            var apiResult = APIHelper.GetAPIResult<List<int>>($"/AdminRight/GetSystemAdminRightIds");
            return apiResult;
        }


    }
}
