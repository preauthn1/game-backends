using target.Models.Enums;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{


    public class ThirdPartyVendorFetchGameListConvertModel
    {

        public string GameName { get; set; }


        /// <summary>
        /// 組出 agg_三方gameCode 這種格式
        /// 組出 jili_三方gameCode 這種格式
        /// </summary>
        public string GameCode { get; set; }


        public string ThirdPartyGameCode { get; set; }

        /// <summary>
        /// 對應 GameVendorTypeEnum
        /// </summary>
        public int GameVendorType { get; set; }


        public int GameType { get; set; }


        public Dictionary<LanguageEnum , string> LanguageText { get; set; }

    }


}
