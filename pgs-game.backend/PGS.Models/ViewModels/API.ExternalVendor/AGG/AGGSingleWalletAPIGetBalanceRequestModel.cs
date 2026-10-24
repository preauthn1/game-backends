using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class AGGSingleWalletAPIGetBalanceRequestModel
    {
        /// <summary>
        /// 對應我方 playerID
        /// </summary>
        public int UserID { get; set; }


        public string RequestId { get; set; }


    }




    public class AGGSingleWalletAPIGetBalanceResponseDataModel
    {
        public List<AGGSingleWalletEachBalanceItemViewModel> Balances { get; set; }


        public string StatusCode { get; set; }


        public string RequestId { get; set; }

    }


    public class AGGSingleWalletEachBalanceItemViewModel
    {
        public string CurrencyCode { get; set; }

        public decimal Amount { get; set; }

        public bool IsIso4217 { get; set; }
    }


    public class AGGCurrencyModel
    {
        public string currencyCode { get; set; }

        public bool isIso4217 { get; set; }
    }



    public class AGGSingleWalletCurrencyItemViewModel
    {
        public string CurrencyCode { get; set; }

        public bool IsIso4217 { get; set; }
    }





}
