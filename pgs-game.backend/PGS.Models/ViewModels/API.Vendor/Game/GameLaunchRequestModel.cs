using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using target.Models.Attributes;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{


    /// <summary>
    /// 這個class有被兩個class繼承
    /// </summary>
    public class BaseGameLaunchRequestModel
    {

        [SwaggerApiRequiredField]
        public int MerchantID { get; set; }


        [SwaggerApiRequiredField]
        public string MerchantSecretKey { get; set; }


        public string Lang { get; set; }


        [SwaggerApiRequiredField]
        public int GameID { get; set; }


        [KeepValueWhenLobbyLaunch]
        [SwaggerIgnore]
        public bool IsTestMode { get; set; }


        /// <summary>
        /// 若指定true。則使用定義好的試玩商戶進行遊戲
        /// Jili不支援
        /// </summary>
        [KeepValueWhenLobbyLaunch]
        [SwaggerIgnore]
        public bool IsTrialModeToLaunch { get; set; }


        /// <summary>
        /// 等同於 IsTrialModeToLaunch
        /// 只是名稱比較好看、直覺 (2023/08) 
        /// </summary> 
        [KeepValueWhenLobbyLaunch]
        [ModelBinder(typeof(NumberOrBooleanModelBinder))]
        public bool IsTrialModeV2{ get; set; }



        /// <summary>
        /// 返回平台網址 (可選填)
        /// </summary>
        [KeepValueWhenLobbyLaunch]
        public string BackUrl { get; set; }


        [SwaggerIgnore]
        [KeepValueWhenLobbyLaunch]
        public string ExtraLink { get; set; }


        /// <summary>
        /// 真人視訊可選參數
        /// </summary>
        [KeepValueWhenLobbyLaunch]
        public string LiveCasinoLevelID { get; set; }


        /// <summary>
        /// 若指定進入大廳，可指定要預設顯示某個遊戲類別
        /// </summary>
        [SwaggerIgnore]
        public int? LobbyDefaultCategory { get; set; }


        /// <summary>
        /// 對應 UserDeviceTypeEnum
        /// </summary>
        [KeepValueWhenLobbyLaunch]
        public int? UserDevice { get; set; }




        /// <summary>
        /// 是否來自Lobby大廳的請求 (會保證不會產生iframe網址)
        /// </summary>

        [SwaggerIgnore]
        [ModelBinder(typeof(NumberOrBooleanModelBinder))]
        public bool IsFromLobbyPage { get; set; }


        [SwaggerIgnore]
        public bool IsFromDemoSite { get; set; }


        /// <summary> 
        /// 是否來自iframe的請求 (會保證不會產生iframe網址)
        /// </summary>
        [SwaggerIgnore]
        [ModelBinder(typeof(NumberOrBooleanModelBinder))]
        public bool IsFromIframe { get; set; }



        /// <summary>
        /// (內部用參數) 不要返回iframe網址
        /// </summary>
        [SwaggerIgnore]
        public bool InternalParameter_DoNotIframeUrl { get; set; }


        /// <summary>
        /// 選填參數，可讓商戶選擇要進哪種等級的廳
        /// </summary>
        [SwaggerIgnore]
        public int? RoomLevelFlagID { get; set; }



        /// <summary>
        /// 參考 HDTypeEnum
        /// </summary>
        [SwaggerIgnore]
        public int DeviceType { get; set; }


        /// <summary>
        /// 額外參數 (程式端使用、此變數並非商戶主動傳過來)
        /// 大部分的參數是三方遊戲商相關邏輯會用到的
        /// </summary>
        [SwaggerIgnore]
        public GameLaunchExtraSettingModel ExtraLaunchSetting { get; set; }


        /// <summary>
        /// 如果是Lobby模式，singleWalletToken 會允許使用同一組sessionToken
        /// </summary>
        [SwaggerIgnore]
        [ModelBinder(typeof(NumberOrBooleanModelBinder))]
        public bool IsAllowDuplicateSessionToken { get; set; }


        /// <summary>
        /// 特殊ReverseAPI的商戶才會用到
        /// </summary>
        [SwaggerIgnore]
        public string ExtraSessionTokenRawLarge { get; set; }

    }


    public class GameLaunchExtraSettingModel
    {
        [SwaggerIgnore]
        public string LiveCasinoLevelMappingValue { get; set; }

        [SwaggerIgnore]
        public int? LobbyLinkWithGameVendorID { get; set; }

        [SwaggerIgnore]
        public string ExtraLink { get; set; }

        [SwaggerIgnore]
        public bool IsTestMode { get; set; }

        [SwaggerIgnore]
        public int? PlayerID { get; set; }

        /// <summary>
        /// 指定用帳務排除的方式進入遊戲 (僅特定遊戲商支援此)
        /// </summary>
        [SwaggerIgnore]
        public bool IsAccountingIgnoreLaunch { get; set; }
    }



    /// <summary>
    /// 請求參數
    /// </summary>
    public class GameLaunchRequestModel : BaseGameLaunchRequestModel
    {
    
        /// <summary>
        /// 非正式使用(內部測試特殊場景)
        /// </summary>
        [SwaggerIgnore]
        public int? SecretParameter_ServerID { get; set; }



        public string PlayerName { get; set; }


        /// <summary>
        /// [單錢包模式] 要帶入此參數，商戶端自訂生命週期token 跟商戶的API溝通需要用到此Token
        /// </summary>
        public string SessionToken { get; set; }

    }



    /// <summary>
    /// 启动游戏請求參數 (VendorAPI_转账模式)
    /// </summary>
    public class GameLaunchVendorApiTransferModeRequestModel : BaseGameLaunchRequestModel
    {


        [SwaggerApiRequiredField]
        public string PlayerName { get; set; }


        /// <summary>
        /// 非正式使用(內部測試特殊場景)
        /// </summary>
        [SwaggerIgnore]
        public int? SecretParameter_ServerID { get; set; }




    }




    /// <summary>
    /// 启动游戏請求參數 (VendorAPI_单钱包模式)
    /// </summary>
    public class GameLaunchVendorApiSingleWalletModeRequestModel : BaseGameLaunchRequestModel
    {

        /// <summary>
        /// [單錢包模式] 要帶入此參數 (用戶唯一識別碼)
        /// </summary>
        [SwaggerApiRequiredField]
        public string PlayerUniqueID { get; set; }



        /// <summary>
        /// [單錢包模式] 要帶入此參數，跟商戶的API溝通需要用到此Token
        /// </summary>
        [SwaggerApiRequiredField]
        public string SessionToken { get; set; }

    }





    /// <summary>
    /// 回應參數
    /// </summary>
    public class GameLaunchViewModel
    {
        public bool IsMultipleServers { get; set; }

        public bool IsLobbyLink { get; set; }

        public bool IsIframeLink { get; set; }

        public bool IsShowGamePageHeader { get; set; }


        /// <summary>
        /// 有指定gameID的情境才會有值
        /// </summary>
        public int PlayerID { get; set; }

        public PlayerTokenJwtModel PlayerJwtModel { get; set; }


        public GameLaunchRequiredInfoViewModel Info { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ExternalVendorLaunchInfoViewModel ExternalVendorInfo { get; set; }

    }

    public class ExternalVendorLaunchInfoViewModel
    {
        public string ExternalVendorUrl { get; set; }
    }


    /// <summary>
    /// 這個model包的東西  client & GameServer 會用到
    /// </summary>
    public class GameLaunchRequiredInfoViewModel
    {
        public GameLaunchRequiredInfoViewModel()
        {
            this.ThirdPartyInfo = new ThirdPartyExtraInfoModel();
        }


        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public string Lang { get; set; }

        public int ServerID { get; set; }

        public int GameID { get; set; }

        public string GameCode { get; set; }

        [ReferType(typeof(MerchantLoadingStyleModeEnum))]
        public int StyleMode { get; set; }

        public string ServerConnection { get; set; }

        public string Access_Token { get; set; }


        /// <summary>
        /// Client端的log開關  0: 關閉，  1: 啟用， 未來也許有第三種? 到時候在定義enum
        /// </summary>
        public int Log { get; set; }

        public ThirdPartyExtraInfoModel ThirdPartyInfo { get; set; }
    }



    /// <summary>
    /// 此model 包含了 API.Vendor launch在使用
    /// 如果有加新欄位 記得要隱藏
    /// </summary>
    public class GameLinkGenerateResultViewModel
    {

        public string Link { get; set; }


        [SwaggerIgnore]
        [JsonIgnore]
        public bool IsShowGamePageHeader { get; set; }


        [SwaggerIgnore]
        [JsonIgnore]
        public int PlayerID { get; set; }


        [SwaggerIgnore]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public GameLaunchGlobalDebugInfo DebugInfo { get; set; }

    }




    public class ThirdPartyExtraInfoModel
    {
        public int GameVendorID { get; set; }


        public string MD5Text { get; set; }
    }



}
