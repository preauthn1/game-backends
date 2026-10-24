using System;
using System.Collections.Generic;
using target.BL.Interface;
using target.BL.Services;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Models.Models;
using System.Linq;
using target.DA.Repository;
using target.Utility;
using target.Utility.Helper;
using target.Models.ViewModels.GameVendor;
using target.Models.Enums;
using target.Models;
using target.Utility.Extensions.Web;

namespace target.BL.Services
{

    public class PromotionRecordService : BaseVendorService, IPromotionRecordService
    {
        public EventPrizeRecordRepository EventPrizeRecordRepository { get; set; }

        public EventRepository EventRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }



        /// <summary>
        /// 取得活動領獎紀錄
        /// </summary>
        public PagedList<EventPrizeRecordVendorApiViewModel> GetPrizeRecords(PromotionRecordRequestModel filter)
        {
            filter.RuleFor(x => x.StartTime).Rule_NotEmptyDateTime();
            filter.RuleFor(x => x.EndTime).Rule_NotEmptyDateTime();
                      
            this.GetMerchantAndCheckIsValid(filter.MerchantID, filter.MerchantSecretKey);

            //檢查 VendorAPI設置維護
            CheckIsVendorAPIMaintain();

            var qeuryMerchant = MerchantRepository.GetSingleByCache(filter.MerchantID);

            var filterConvert = filter.ReturnInjecteObject<QueryEventPrizeRecordRequestViewModel>();
            filterConvert.MerchantIDs = filter.MerchantID.ToListCollection();
            filterConvert.PrizeTypes = filter.EventPrizeType.ToIntegerListCollection();

            if (filter.IsFilterByCreateTime)
            {
                filterConvert.CreateTimeBegin = filter.StartTime;
                filterConvert.CreateTimeEnd = filter.EndTime;
            }
            else
            {
                filterConvert.ReceiveTimeBegin = filter.StartTime;
                filterConvert.ReceiveTimeEnd = filter.EndTime;
            }

            List<int> availablePrizeType = EventDefinitionHelper.PrizeTypes_VendorAPI_VisiblePrizeTypes();

            if (filterConvert.PrizeTypes.IsNotEmptyNull())
            {
                if(!filterConvert.PrizeTypes.All(x=> availablePrizeType.Contains(x)))
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("invalid prizeType"));
                }
            }
            else
            {
                filterConvert.PrizeTypes = availablePrizeType;
            }

            //用字串搜尋成本太高，會反查IDs再用playerID過濾
            if (!string.IsNullOrWhiteSpace(filter.PlayerName))
            {
                filterConvert.PlayerIDs = GetMerchantPlayerIDByPlayerName(filter.MerchantID , filter.PlayerName);
            }

            var query = EventPrizeRecordRepository.GetPagingByFilter(filterConvert);

            var pageModels = ConvertToNewTypePagingObject<EventPrizeRecordDetailQuery, EventPrizeRecordVendorApiViewModel>(query);
            pageModels.Items = query.Items.Select(x => 
                {
                    var convert = DataModelAndDbValueConvertHelper.ConvertEventPrizeRecordDetailToViewModel(x);
                    return convert;
                })
                .ToList();

            SetExtraProperty(pageModels.Items , filter);
            return pageModels;
        }


        private void SetExtraProperty(List<EventPrizeRecordVendorApiViewModel> items , PromotionRecordRequestModel filter)
        {
            if (items.IsEmptyOrNull())
                return;

            List<int> eventIDs = items.Select(z => z.EventID).Distinct().ToList();

            var queryEvent = EventRepository.GetByIds(eventIDs);

            foreach (var item in items)
            {
                var marchEvent = queryEvent.FirstOrDefault(z => z.EventID == item.EventID);
                item.EventName = DataModelAndDbValueConvertHelper.GetEventDisplayName(marchEvent);
            }

            if(filter.IsExchangeToOriginalRate)
            {
                var rate_to_original = GetExchangeToOriginalRate(filter.IsExchangeToOriginalRate, filter.MerchantID);

                if (rate_to_original != 0.0M && rate_to_original != 1.0M)
                {
                    foreach (var item in items)
                    {
                        item.PrizeCashAmount = item.PrizeCashAmount / rate_to_original;
                    }
                }
            }
        }

    }
}
