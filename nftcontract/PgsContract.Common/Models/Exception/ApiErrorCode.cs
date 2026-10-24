using System.ComponentModel;

namespace target.Models.Models
{
    public enum ApiErrorCode
    {       
        Success = 0,
        SuccessOK = 1,

        ServerPlayerOutOfRange = 400201,


        //401001~401099 會給前端拿來判斷強制導到登入頁，請謹慎使用
        //401001~401099 會給前端拿來判斷強制導到登入頁，請謹慎使用
        //401001~401099 會給前端拿來判斷強制導到登入頁，請謹慎使用
        //401001~401099 會給前端拿來判斷強制導到登入頁，請謹慎使用
        //401001~401099 會給前端拿來判斷強制導到登入頁，請謹慎使用
        //401001~401099 會給前端拿來判斷強制導到登入頁，請謹慎使用

        #region 401 身分授權類

        Authorize_AccessToken_NotValid = 401001,

        Authorize_AccessToken_Expired = 401002,

        Authorize_TokenHasChanged = 401003,

        Authorize_TokenAvailableGameNotMatch = 401005,

        Authorize_JwtTokenParsingFail = 401008,

        Authorize_JwtTokenVersionNotMatch = 401009,

        #endregion



        #region 403 其他邏輯錯誤

        InvalidParameter = 403001,

        InvalidParameter_TextEmpty = 403002,

        InvalidParameter_TextLength = 403003, 
        
        InvalidParameter_NotAccountFormat = 403004,

        InvalidParameter_NotSupportLanguageCodet = 403005,

        InvalidParameter_ConfirmCodeNotMatch = 403009,

        InvalidParameter_NumberEmptyZero = 403011,

        InvalidParameter_NumberMinus = 403012,

        InvalidParameter_NumberValueTooSmall = 403013,

        InvalidParameter_NumberValueTooLarge = 403014,

        InvalidParameter_NumberValueNotAllowFloat = 403015,

        InvalidParameter_NumberValueMustPlacesTwo = 403016,

        InvalidParameter_MustNumberFormat = 403017,

        InvalidParameter_NumberValueNotAllowNull = 403018,

        InvalidParameter_Collection = 403019,

        InvalidParameter_DateTimeRequired = 403021,

        InvalidParameter_StartTimeMustSmallThanEndTime = 403022,

        InvalidParameter_DateTimeRangeTooLarge = 403023,

        InvalidParameter_DateTimeTooSmall = 403024,

        InvalidParameter_ParsingError = 403059,

        InvalidParameter_TimestampError = 403070,

        InvalidParameter_SignKeyError = 403071,

        InvalidParameter_SignKeyExpired = 403072,

        InvalidParameter_SourceValueMustNotEqualTargetValue = 403083,

        InvalidParameter_IsSupportSingleSelectOnly = 403097,

        InvalidParameter_CollectionIsEmptyNull = 403098,

        InvalidParameter_ModelIsNull = 403099,




        Forbidden_IPAddressNotAllow = 403201,

        Forbidden_DuplicateEnterGameServer = 403202,

        Forbidden_EnterGameError_WalletAmountNotEnough = 403203,

        Forbidden_RequiredApiCertificate = 403204,

        Forbidden_DoNotAllowWhenProduction = 403222,
        
        //禁止編輯
        Forbidden_DoNotAllowEdit = 403223,

        //未定義的編號
        Forbidden_NotValidDefineValue = 403224,


        Forbidden_SingleWalletMerchantDoNotSupportGame = 403251,


        Forbidden_PagePermissionNotAllow = 403271,

        Forbidden_WatchDataPermissionNotAllow = 403272,

        Forbidden_WhenPlayingGame_NotAllowTransfer = 403273,

        Forbidden_NotSupportExternalLinkDisplay = 403288,


        SystemMaintain = 403501,

        GameUnderMaintain = 403508,


        InvalidStatus = 403601,

        InvalidStatus_AccountStatus = 403602,

        InvalidStatus_OrganizationStatus = 403603,

        InvalidStatus_DisableNow = 403609,

        InvalidStatus_GameStatus = 403610,

        InvalidStatus_BetRecordDuplicateSettlement = 403611,

        InvalidStatus_BetRecordAlreadyCancel = 403612,

        InvalidStatus_InvalidBetRecordOperate = 403613,

        InvalidStatus_EventDisable = 403614,

        InvalidStatus_MerchantGameMappingDisable = 403619,

        WalletTranferFail_WalletAmountNotEnough = 403651,

