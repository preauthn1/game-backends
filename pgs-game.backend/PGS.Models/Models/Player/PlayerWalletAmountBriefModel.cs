using target.Models.Enums;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class PlayerWalletAmountBriefModel
    {
        public int MerchantID { get; set; }

        public MerchantWalletModeEnum WalletMode { get; set; }

        public decimal Amount { get; set; }
    }

    public class PlayerWalletGamePointBriefModel
    {
        public int MerchantID { get; set; }

        public MerchantWalletModeEnum WalletMode { get; set; }

        public decimal Amount { get; set; }

        public decimal GamePoint { get; set; }
    }


}
