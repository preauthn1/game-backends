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
using target.Models.ViewModels.API.AdminSys;
using Microsoft.Extensions.Caching.Memory;
using target.Models.Attributes;
using target.Utility.Extensions.Web;

namespace target.BL.Services
{

    public class AGGGameVendorManagementService : BaseAdminSysService, IAGGGameVendorManagementService
    {

        public MerchantGameSettingRepository MerchantGameSettingRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }


        public AGGJackpotAmountViewModel GetJPCurrentAmount(int userID, QueryJackpotAmountRequestParamete filter)
        {
            filter.RuleFor(z => z.VendorConfigID).Rule_NotZeroNotMinus();

            var queryVendorConfig = VendorConfigRepository.GetById(filter.VendorConfigID);

            if (queryVendorConfig == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            var gameVendorService = ServiceInstanceHelper.InstanceSlotsGameVendorService((GameVendorTypeEnum)queryVendorConfig.VendorID);

            var apiResult = gameVendorService.GetJackpotCurrentAmount(new AGGAccountingGetJackpotAPIParameter
            {
                VendorConfigID = filter.VendorConfigID,
                GameID = 0
            });

            var allEnums = ExtensionOfEnum.GetEnumAllValue<AGGJackpotGroupEnum>();

            var items = new List<AGGJackpotAmountItemViewModel>();

            foreach (var item in apiResult.ResultObject)
            {
                int jpGroup = item.Key;

                bool isEnumDefine = ((AGGJackpotGroupEnum)jpGroup).IsDefinedValue();

                List<decimal> jpValues = item.Value;


                int loopIndex = 0;

                string groupName = MultiLanguageTextHelper.GetEnumsCultureText((AGGJackpotGroupEnum)jpGroup);

                if(!isEnumDefine)
                {
                    groupName = string.Empty;
                }

                foreach (var eachAmount in jpValues)
                {
                    loopIndex++;

                    items.Add(new AGGJackpotAmountItemViewModel
                    {
                        GroupID = jpGroup,
                        GroupName = groupName,
                        IsDefined = isEnumDefine,
                        JPAmount = eachAmount,
                        JPAmountName = $"彩池金額{loopIndex} "
                    });
                }
            }

            items = items.OrderBy(z => z.IsDefined ? 0 : 999)
                .ThenBy(z => z.GroupID)
                .ToList();

            List<int> groupIDs = items.Select(z => z.GroupID).Distinct().ToList();

            foreach (var item in items)
            {
                item.IsOddSequence = groupIDs.IndexOf(item.GroupID) % 2 == 1;
            }

            return new AGGJackpotAmountViewModel
            {
                Items = items
            };
        }

    }
}
