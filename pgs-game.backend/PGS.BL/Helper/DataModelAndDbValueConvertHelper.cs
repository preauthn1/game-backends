using target.Models;
using System;
using System.Collections.Generic;
using System.Text;
using target.Utility.Extensions;
using target.Models.Models;
using target.Models.Enums;
using target.Models.Table;
using target.BL.Interface;
using target.Utility.IOC;
using target.Utility.Extensions.Web;
using target.Models.ViewModels;
using target.Utility.Helper;

namespace target.BL.Services
{
    public class DataModelAndDbValueConvertHelper
    {
        public static EventPrizeRecordVendorApiViewModel ConvertEventPrizeRecordDetailToViewModel(EventPrizeRecordDetailQuery queryPrize)
        {
            if (queryPrize == null)
                return null;

            var convert = ConvertEventPrizeRecordToViewModel(queryPrize);
            convert.ReferCampaignID = queryPrize.ReferThirdPartyEventID;
            return convert;
        }


        /// <summary>
        /// 至少有三個地方使用 (遊戲商callback、排程callbackRetry、VendorAPI紀錄)
        /// 並且有迴圈呼叫。因此可能要避免在裡面有DB查詢
        /// ※※　有幾個地方是呼叫上面的 ConvertEventPrizeRecordDetailToViewModel
        /// </summary>
        public static EventPrizeRecordVendorApiViewModel ConvertEventPrizeRecordToViewModel<T>(T queryPrize) where T: EventPrizeRecordEntity
        {
            if (queryPrize == null)
                return null;

            var convert = queryPrize.ReturnInjecteObject<EventPrizeRecordVendorApiViewModel>();
            convert.EventPrizeRecordID = queryPrize.Id;
            convert.PrizeCashAmount = queryPrize.UpdateAmount.HasValue ? queryPrize.UpdateAmount.Value : 0;
            convert.QuantityOrReferenceRatio = queryPrize.AmountOrQuantity;
            convert.StatusText = MultiLanguageTextHelper.GetEnumsCultureText((EventPrizeRecordStatusEnum)queryPrize.Status);
            convert.EventPrizeTypeText = MultiLanguageTextHelper.GetEnumsCultureText((EventPrizeTypeEnum)queryPrize.EventPrizeType);
            convert.EventID = queryPrize.EventID;
            convert.GameID = queryPrize.GameID;
            convert.GameName = DbMultiLanguageTextHelper.GetGameDisplayName(convert.GameID);
            convert.PrizeRecordTransactionNo = queryPrize.PgsTransactionNo;
            return convert;
        }

       
        public static string GetEventDisplayName(EventEntity queryEvent)
        {
            if (queryEvent == null)
                return string.Empty;

            string targetEventName = string.Empty;

            //有填寫外部名稱則優先顯示外部名稱
            if (!string.IsNullOrWhiteSpace(queryEvent.DisplayNameExternal))
            {
                targetEventName = queryEvent.DisplayNameExternal;
            }
            else
            {
                targetEventName = queryEvent.DisplayName;
            }

            targetEventName = targetEventName ?? "";
            targetEventName = targetEventName.Replace("<br>", "");

            if (queryEvent.EventType == (int)EventTypeEnum.RankCompetition)
            {
                string beginText = queryEvent.BeginTime.AddHours(8).ToString("MM/dd");
                string endText = queryEvent.EndTime.AddHours(8).ToString("MM/dd");
                targetEventName = $"《{beginText}-{endText}》" + targetEventName;
            }

            return targetEventName;
        }

    }
}
