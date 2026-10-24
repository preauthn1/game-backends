using target.Models.CultureText;
using target.Utility.Helper;
using System;
using System.ComponentModel;

namespace target.Models.ViewModels.GameVendor
{
    public class ExternalVendorCreateAccountRequestParameter
    {
        public int PlayerID { get; set; }

        public int MerchantID { get; set; }

        /// <summary>
        /// 不一定是傳玩家playerName、也有可能傳playerID
        /// </summary>
        public string PlayerName { get; set; }

        public string Currency { get; set; }

    }


}
