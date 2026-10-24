using Newtonsoft.Json;
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
    public class GameInfoRequestModel : BaseVendorRequestParameter
    {
        public int? SpecialOrderSequence { get; set; }


        /// <summary>
        /// 其他程式端需要特殊使用的參術
        /// </summary>
        [SwaggerIgnore]
        [SwaggerIgnoreOutputWhenPagedList]
        public GameListExtraParameter ExtraParameter { get; set; }

    }


    public class GameListExtraParameter
    {
        public bool IsIncludeMerchantDisableGame { get; set; }

        public List<int> FilterGameVendorIDs { get; set; }

    }



    /// <summary>
    /// 回應參數
    /// </summary>
    public class GameInfoViewModel
    {

        /// <summary>
        /// 遊戲編號
        /// </summary>
        public int GameID { get; set; }


        /// <summary>
        /// 遊戲代碼
        /// </summary>
        public string GameCode { get; set; }


        public string GameName { get; set; }


        public string ImageUrl { get; set; }


        public string ImageUrl_H { get; set; }


        /// <summary>
        /// 遊戲分類
        /// </summary>
        public int GameType { get; set; }


        public string GameTypeName { get; set; }


        [JsonIgnore]
        public DateTime CreateTime { get; set; }


        public bool IsPrePaidMode { get; set; }


        public bool IsLaunchAvailable { get; set; }


        public string SupportedOrientation { get; set; }


        public List<i18nGameNameViewModel> GameNameMappings { get; set; }


    }


    public class i18nGameNameTempViewModel : i18nGameNameViewModel
    {
        public int GameID { get; set; }
    }

    public class i18nGameNameViewModel
    {
        public string LanguageCode { get; set; }

        public string Text { get; set; }
    }

}
