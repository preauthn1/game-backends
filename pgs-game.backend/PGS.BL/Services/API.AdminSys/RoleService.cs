using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.BL.Interface;
using target.Models.Enums;
using target.Models.Models;
using System.Linq;
using target.Utility.Helper;
using target.Models;
using target.DA.Repository;
using Lck.Utility.Extensions.Helper;
using DocumentFormat.OpenXml.Wordprocessing;

namespace target.BL.Services
{

    public class RoleService : BaseAdminSysService, IRoleService
    {

        public RoleRepository RoleRepository { get; set; }

        public RolePermissionMappingRepository RolePermissionMappingRepository { get; set; }

        public UserRoleMappingRepository UserRoleMappingRepository { get; set; }

        private const int ROLE_NAME_MAX_LENGTH = 50;


        public PagedList<RoleViewModel> GetByPagingFilter(QueryRoleApiRequestParameterModel filter)
        {
            var query = RoleRepository.GetPagingByFilter(filter);

            var viewModels = ConvertToNewTypePagingObject<RoleEntity, RoleViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = ConvertToViewModels(x);
                    return convert;
                })
                .ToList();

            SetExtraProperty(viewModels.Items);

            if(filter.PageSize >= 500)
            {
                viewModels.Items = viewModels.Items
                    .OrderBy(x => x.Sort)
                    .ThenBy(x=> x.RoleID)
                    .ToList();
            }

            return viewModels;
        }

        public RoleViewModel GetByID(int roleID)
        {
            var queryRole = RoleRepository.GetById(roleID);

            if (queryRole == null)
                throw new TerproException(ApiErrorCode.NotFoundRole);

            var convert = ConvertToViewModels(queryRole);

            var queryPermissions = RolePermissionMappingRepository.GetByFilter(new QueryRolePermissionMappingRequestViewModel
            {
                RoleIDs = roleID.ToListCollection()
            });

            convert.Permissions = queryPermissions.Select(x => x.PermissionID).ToList();
            return convert;
        }


        public void Create(int userID , RoleCreateRequestModel model)
        {
            model.RuleFor(x => x.RoleName).Rule_NotEmptyText().Rule_MaxLengthOK(ROLE_NAME_MAX_LENGTH);

            var newEntity = model.ReturnInjecteObject<RoleEntity>();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            newEntity.CreateTime = DateTimeHelper.GetCurrentTime();
            newEntity.UpdateTime = DateTimeHelper.GetCurrentTime();
            newEntity.UpdateBy = currentUser.UserName;
            newEntity.CreateBy = currentUser.UserName;
            newEntity.Sort = model.Sort;

            var newRoleID = RoleRepository.Create(newEntity);

            if(model.TemplateID.HasValue)
            {
                if(currentUser.IsSuperAdminOrg)
                {
                    RolePermissionMappingRepository.CopyPermissionFromRole(newRoleID, model.TemplateID.Value);
                }
            }
        }


        public void Update(int userID, RoleUpdateRequestModel model)
        {
            model.RuleFor(x => x.RoleID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.RoleName).Rule_NotEmptyText().Rule_MaxLengthOK(ROLE_NAME_MAX_LENGTH);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var queryExist = RoleRepository.GetById(model.RoleID);

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundRole);

            queryExist.RoleName = model.RoleName;
            queryExist.Sort = model.Sort;
            queryExist.UpdateTime = DateTimeHelper.GetCurrentTime();
            queryExist.UpdateBy = currentUser.UserName;
            queryExist.Memo = model.Memo;

            RoleRepository.Update(queryExist);
        }


        public void Delete(int roleID)
        {
            var queryExist = RoleRepository.GetById(roleID);

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundRole);

            //刪除mapping
            RoleRepository.Delete(roleID);

            //刪除該角色的權限
            RolePermissionMappingRepository.DeleteAndInsertNew(roleID , new List<int>());

            //刪除被指派為該角色的設置
            UserRoleMappingRepository.DeleteByRoleID(roleID);

        }



        public void UpdateRolePermissions(int userID , RolePermissionUpdateModel model)
        {
            model.RuleFor(x => x.RoleID).Rule_NotZeroNotMinus();
            model.Permissions = model.Permissions.Distinct().ToList();

            var queryExist = RoleRepository.GetById(model.RoleID);

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundRole);

            RolePermissionMappingRepository.DeleteAndInsertNew(model.RoleID , model.Permissions);

            //清除cache (如果未來開放帳號角色一對多 清除cache會有些問題)
            CacheRemoveHelper.RemoveCache_WhenRolePermissionUpdate(model.RoleID);

            var log = InstanceOperateLog(userID, UserOperateTypeEnum.UpdateRolePermission, queryExist.RoleID, model.ConvertToJsonString());
            WriteOperateLogAndNotifyAlways(userID, log);
        }


        private RoleViewModel ConvertToViewModels(RoleEntity queryRole)
        {
            var convert = queryRole.ReturnInjecteObject<RoleViewModel>();
            return convert;
        }


        private void SetExtraProperty(List<RoleViewModel> items)
        {
            if (items.IsEmptyOrNull())
                return;

            List<int> roleIDs = items.Select(z => z.RoleID).ToList();

            var queryRoles = UserRoleMappingRepository.GetByFilter(new QueryUserRoleMappingRequestViewModel
            {
                RoleIDs = roleIDs,
                IsEnableAccountOnly = true,
            });

            foreach (var item in items)
            {
                item.UserMappingCount = queryRoles.Where(z => z.RoleID == item.RoleID).Count();
            }
        }


    }
}
