using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class GameListViewModel
    {

        public int GameID { get; set; }

        public string GameName { get; set; }

        public string GameName_CN { get; set; }

        public string GameName_EN { get; set; }

        public string GameCode { get; set; }

        /// <summary>
        /// 對應 GameTypeEnum
        /// </summary>
        public int GameType { get; set; }


        public string GameTypeText { get; set; }

        /// <summary>
        /// 對應 GameVendorTypeEnum
        /// </summary>
        public int GameVendorType { get; set; }

        public string LanguageCode { get; set; }

        public string GameVendorTypeText { get; set; }

        public int Status { get; set; }

        public bool IsSkipInsertZeroBet { get; set; }

        public DateTime CreateTime { get; set; }

        public string ThirdPartyGameCode { get; set; }

        public string ThirdPartySecondCode { get; set; }

        public bool IsThirdPartyGame { get; set; }


        /// <summary>
        /// 列舉是否有定義
        /// </summary>
        public bool IsGameListEnumDefined { get; set; }


        public bool IsSupportSingleWallet { get; set; }

        public bool IsSupportRaiseProcess { get; set; }

        public bool IsForbiddenGameLaunch { get; set; }

        public string SupportCurrency { get; set; }

        public string GameImageUrl { get; set; }

        public string GameImageUrl2 { get; set; }

        public string Memo { get; set; }


        public int NumberOfMerchants { get; set; }

        public string SupportedOrientationCN { get; set; }

        public string SupportedOrientationEN { get; set; }

        public string ExtraTextForKeywordSearch { get; set; }
    }



    public class GameMerchantsMappingInfoViewModel
    {

        public int GameID { get; set; }

        public string GameName { get; set; }

        public int GameVendorType { get; set; }


        public List<GameMerchantsViewModel> Merchants { get; set; }
    }


    public class GameMerchantsViewModel
    {
        public int MerchantID { get; set; }

        public string MerchantName { get; set; }

        public int VendorConfigID { get; set; }

        public string VendorConfigName { get; set; }

        public decimal ConvertRate { get; set; }

    }


}
