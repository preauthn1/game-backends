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
using target.Models.Attributes;
using target.Models.ViewModels.API.AdminSys;
using target.Utility;
using System.Transactions;

namespace target.BL.Services
{

    public class MerchantCreditChangeLogService : BaseAdminSysService, IMerchantCreditChangeLogService
    {

        public MerchantRepository MerchantRepository { get; set; }

        public MerchantCreditRepository MerchantCreditRepository { get; set; }

        public MerchantCreditChangeLogRepository MerchantCreditChangeLogRepository { get; set; }


        public PagedList<MerchantCreditChangeLogViewModel> GetByPagingFilter(int userID, QueryMerchantCreditChangeLogApiRequestParameterModel filter)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            filter.RuleFor(x => x.CreateTimeBegin).Rule_NotNullDateTime();
            filter.RuleFor(x => x.CreateTimeEnd).Rule_NotNullDateTime();
            filter.RuleFor(x => x.CreateTimeBegin).Rule_StartTimeMustSmallThanEndTime(filter.CreateTimeEnd);

            if (SettingHelper.IsProduction)
            {
                filter.RuleFor(x => x.CreateTimeBegin).Rule_StartTimeAndEndTimeMaxDaysRange(filter.CreateTimeEnd , 8);
            }

            if(filter.MerchantID.HasValue)
            {
                CheckViewMerchantPermission(userID, filter.MerchantID.Value);
                filter.MerchantIDs = filter.MerchantID.ToIntegerListCollection();
            }
            else
            {
                if (!currentUser.IsSuperAdminOrg)
                {
                    filter.FilterMerchantIDsByMultipleHierarchyMerchant = currentUser.MyMultipleHierarchyMerchantIDs;
                }
            }

            var query = MerchantCreditChangeLogRepository.GetPagingByFilter(filter);

            var viewModels = ConvertToNewTypePagingObject<MerchantCreditChangeLogEntity, MerchantCreditChangeLogViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = x.ReturnInjecteObject<MerchantCreditChangeLogViewModel>();
                    return convert;
                })
                .ToList();

            SetExtraProperty(viewModels.Items);
            return viewModels;
        }


        private void SetExtraProperty(List<MerchantCreditChangeLogViewModel> items)
        {
            List<int> merchants = items.Select(z => z.MerchantID).ToList();

            var queryMerchants = DbValueCacheHelper.GetMerchants(merchants);

            foreach (var item in items)
            {
                item.TypeText = MultiLanguageTextHelper.GetEnumsCultureText((MerchantCreditChangeTypeEnum)item.Type);
                item.MerchantName = queryMerchants.FirstOrDefault(z => z.MerchantID == item.MerchantID)?.Name;
            }
        }

    }
}
