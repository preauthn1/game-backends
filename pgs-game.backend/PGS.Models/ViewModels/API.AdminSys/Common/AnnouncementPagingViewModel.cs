using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class CurrencyManagementViewModel
    {
        public List<CurrencyItemViewModel> Items { get; set; }

    }

    public class CurrencyItemViewModel
    {
        public string Currency { get; set; }

        public string CurrencyText { get; set; }

        public bool IsHot { get; set; }

        public bool IsDisableEdit { get; set; }

        public decimal ExchangeRateToTWD { get; set; }

        public decimal ExchangeRateToUSDT { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public string LastUpdateTimeDiffText { get; set; }

        /// <summary>
        /// 超過N天沒更新匯率 視為棄用的幣別
        /// </summary>
        public bool IsDiscardManyDays { get; set; }
    }


    public class CurrencyUdpateRequestModel
    {
        public string Currency { get; set; }

        /// <summary>
        /// 目前支援 USDT/TWD (基本上就是DB中的兩個欄位)
        /// </summary>
        public string ExchangeCurrency { get; set; }

        public decimal ExchangeRate { get; set; }

    }


    public class CurrencyRelativeModel
    {

        public CurrencyRelativeModel ()
        {

        }

        public CurrencyRelativeModel(string currency , decimal exchangeRateDiff)
        {
            this.Currency = currency;
            this.ExchangeRateDiff = exchangeRateDiff;
        }

        public string Currency { get; set; }

        public decimal ExchangeRateDiff { get; set; }
    }



}
