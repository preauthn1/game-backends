using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class MerchantGameSettingViewModel
    {

        public int NumberOfChildMerchant { get; set; }

        public List<MerchantGameSettingItemViewModel> GameSettingItems { get; set; }


    }


    public class MerchantGameSettingItemViewModel
    {
        public int GameID { get; set; }

        public string GameName { get; set; }

        public string GameNameCN { get; set; }

        public string GameNameEN { get; set; }

        public int GameType { get; set; }

        public string GameTypeText { get; set; }

        public int Status { get; set; }

        /// <summary>
        /// 是否GameList這張表 狀態是關閉
        /// </summary>
        public bool IsGameListDisable { get; set; }

        public bool IsEditDisabledDueToSingleWallet { get; set; }

        public decimal ConvertRate { get; set; }

        public int VendorConfigID { get; set; }

        public string VendorConfigName { get; set; }

        public string ImageUrl { get; set; }
    }



}
