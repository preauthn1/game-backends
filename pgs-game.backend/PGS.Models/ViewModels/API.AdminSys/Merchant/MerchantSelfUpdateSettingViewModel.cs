using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class MerchantSelfUpdateSettingViewModel
    {
        public int MerchantID { get; set; }

        public List<MerchantGameFlagSelfSettingViewModel> LobbyVisibleGames { get; set; }

    }


    public class MerchantGameFlagSelfSettingViewModel
    {
        public int GameID { get; set; }

        public string GameName { get; set; }

        public bool IsEnable { get; set; }
    }


}
