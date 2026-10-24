using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class EventPrizeBundleExtraParameterModel
    {
        public bool IsAlwaysRule { get; set; }

        public bool IsDisabled { get; set; }

        public List<EventRuleItemEntity> RuleItems { get; set; }

    }



    public class EventViewModel
    {

        public int EventID { get; set; }

        public int EventType { get; set; }

        public int EventTemplateType { get; set; }

        public string EventTypeText { get; set; }

        public string EventTypeMemo { get; set; }

        public string DisplayName { get; set; }

        public string DisplayNameExternal { get; set; }

        public int Status { get; set; }

        public int? MerchantSelectTemplateID { get; set; }

        public string MerchantSelectTemplateName { get; set; }

        public int MatchMultiplePrizeId { get; set; }

        public string MatchMultiplePrizeName { get; set; }

        public string StatusText { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }

        public DateTime CreateTime { get; set; }

        public string CreateBy { get; set; }

        public DateTime UpdateTime { get; set; }

        public string UpdateBy { get; set; }

        public string AvailableMerchantName { get; set; }

        public int NumberOfMerchants { get; set; }

        public int NumberOfGames { get; set; }

        public bool IsExpired { get; set; }

        public bool IsAlreadyEnd { get; set; }

        public bool IsInProgress { get; set; }

        public List<string> ExtraTags { get; set; }

    }

    public class EventPrizeRecordViewModel
    {
        public long Id { get; set; }

        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public int MerchantID { get; set; }

        public string MerchantName { get; set; }

        public int GameID { get; set; }

        public string GameName { get; set; }

        public int EventID { get; set; }

        public string EventName { get; set; }

        public int EventPrizeBundleID { get; set; }

        public int EventPrizeItemID { get; set; }

        public int EventPrizeType { get; set; }

        public string EventPrizeTypeName { get; set; }

        public decimal AmountOrQuantity { get; set; }

        public decimal? UpdateAmount { get; set; }

        public int Status { get; set; }

        public string StatusText { get; set; }

        public string Memo { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? ReceiveTime { get; set; }

        public bool IsAmountRequired { get; set; }

        public bool IsSingleWallet { get; set; }

        public string ReferenceNo { get; set; }

        public decimal? ReferenceAmount { get; set; }

        public string PgsTransactionNo { get; set; }

        public string ThirdOrderNo { get; set; }

        public string DisplayOrderNo2 { get; set; }

        public long? NotifyQueueID { get; set; }

        public decimal? ReferValidAmount { get; set; }

        public string ReferThirdPartyEventID { get; set; }

    }


}
