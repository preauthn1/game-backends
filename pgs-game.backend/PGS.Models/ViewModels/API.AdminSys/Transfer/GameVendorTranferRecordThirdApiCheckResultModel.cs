using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class GameVendorTranferRecordThirdApiCheckResultModel
    {
        public bool IsValid { get; set; }

        public int ApiCode { get; set; }

        public string ApiMessage { get; set; }

        public decimal Amount { get; set; }

        public int Type { get; set; }

        public string TypeText { get; set; }

        public string ThirdOrderNo { get; set; }

        public string TransactionNo { get; set; }

        public string PlatformOrderNo { get; set; }


        /// <summary>
        /// 平台錢包
        /// </summary>
        public decimal BeforeAmount { get; set; }

        public decimal AfterAmount { get; set; }

        /// <summary>
        /// 捕魚錢包
        /// </summary>
        public decimal BeforeFishWalletAmount { get; set; }

        public decimal AfterFishWalletAmount { get; set; }

        public string Message { get; set; }

        public string MessageOfFishingRecord { get; set; }

        public string MessageOfPlatformResponse { get; set; }

        public string MessageOfValidateBeforeAmount { get; set; }
    }

}
