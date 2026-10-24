using System.Collections.Generic;

namespace target.Models.ViewModels
{




    public class LobbyApiGenerateLinkParameterModel
    {
        public string SignKey { get; set; }

        public long Ts { get; set; }

        public string PlayerName { get; set; }

        public int GameID { get; set; }

        public string CheckCode { get; set; }

        [SwaggerIgnore]
        public bool IsFromIframe { get; set; }

    }

    public class LobbyApiGetGamesRequestModel
    {
        public string SignKey { get; set; }

        public long Ts { get; set; }

        public string PlayerName { get; set; }

        /// <summary>
        /// 指定查詢 特定的遊戲商是否在維護
        /// </summary>
        public int? TargetMaintainVendorID { get; set; }

    }

    public class LobbyApiGetGamesInfoViewModel
    {
        public List<LobbyGamesViewModel> Games { get; set; }

        public List<LobbyGamesViewModel> RecentlyGames { get; set; }

        public List<AnnouncementItemViewModel> AdvertisementItems { get; set; }

        public bool IsAdvertisementLayout { get; set; }

        public int MerchantID { get; set; }

        public int PlayerID { get; set; }

        public string PlayerName { get; set; }


        /// <summary>
        /// 2023/06 此欄位已不透過setting API、games API提供
        /// </summary>
        public decimal Balance { get; set; }

        public string ExternalExitLink { get; set; }

        public string ExternalExitLinkRedirectPortalFormat { get; set; }

        public bool IsIframeEnable { get; set; }

        public bool IsSoundDefaultEnable { get; set; }

        public bool IsLobbyTransferEnable { get; set; }

        public int BgmType { get; set; }

        public bool IsOpenNewTab { get; set; }

        public string SiteDisplayName { get; set; }

        public string MaintainMsg { get; set; }

        public bool IsAllGameMaintain { get; set; }

        /// <summary>
        /// 商戶進階設置: Common_LobbyStyle
        /// </summary>
        public int LobbyStyle { get; set; }

        /// <summary>
        /// 是否為印尼品牌分類
        /// </summary>
        public bool IsIONSlot { get; set; }

        public List<LobbyGameTypeViewModel> GameTypes { get; set; }
    }

    public class LobbyTagGameModel
    {
        public List<int> InvisibleGameIDs { get; set; }

        public List<int> HotGameIDs { get; set; }

        public List<int> NewGameIDs { get; set; }
    }



    public class LobbyGameTypeViewModel
    {
        public string GameTypeName { get; set; }

        public int GameType { get; set; }
    }

    public class LobbyGenerateLinkResultModel
    {
        public string SiteDisplayName { get; set; }

        public string GameName { get; set; }

        public string Url { get; set; }


        /// <summary>
        /// 若是pgs自製遊戲，會在畫面動態產生一個返回大廳的按鈕 (需啟用iframe)
        /// </summary>
        public string BackUrlLobby { get; set; }


        /// <summary>
        /// 商戶端自訂的返回網址 (前端iframe有一些事件要判斷)
        /// </summary>
        public string ExternalBackUrlKeyword { get; set; }


        /// <summary>
        /// 部分遊戲需要告知前端 需強制使用橫屏的最佳化視覺體驗
        /// </summary>
        public bool IsLandscapeRequired { get; set; }


        /// <summary>
        /// 是否為印尼品牌
        /// </summary>
        public bool IsIONSlot { get; set; }


        public GameLaunchExtraInfoModel ExtraInfo { get; set; }


    }



    public class GameLaunchExtraInfoModel
    {

        public bool IsShowExchangeRateHint { get; set; }


        public bool IsIONSlot { get; set; }

        /// <summary>
        /// 不顯示載入logo
        /// </summary>
        public bool IsDisableLogoAndLoading { get; set; }


        public bool IsShowGamePageHeader { get; set; }

    }



}
