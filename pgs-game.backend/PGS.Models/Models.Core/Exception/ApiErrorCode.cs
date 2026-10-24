using target.Models.CultureText;
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

        Authorize_TokenExpiredAndPleaseReturnToOriginalPlatform = 401031,

        Authorize_LinkHasExpired = 401081,

        Authorize_LinkHasExpiredPleaseContactUs = 401082,

        Authorize_GameListDownloadLinkHasExpired = 401085,

        #endregion



        #region 403 其他邏輯錯誤
        InvalidParameter = 403001,

        InvalidParameter_TextEmpty = 403002,

        InvalidParameter_TextLength = 403003, 
        
        InvalidParameter_NotAccountFormat = 403004,

        InvalidParameter_NotSupportLanguageCodet = 403005,

        InvalidParameter_NotPasswordFormat = 403006,

        InvalidParameter_ConfirmCodeNotMatch = 403009,

        InvalidParameter_ThePasswordTextIsForbidden = 403010,

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

        InvalidParameter_DateTimeRangeTooOld_DataAlreadyExpired = 403028,

        InvalidParameter_AccountMustPrefixFormatPleaseContactAccountProvider = 403031,

        InvalidParameter_NotIPAddressFormat = 403035,

        InvalidParameter_WhiteListAddressEmpty = 403041,

        InvalidParameter_ConfirmPasswordNotEqualPassword = [REDACTED],

        InvalidParameter_OriginalPasswordNotCorrect = 403048,

        InvalidParameter_OrderByColumn = 403055,

        InvalidParameter_ParsingError = 403059,

        InvalidParameter_UrlFormat = 403063,

        InvalidParameter_RestrictUrlFormat = 403064,

        InvalidParameter_TimestampError = 403070,

        InvalidParameter_SignKeyError = 403071,

        InvalidParameter_SignKeyExpired = 403072,

        InvalidParameter_Body = 403079,

        InvalidParameter_SourceValueMustNotEqualTargetValue = 403083,

        InvalidParameter_IsSupportSingleSelectOnly = 403097,

        InvalidParameter_CollectionIsEmptyNull = 403098,

        InvalidParameter_ModelIsNull = 403099,

        InvalidParameter_PleaseSelectMerchantID = 403105,

        InvalidParameter_PageExpiredPleaseRefreshAgain = 403122,

        InvalidParameter_IsNotValidPlayerIdFormat = 403131,

        InvalidParameter_IsNotValidMerchantIDFormat = 403132,

        InvalidParameter_IsNotValidPgsTransactionNoFormat = 403135,

        Forbidden_IPAddressNotAllowOrNeed2FA = 403200,

        Forbidden_IPAddressNotAllow = 403201,

        Forbidden_DuplicateEnterGameServer = 403202,

        Forbidden_EnterGameError_WalletAmountNotEnough = 403203,

        Forbidden_RequiredApiCertificate = 403204,

        Forbidden_AmountNotEnough = 403209,

        Forbidden_DoNotAllowWhenProduction = 403222,
        
        //禁止編輯
        Forbidden_DoNotAllowEdit = 403223,

        //未定義的編號
        Forbidden_NotValidDefineValue = 403224,

        Forbidden_DoNotAllowCreateTargetLayerAgent = 403229,

        Forbidden_SecurityEncryptOrDecryptError = 403231,

        Forbidden_SingleWalletMerchantDoNotSupportGame = 403251,

        Forbidden_AgentNotAllowCreateSingleWallet = 403255,

        Forbidden_AgentNotAllowTransferWallet = 403256,

        Forbidden_PagePermissionNotAllow = 403271,

        Forbidden_WhenPlayingGame_NotAllowTransfer = 403273,

        Forbidden_WatchDataPermissionNotAllow = 403277,

        Forbidden_WatchDataPermissionNotAllow_CompanyIP = 403278,

        Forbidden_NotSupportGameLaunch = 403284,

        Forbidden_NotSupportExternalLinkDisplay = 403288,

        Forbidden_NotSupportThis = 403289,

        Forbidden_NotSupportThisGame = 403290,

        Forbidden_PlayerNotSupportThisGame = 403291,

        Forbidden_RequestTooManyTimes = 403295,

        Forbidden_RequestTooManyTimes_Login = 403296,

        Forbidden_InsufficientPoint = 403299,

        Forbidden_InsufficientPointOrReachMaxTimes = 403301,

        Forbidden_DataTooMuchPleaseTryReduce = 403322,

        Forbidden_EventHasEnd = 403355,

        Forbidden_AGG_NotAllowBuyFeature = 403441,

        Forbidden_SingleWalletMerchantNotSupportMoneyLog = 403468,

        SystemMaintain = 403501,

        GameUnderMaintain = 403508,

        DatabaseBusyAndFeatureUnderMaintain = 403517,

        InvalidStatus_MerchantStatus = 403599,

        InvalidStatus = 403601,

        InvalidStatus_AccountStatus = 403602,

        InvalidStatus_OrganizationStatus = 403603,

        InvalidStatus_DisableNow = 403609,

        InvalidStatus_GameStatus = 403610,

        InvalidStatus_BetRecordAlreadySettlement = 403611,

        InvalidStatus_BetRecordAlreadyCancel = 403612,

        InvalidStatus_InvalidBetRecordOperate = 403613,

        InvalidStatus_EventDisable = 403614,

        InvalidStatus_MerchantGameMappingDisable = 403619,

        BetFail_WalletAmountNotEnough = 403652,

        WalletTranferFail_WalletAmountNotEnough = 403651,

        WalletTranferFail_ApiResponseFail = 403655,

        WalletTranferFail_MerchantCreditAmountNotEnough = 403657,

        ThirdPartyError_SettingDataNotCompleted = 403901,

        ThirdPartyApiFail_SeamlessAPIResponse_AuthError = 403906,

        ThirdPartyApiFail_SeamlessAPIResponse_ParsingFail = 403907,

        ThirdPartyApiFail_SeamlessAPIResponseError = 403908,

        ThirdPartyApiFail_ResponseError = 403911,

        ThirdPartyApiFail_ParsingFail = 403912,

        ThirdPartyApiFail_IsNotValid = 403913,

        ThirdPartyApiFail_SignCheckNotMatch = 403914,

        ThirdPartyApiFail_ResponseDataUnExpected = 403915,

        ThirdPartyApiFail_ResponseMissingRequiredInfo = 403917,

        ThirdPartyApiFail_Timeout = 403918,

        ThirdPartyApiFail_Timeout_SettleProcess = 403919,

        ThirdPartyApiFail_SingleWalletCallbackFail = 403927,


        #endregion

        #region 404 查無資料類

        NotFoundData = 404001,

        NotFoundMerchant = 404002,

        NotFoundPlayer = 404003,

        NotFoundUser = 404004,

        NotFoundUserLogin = 404005,

        NotFoundOrganization = 404006,

        NotFoundPlayerPleaseVendorCreateFirst = 404007,

        NotFoundServer = 404008,
        
        NotFoundGame = 404009,

        NotFoundGameRecord = 404010,

        NotFoundPlayerWallet = 404011,

        NotFoundServerCurrencyMatch = 404012,

        NotFoundRole = 404013,

        NotFoundRecord = 404014,

        NotFoundSessionToken = [REDACTED],

        NotFoundBetRecord = 404016,

        NotFoundBetRecordOrDeleteAlready = 404017,

        NotFoundBetRecordWhenCancelProcess = 404018,

        NotFoundBetRecordWhenPlaceBetRetry = 404019,

        NotFoundMerchantHierarchy = 404027,

        NotFoundAnyParticipant = 404033,

        NotFoundMerchantOrWrongApiKey = [REDACTED],

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

        NotFoundPlayer_IsThisAccountOrPlayerID = 404201,

        NotFoundPerGameLogButBetRecordExist = 404888,

        NotFoundPageOrAPiUrlPathLengthInvalid = 404995,

        NotFoundPageOrAPiUrl = 404999,

        MethodNotAllow = 405001,


        #endregion


        InternalServerError = 500001,

        NotImplement = 500002,

        HttpStatusCodeError = 500008,

        BusinessLogicDeny = 500015,

        BusinessLogicDeny_SettingDataIncorrect = 500017,

        BusinessLogicDeny_SettingDataNotCompleted = 500019,

        BusinessLogicDeny_NotAllowDeleteData_ModifyRecently = 500020,

        BusinessLogicDeny_NotAllowDeleteData_CurrentServer = 500021,

        BusinessLogicDeny_MustFilterMerchantWhenKeywordSearch = 500024,

        BusinessLogicDeny_MustFilterPlayer = 500025,

        BusinessLogicDeny_DuplicateToken = [REDACTED],

        BusinessLogicDeny_DuplicateOrderNo = 500028,

        BusinessLogicDeny_DuplicateDataExist = 500029,

        BusinessLogicDeny_CurrencyNotMatch = 500030,

        BusinessLogicDeny_DuplicateEventType = 500031,

        BusinessLogicDeny_GameAndGameVendorNotMatch = 500033,

        BusinessLogicDeny_MatchDataIsNotSingleOne = 500035,

        BusinessLogicDeny_SimulateErrorOccur = 500041,

        BusinessLogicDeny_OperatorRiskScore = 500046,

        BusinessLogicDeny_SettlementErrorByRiskControl = 500056,

        BusinessLogicDeny_MultiplePlayerNameMatch_PleaseUsePlayerID = 500063,

        BusinessLogicDeny_EventPrizeEmpty = 500077,

        BusinessLogicDeny_EventRuleNotAllPass = 500078,

        BusinessLogicDeny_EventRewardSendFail = 500079,

        BusinessLogicDeny_DatabaseBusyAndUpdateFail = 500081,

        BusinessLogicDeny_PleaseEmptyCollectionDataFirst = 500085,

        BusinessLogicDeny_YourAgentNotAssignReferenceCreditMerchant = 500131,

        BusinessLogicDeny_UpdateSelfCreditIsRejected = 500155,

        BusinessLogicDeny_OverThanMaxHierarchyLayer = 500175,

        BusinessLogicDeny_PleaseFilterAtLeastOnColumn = 500183,

        BusinessLogicDeny_MerchantTypeNotMatch = 500188,

        BusinessLogicDeny_SingleWalletMerchantCanNotCopyCreateByAgentSelf = 500195,

        BusinessLogicDeny_UnexpectedEventSetting = 500211,

        BusinessLogicDeny_SingleWalletEndpointNotSetup = 500235,

        BusinessLogicDeny_DataNotSyncSuccess_PleaseWait10Minutes = 500277,



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



        None = 999001,


    }
}
