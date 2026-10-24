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

namespace target.BL.Services
{

    public class PermissionService : BaseAdminSysService, IPermissionService
    {
        public PermissionRepository PermissionRepository { get; set; }

        public RolePermissionMappingRepository RolePermissionMappingRepository { get; set; }

        public UserPermissionMappingRepository UserPermissionMappingRepository { get; set; }

        public UserRoleMappingRepository UserRoleMappingRepository { get; set; }

        public RoleRepository RoleRepository { get; set; }

        public UserCustomizedSettingRepository UserCustomizedSettingRepository { get; set; }




        public UserPermissionViewModel GetAvailablePermissionsForMenu(int userID)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            if(currentUser.Roles.IsEmptyOrNull())
            {
                return new UserPermissionViewModel
                {
                    AllPermissions = new List<PermissionTokenBriefViewModel>(),
                    NestedPermissions = new List<UserPermissionItemViewModel>(),
                    AllPermissionsIncludeUserPermission = new List<PermissionTokenBriefViewModel>(),
                };
            }
               
            List<int> queryRolePermissionIDs = RolePermissionMappingRepository.GetByFilter(new QueryRolePermissionMappingRequestViewModel
                {
                    RoleIDs = currentUser.Roles
                })
                .Select(x=> x.PermissionID)
                .ToList();

            var queryPermissions = PermissionRepository.GetByIds(queryRolePermissionIDs);
            
            List<UserPermissionItemViewModel> nestedModels = ConvertToViewModels(queryPermissions);

            //複製一份一模一樣的
            var allPermissionViewModels = nestedModels.Select(x => x.ReturnInjecteObject<UserPermissionItemViewModel>()).ToList();

            //處理巢狀迴圈
            NestedFormatSetValue(nestedModels);

            //排除特殊type的權限(大部分此類都只是拿來判斷用的flag 而非頁面型menu)
            nestedModels = nestedModels.Where(x => x.PermissionType != PermissionTypeEnum.Special.ToInteger()).ToList();

            var viewModel = new UserPermissionViewModel
            {
                NestedPermissions = nestedModels.Where(x => x.RootPermissionID == x.PermissionID).ToList(),
                AllPermissions = allPermissionViewModels.Select(x => x.ReturnInjecteObject<PermissionTokenBriefViewModel>()).ToList(),
            };

            viewModel.AllPermissionsIncludeUserPermission = GetPermissionsIncludeUserPermission(userID, viewModel.AllPermissions);
            return viewModel;
        }

    

        public UserPermissionViewModel GetPermissionsNestedFormat(int userID)
        {
            var query = PermissionRepository.GetAll();

            var convertBrief = query
                .Select(x => ConvertToBriefViewModel(x))
                .ToList();

            List<UserPermissionItemViewModel> viewModels = ConvertToViewModels(query);

            //處理巢狀迴圈
            NestedFormatSetValue(viewModels);

            var rootItems = viewModels
                .Where(x => x.RootPermissionID == x.PermissionID)
                .OrderBy(z => z.Sort)
                .ToList();

            return new UserPermissionViewModel
            {
                NestedPermissions = rootItems,
                AllPermissions = convertBrief,
                AllPermissionsIncludeUserPermission = new List<PermissionTokenBriefViewModel>(),
            };
        }



        public RolePermissionCompareViewModel CompareRolePermissionsDiff(int userID , LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.RoleIDs).Rule_IsNotEmptyCollection();