        BetFail_WalletAmountNotEnough = 403652,

        WalletTranferFail_MerchantCreditAmountNotEnough = 403657,

        ThirdPartyError_SettingDataNotCompleted = 403901,

        ThirdPartyApiFail_ResponseError = 403911,

        ThirdPartyApiFail_ParsingFail = 403912,

        ThirdPartyApiFail_IsNotValid = 403913,

        ThirdPartyApiFail_SignCheckNotMatch = 403914,

        ThirdPartyApiFail_ResponseDataUnExpected = 403915,

        ThirdPartyApiFail_Timeout = 403918,

        ThirdPartyApiFail_Timeout_SettleProcess = 403919,

        ThirdPartyApiFail_BlockChainTransactionFail = 403932,

        #endregion

        #region 404 查無資料類

        NotFoundData = 404001,

        NotFoundMerchant = 404002,

        NotFoundPlayer = 404003,

        NotFoundUser = 404004,

        NotFoundUserLogin = 404005,

        NotFoundOrganization = 404006,

        NotFoundServer = 404008,
        
        NotFoundGame = 404009,

        NotFoundGameRecord = 404010,

        NotFoundPlayerWallet = 404011,

        NotFoundServerCurrencyMatch = 404012,

        NotFoundRole = 404013,

        NotFoundRecord = 404014,

        NotFoundSessionToken = [REDACTED],

        NotFoundBetRecord = 404016,

        NotFoundReceipt = 404025,

        NotFoundAnyParticipant = 404033,

        NotFoundAvailableServer_MerchantApiSelected = 404051,

        NotFoundAvailableServer_FlagLevelNotMatch = 404052,

        NotFoundEvent = 404061,

        NotFoundEventPrize = 404062,

        NotFoundEventPrizeRuleBundle = 404063,

        NotFoundEventCounter = 404064,

        NotFoundOrAlreadyDelete = 404088,

        NotFoundGameRTPConfig = 404111,

        NotFoundGameRTPConfigOption_NotSupportTargetRTP = 404112,

        NotFoundTicketOrNotEnough = 404166,

        NotFoundVendorConfig = 404172,

        NotFoundGameVendor = 404184,

        NotFoundPerGameLogButBetRecordExist = 404888,


        #endregion




        InternalServerError = 500001,

        NotImplement = 500002,

        BusinessLogicDeny = 500018,

        BusinessLogicDeny_SettingDataNotCompleted = 500019,

        BusinessLogicDeny_NotAllowDeleteData_ModifyRecently = 500020,

        BusinessLogicDeny_NotAllowDeleteData_CurrentServer = 500021,

        BusinessLogicDeny_DuplicateToken = [REDACTED],

        BusinessLogicDeny_DuplicateOrderNo = 500028,

        BusinessLogicDeny_DuplicateDataExist = 500029,



        BusinessLogicDeny_CurrencyNotMatch = 500030,

        BusinessLogicDeny_DuplicateEventType = 500031,

        BusinessLogicDeny_GameAndGameVendorNotMatch = 500033,

        BusinessLogicDeny_MatchDataIsNotSingleOne = 500035,

        BusinessLogicDeny_SimulateErrorOccur = 500041,

        BusinessLogicDeny_SettlementErrorByRiskControl = 500056,

        BusinessLogicDeny_MultiplePlayerNameMatch_PleaseUsePlayerID = 500063,

        BusinessLogicDeny_EventPrizeEmpty = 500077,

        BusinessLogicDeny_DatabaseBusyAndUpdateFail = 500081,

        BusinessLogicDeny_PleaseEmptyCollectionDataFirst = 500085,

        BusinessLogicDeny_YourAgentNotAssignReferenceCreditMerchant = 500131,

        BusinessLogicDeny_UpdateSelfCreditIsRejected = 500155,

        BusinessLogicDeny_OverThanMaxHierarchyLayer = 500175,

        FunkyApi_InvalidInput = 700400,
        FunkyApi_PlayerIsNotLogin = 700401,
        FunkyApi_InsufficientBalance = 700402,
        FunkyApiBet_Already_Exists_ = 700403,
        FunkyApi_Bet_Was_not_found = 700404,
        FunkyApi_API_Suspended = 700405,
        FunkyApi_over_The_max_winning = 700406,
        FunkyApi_Over_The_max_loss = 700407,
        FunkyApi_Bet_Was_Already_Settled = 700409,
        FunkyApi_Bet_Was_Already_Cancelled = 700410,
        FunkyApi_GameCodeNotDefine = 700499,


    }
}
