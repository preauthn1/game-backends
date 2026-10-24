using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class AGGFishWalletBalanceResponse
    {
        public int code { get; set; }
        public string status { get; set; }
        public AGGFishWalletBalanceDetail result { get; set; }
    }

    public class AGGFishWalletBalanceDetail
    {
        public decimal balance { get; set; }
        public decimal balanceFish { get; set; }
    }


}
