using Newtonsoft.Json;
using System.Collections.Generic;

namespace target.Models.ViewModels.GameVendor
{
   
    public class AGGAvailableRTPMode
    {
        public string nature { get; set; }
    }

    public class AGGAvailableRTPGameSettingModel
    {
        public int @default { get; set; }
        public Dictionary<string , int> list { get; set; }

        /// <summary>
        /// 這欄是我方自己定義的 (非API回的)
        /// </summary>
        public List<int> AvailableRTPs { get; set; }


        public AGGAvailableRTPMode mode { get; set; }
        public int threshold { get; set; }
        public string modeHigh { get; set; }
        public string modeLow { get; set; }
    }


}
