using Microsoft.AspNetCore.Mvc;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{




    /// <summary>
    /// 請求參數
    /// </summary>
    public class WalletCashPlayerAmountRequestModel : BaseVendorRequestParameter
    {
        [ModelBinder(typeof(NumberOrBooleanModelBinder))]
        public bool IsIncludeExtraInfo { get; set; }


        [SwaggerApiRequiredField]
        public string PlayerName { get; set; }
    }


    /// <summary>
    /// 回應參數
    /// </summary>
    public class WalletCashPlayerAmountViewModel
    {

        public string Currency { get; set; }

        public decimal Amount { get; set; }

        public decimal AmountWithConvert { get; set; }

        public decimal ConvertRate { get; set; }

        /// <summary>
        /// 有指定特定參數 才會顯示的額外資訊
        /// </summary>
        public WalletCashExtraInfoViewModel ExtraInfo { get; set; }

    }


    public class WalletCashExtraInfoViewModel
    {
        public bool IsTransferLock { get; set; }

        public string PlayerName { get; set; }

    }




}
