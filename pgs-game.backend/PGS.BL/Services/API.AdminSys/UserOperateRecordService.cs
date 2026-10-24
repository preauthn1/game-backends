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

    public class UserOperateRecordService : BaseAdminSysService, IUserOperateRecordService
    {
        public UserOperateRecordRepository UserOperateRecordRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public UserRepository UserRepository { get; set; }


        public PagedList<UserOperateRecordViewModel> GetByPagingFilter(int userID, QueryUserOperateRecordApiRequestParameterModel filter)
        {
            filter.OperateTypes = filter.OperateType.ToIntegerListCollection();

            filter.RuleFor(x => x.CreateTimeBegin).Rule_StartTimeAndEndTimeMaxDaysRange(filter.CreateTimeEnd, 31);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            if (!currentUser.IsSuperAdminOrg)
            {
                if (currentUser.ReferenceCreditMerchant > 0)
                {
                    filter.ReferenceCreditMerchantIDs = currentUser.AvailableMerchantIDs;
                }
                else
                {
                    if (HasTargetPermission(userID, "Records_UserOperateRecord_SearchAllOrg"))
                    {
                        filter.OrganizationID = currentUser.OrganizationID;
                    }
                    else
                    {
                        filter.UserID = currentUser.UserID;
                    }
                }
            }

            if (filter.CreateIPAddress.ContainsWihtoutCase("*"))
            {
                filter.CreateIPAddressLikeQuery = filter.CreateIPAddress.Replace("*", "");
                filter.CreateIPAddress = string.Empty;
            }

            var query = UserOperateRecordRepository.GetPagingByFilter(filter);

            var viewModels = ConvertToNewTypePagingObject<UserOperateRecordEntity, UserOperateRecordViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = x.ReturnInjecteObject<UserOperateRecordViewModel>();
                    return convert;
                })
                .ToList();

            SetExtraProperty(viewModels.Items);
            return viewModels;
        }


        public PagedList<UserOperateLockPlayerRecordViewModel> GetPlayerLockRecordByPagingFilter(int userID, QueryUserOperateRecordApiRequestParameterModel filter)
        {
            filter.MerchantIDs = filter.MerchantID.ToIntegerListCollection();
            filter.MerchantIDs = FilterAvailablePermissionMerchantIDs(userID, filter.MerchantIDs);

            filter.TargetIDs = filter.PlayerID.ToIntegerListCollection();
            filter.OperateTypes = new List<int>
                {
                     UserOperateTypeEnum.Player_UnlockStatus.ToInteger(),
                     UserOperateTypeEnum.Player_LockStatus.ToInteger(),
                };

            if (!string.IsNullOrWhiteSpace(filter.PlayerName))
            {
                filter.TargetIDs = PlayerRepository.GetPlayerIDsByPlayerName(filter.PlayerName);
            }

            var query = UserOperateRecordRepository.GetPagingByFilter(filter);

            var viewModels = ConvertToNewTypePagingObject<UserOperateRecordEntity, UserOperateLockPlayerRecordViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = ConvertToViewModels(x);
                    return convert;
                })
                .ToList();

            return viewModels;
        }

        private UserOperateLockPlayerRecordViewModel ConvertToViewModels(UserOperateRecordEntity query)
        {
            var convert = query.ReturnInjecteObject<UserOperateLockPlayerRecordViewModel>();
            convert.MerchantName = MerchantRepository.GetSingleByCache(query.MerchantID)?.Name;
            convert.PlayerName = PlayerRepository.GetBriefByCache((int)query.TargetID)?.PlayerName;
            convert.PlayerID = (int)query.TargetID;
            convert.UpdateBy = UserRepository.GetSingleByCache(query.UserID)?.UserName;

            return convert;
        }


        private void SetExtraProperty(List<UserOperateRecordViewModel> items)
        {
            List<int> userIDs = items.Select(z => z.UserID).ToList();

            string selectColumns = " UserID , UserName , Status ";
            var queryUsersBrief = UserRepository.GetByFilter(new QueryUserRequestViewModel
            {
                UserIDs = userIDs

            }, selectColumns);

            foreach (var item in items)
            {
                var typeEnum = (UserOperateTypeEnum)item.OperateType;

                var matchUser = queryUsersBrief.FirstOrDefault(z => z.UserID == item.UserID);

                item.OperateTypeText = MultiLanguageTextHelper.GetEnumsCultureText(typeEnum);
                item.OperateUserName = matchUser?.UserName;

                if (item.Memo == typeEnum.ToString())
                {
                    item.Memo = string.Empty;
                }

                if (matchUser != null && matchUser.Status != NormalActiveStatus.Enable.ToInteger())
                {
                    var statusText = MultiLanguageTextHelper.GetEnumsCultureText((NormalActiveStatus)matchUser.Status);
                    item.Memo += $"《Status：{statusText}》";
                }

                if (item.DeviceType.HasValue)
                {
                    item.DeviceTypeText = ((EazyDeviceTypeEnum)item.DeviceType).ToString();
                }
            }
        }

    }
}
