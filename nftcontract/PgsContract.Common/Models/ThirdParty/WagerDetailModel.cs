using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace target.Models.ViewModels.Base
{
    public class WagerDetailModel
    {      
        public long BetAmount { get; set; }

        public long PayoutRate100X { get; set; }

        public long PayoutAmount { get; set; }

        public long SettleTS { get; set; }

        public string SpinResult { get; set; }

        public string WinlineText { get; set; }

        public string PlayerAddress { get; set; }
    }


    public class NFTGameSettingModel
    {
        public string Name { get; set; }
        public string Owner { get; set; }
        public int RTP { get; set; }
    }



}