            if(model.RoleIDs.Count() < 2)
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, "請至少勾選兩個查詢項目");
            }

            var queryRoles = RoleRepository.GetByIds(model.RoleIDs).OrderBy(x => x.RoleID).ToList();

            var queryAssignPermissions = RolePermissionMappingRepository.GetByFilter(new QueryRolePermissionMappingRequestViewModel
            {
                RoleIDs = model.RoleIDs
            });

            List<int> permissionIDs = queryAssignPermissions.Select(x => x.PermissionID).Distinct().ToList();

            var queryAllPermissions = PermissionRepository.GetAll();

            Dictionary<int, Dictionary<int, bool>> rolePermissionEnableDic = new Dictionary<int, Dictionary<int, bool>>();

            foreach(var role in queryRoles)
            {
                var permissionEnableDic = new Dictionary<int, bool>();

                foreach (var permission in queryAllPermissions)
                {
                    bool isExist = queryAssignPermissions.Any(x => x.PermissionID == permission.PermissionID && x.RoleID == role.RoleID);
                    permissionEnableDic.Add(permission.PermissionID, isExist);
                }

                rolePermissionEnableDic.Add(role.RoleID, permissionEnableDic);
            }

            var convertBrief = queryAllPermissions.Select(x => new PermissionTokenBriefViewModel
                {
                    PageUrl = x.PageUrl,
                    Token = [REDACTED],
                    PermissionID = x.PermissionID,
                    ParentPermissionID = x.ParentPermissionID
                })
                .ToList();

            List<UserPermissionItemViewModel> nestedViewModels = ConvertToViewModels(queryAllPermissions);

            //處理巢狀迴圈
            NestedFormatSetValue(nestedViewModels);

            var resultModel = new RolePermissionCompareViewModel
            {
                Permissions = new List<PermissionCompareBriefViewModel>()
            };

            resultModel.Roles = queryRoles
                .Select(x => new RoleBriefViewModel
                {
                    RoleID = x.RoleID,
                    RoleName = x.RoleName
                })
                .ToList();

            var emptyList = new List<UserPermissionItemViewModel>();


            string checkFlag = "v";

            foreach (var permission in queryAllPermissions)
            {
                bool hasAssignPermissionToAnyone = permissionIDs.Contains(permission.PermissionID);

                var permissionItem = new PermissionCompareBriefViewModel
                {
                    PermissionID = permission.PermissionID,
                    PermissionToken = [REDACTED],
                    PermissionNevigatePath = new List<string>
                    {
                        permission.PermissionToken
                    },
                    RootPermissionToken = [REDACTED],
                    RoleEnableFlag = new List<string>(),
                    IsUnAssignToRoles = hasAssignPermissionToAnyone ? false : true,
                };

                foreach (var role in resultModel.Roles)
                {
                    bool isAssignForThisRole = rolePermissionEnableDic[role.RoleID][permission.PermissionID];
                    permissionItem.RoleEnableFlag.Add(isAssignForThisRole ? checkFlag : "");
                }

                if(permission.ParentPermissionID != 0)
                {
                    permissionItem.HasParent = true;

                    var parent = queryAllPermissions.FirstOrDefault(x => x.PermissionID == permission.ParentPermissionID);
                    permissionItem.ParentPermissionToken = [REDACTED] => x.PermissionID == permission.ParentPermissionID)?.PermissionToken;

                    var loopHierarchy = parent.ReturnInjecteObject<PermissionEntity>();

                    while (true)
                    {
                        permissionItem.PermissionNevigatePath.Insert(0, loopHierarchy.PermissionToken);

                        if (loopHierarchy.ParentPermissionID <= 0)
                        {
                            permissionItem.RootPermissionToken = [REDACTED];
                            break;
                        }
                        else
                        {
                            loopHierarchy = queryAllPermissions.FirstOrDefault(x => x.PermissionID == loopHierarchy.ParentPermissionID);
                        }
                    }
                }

                permissionItem.IsAllValueEqual = permissionItem.RoleEnableFlag.All(x => x == "") || permissionItem.RoleEnableFlag.All(x => x == checkFlag);
                resultModel.Permissions.Add(permissionItem);
            }

            return resultModel;
        }



        public List<PermissionViewModel> GetPermissions(int userID)
        {
            var query = PermissionRepository.GetAll();

            return query.Select(x => new PermissionViewModel
            {
                DisplayName = x.PermissionName,
                PageUrl = x.PageUrl,
                PermissionID = x.PermissionID,
            })
            .ToList();
        }


        public bool IsPermissionExist(int userID , List<AdminSysPermissionKey> permissionKeys)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            //主要使用cache來處理
            var myPermissions = UserRoleMappingRepository.GetRolePermissionsByCache(currentUser.Roles);

            //為了避免麻煩 轉小寫判斷
            var tokensList = permissionKeys.Select(x => x.ToString().ToLower()).ToList();

            bool isExist = myPermissions.Any(z=> tokensList.Contains(z.PermissionToken));

            if (isExist)
                return isExist;

            var queryMatch = UserPermissionMappingRepository.GetByFilter(new QueryUserPermissionMappingRequestViewModel
            {
                UserIDs = userID.ToListCollection(),
                PermissionTokenList = tokensList
            });

            return queryMatch.IsNotEmptyNull();


            // 2024/10/28 先不做了 感覺麻煩
            //var queryDisablePermission = UserCustomizedSettingRepository.GetParseModelByUniqueKey(userID, UserCustomizedSettingEnum.UserSpecialDisablePermission);

            //if(queryDisablePermission.SettingValueOfListInt.IsEmptyOrNull())
            //{
            //    return isExist;
            //}

            ////如果用戶有被標記 要特定嚴格檢查
            //var queryPermission = PermissionRepository.GetPagingByFilter(new QueryPermissionRequestViewModel
            //{
            //    PermissionTokens = permissionKeys.Select(x => x.ToString()).ToList()
            //});

            return isExist;
        }


        private static List<UserPermissionItemViewModel> ConvertToViewModels(List<PermissionEntity> query)
        {
            return query.Select(x => new UserPermissionItemViewModel
            {
                Sort = x.Sort,
                DisplayName = x.PermissionName,
                PageUrl = x.PageUrl,
                PermissionID = x.PermissionID,
                PermissionType = x.PermissionType,
                Token = [REDACTED],
                ParentPermissionID = x.ParentPermissionID,
            })
            .OrderBy(z=> z.Sort)
            .ToList();
        }

        private List<PermissionTokenBriefViewModel> GetPermissionsIncludeUserPermission(int userID, List<PermissionTokenBriefViewModel> allPermissions)
        {
            var queryUserPermissions = UserPermissionMappingRepository.GetByFilter(new QueryUserPermissionMappingRequestViewModel
            {
                UserIDs = userID.ToListCollection()
            });

            if (queryUserPermissions.IsEmptyOrNull())
                return allPermissions;

            List<int> Ids = queryUserPermissions.Select(x => x.PermissionID).ToList();

            var permissions = PermissionRepository.GetByIds(Ids);

            var cloneNewList = allPermissions.Select(x => x.ReturnInjecteObject<PermissionTokenBriefViewModel>()).ToList();

            foreach (var entity in permissions)
            {
                bool isExist = allPermissions.Any(x => x.PermissionID == entity.PermissionID);

                if (!isExist)
                {
                    cloneNewList.Add(ConvertToBriefViewModel(entity));
                }
            }

            return cloneNewList;
        }

        private PermissionTokenBriefViewModel ConvertToBriefViewModel(PermissionEntity entity)
        {
            return new PermissionTokenBriefViewModel
            {
                PageUrl = entity.PageUrl,
                Token = [REDACTED],
                PermissionID = entity.PermissionID,
                ParentPermissionID = entity.ParentPermissionID
            };
        }


        private static void NestedFormatSetValue(List<UserPermissionItemViewModel> viewModels)
        {
            Dictionary<int, int> rootMapping = new Dictionary<int, int>();

            //先處理第一層的
            foreach (var item in viewModels)
            {
                if (item.ParentPermissionID == 0)
                {
                    item.RootPermissionID = item.PermissionID;
                    rootMapping.Add(item.PermissionID, item.RootPermissionID);
                }
            }

            int executeTimes = 1;

            while (true)
            {
                //防止無窮迴圈
                if (executeTimes >= 20)
                    break;

                bool isAny = false;

                foreach (var item in viewModels)
                {
                    if (rootMapping.ContainsKey(item.ParentPermissionID) && !rootMapping.ContainsKey(item.PermissionID))
                    {
                        isAny = true;
                        item.RootPermissionID = rootMapping[item.ParentPermissionID];
                        rootMapping.Add(item.PermissionID, item.RootPermissionID);
                    }
                }

                //如果都處理完了 跳出迴圈
                if (!isAny)
                    break;
            }


            var groupBy = viewModels.GroupBy(x => x.RootPermissionID).ToDictionary(x => x.Key, z => z.ToList());

            //不曉得效能是否頗差
            foreach (var item in groupBy)
            {
                var matchItems = viewModels.Where(x => x.RootPermissionID == item.Key).ToList();

                foreach (var sameRootItem in matchItems)
                {
                    sameRootItem.ChildPermissions = matchItems
                        .Where(x => x.ParentPermissionID == sameRootItem.PermissionID)
                        //.Where(x=> x.PermissionType != PermissionTypeEnum.Special.ToInteger()) //不能在後端判斷這個，會導致前端判定permission缺少array
                        .OrderBy(x => x.Sort)
                        .ToList();
                }
            }


        }
    }
}
