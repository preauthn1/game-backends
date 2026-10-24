using System.Collections.Generic;

namespace target.BL.Services
{
    public class ExcelParseResultModel
    {
        public List<CurrencyLimitBetRowItemModel> CurrencyItems { get; set; }

    }

    public class CurrencyLimitBetRowItemModel
    {
        public string Currency { get; set; }

        public string VipLevel { get; set; }

        public decimal Min { get; set; }

        public decimal Max { get; set; }
    }

}
