using System.ComponentModel;

namespace game.Entity
{
    public enum ApiErrorCode
    {

        [Description("亲，游戏伺服器人数已达到满载、请稍后再做尝试")]
        ServerPlayerOutOfRange = 400201,

        //401001~401099 會給前端拿來判斷強制導到登入頁，請謹慎使用
        //401001~401099 會給前端拿來判斷強制導到登入頁，請謹慎使用

        [Description("亲，您的登入信息已失效，请尝试重新登入")]
        Authorize_AccessToken_NotValid = 401001,
        
        [Description("亲，您的登入信息已失效，请尝试重新登入*")]
        Authorize_AccessToken_Expired = 401002,

        [Description("亲，您的登入信息已失效，请尝试重新登入(**)")]
        Authorize_TokenHasChanged = 401003,

        [Description("亲，您的登入信息已失效，请尝试重新登入(*)")]
        Authorize_JwtTokenParsingFail = 401008,

        [Description("亲，您的登入信息已失效或身分验签版本已过旧，请尝试重新登入")]
        Authorize_JwtTokenVersionNotMatch = 401009,
        





        [Description("亲，您所填写的参数有缺少或参数格式不正确")]
        InvalidParameter = 403001,

        [Description("亲，您所填写的文字栏位不得为空")]
        InvalidParameter_TextEmpty = 403002,

        [Description("亲，您所填写的文字栏位字数不合规定")]
        InvalidParameter_TextLength = 403003,

        [Description("亲，您的帐号格式不正确或包含不合法的字元")]
        InvalidParameter_NotAccountFormat = 403004,

        [Description("亲，您输入的语系代码不是我们支援的格式")]
        InvalidParameter_NotSupportLanguageCodet = 403005,

        [Description("确认码不符合")]
        InvalidParameter_ConfirmCodeNotMatch = 403009,

        [Description("亲，您所填写的数字不得为0")]
        InvalidParameter_NumberEmptyZero = 403011,

        [Description("亲，您所填写的数字不得负数")]
        InvalidParameter_NumberMinus = 403012,

        [Description("亲，您填写的金额数值过低")]
        InvalidParameter_NumberValueTooSmall = 403013,

        [Description("亲，您填写的金额数值过高")]
        InvalidParameter_NumberValueTooLarge = 403014,

        [Description("亲，您输入的数字栏位容许小数点格式")]
        InvalidParameter_NumberValueNotAllowFloat = 403015,

        [Description("亲，您输入的数字栏位是必须填写的、不允许空值")]
        InvalidParameter_NumberValueNotAllowNull = 403018,

        [Description("亲，不允许阵列为空")]
        InvalidParameter_Collection = 403019,

        [Description("亲，您所填写的时间栏位为必要项目")]
        InvalidParameter_DateTimeRequired = 403021,

        [Description("参数格式无法解析")]
        InvalidParameter_ParsingError = 403059,

        [Description("时间戳记格式或时间范围不合法")]
        InvalidParameter_TimestampError = 403070,

        [Description("加密签验证失败，请检查您的参数信息")]
        InvalidParameter_SignKeyError = 403071,

        [Description("加密签验证失败，请检查您的参数信息*")]
        InvalidParameter_SignKeyExpired = 403072,        

        [Description("不允许传入物件型别为空null*")]
        InvalidParameter_ModelIsNull = 403099,




        [Description("您的IP位址不是我们合法的服务区")]
        Forbidden_IPAddressNotAllow = 403201,

        [Description("您已进入其他游戏房，请等待该局游戏结束再重新入房")]
        Forbidden_DuplicateEnterGameServer = 403202,

        [Description("金币不足，您当前的金币未达进房所需标准")]
        Forbidden_EnterGameError_WalletAmountNotEnough = 403203,

        [Description("请求已被拒绝，接口证书验证不合法")]
        Forbidden_RequiredApiCertificate = 403204,

        [Description("请求已被拒绝，您请求的位址在正式环境禁止使用")]
        Forbidden_DoNotAllowWhenProduction = 403222,

        [Description("您没有访问此页面的权限")]
        Forbidden_PagePermissionNotAllow = 403271,

        [Description("您没有查看此资料信息的权限")]
        Forbidden_WatchDataPermissionNotAllow = 403272,

        [Description("亲，您的帐号目前游戏房中，您必须先退出游戏房才能进行转帐")]
        Forbidden_WhenPlayingGame_NotAllowTransfer = 403273,



        [Description("状态不合法或已设为关闭")]
        InvalidStatus = 403601,

        [Description("您的帐号状态被设置为禁用，无法进行此操作")]
        InvalidStatus_AccountStatus = 403602,

        [Description("您的组织或商户状态被设置为禁用，无法进行此操作")]
        InvalidStatus_OrganizationStatus = 403603,

        [Description("亲，你所选择的厂商当前正在例行维护中，请等候维修结束再进行相关操作")]
        InvalidStatus_StatusIsMaintain = 403604,

        [Description("当前状态设置为关闭，请您稍后再做尝试")]
        InvalidStatus_DisableNow = 403609,

        [Description("转帐失败，余额不足")]
        WalletTranferFail_WalletAmountNotEnough = 403651,

        [Description("您启动的游戏已限制入场金币不得过低")]
        Forbidden_WalletAmountNotEnough_Dollars = 403652,


        [Description("商户资料尚未正确设置或未完成设定")]
        ThirdPartyError_SettingDataNotCompleted = 403901,

        [Description("商户对接回应失败代码")]
        ThirdPartyApiFail_ResponseError = 403911,

        [Description("商户对接回应失败代码")]
        ThirdPartyApiFail_ParsingFail = 403912,

        [Description("商户对接回应失败代码，商户加密信息验证失败")]
        ThirdPartyApiFail_IsNotValid = 403913,

        [Description("商户对接回应失败代码，商户加密信息验证失败")]
        ThirdPartyApiFail_SignCheckNotMatch = 403914,

        [Description("商户对接回应失败、回应逾时")]
        ThirdPartyApiFail_Timeout = 403915,





        #region 404 查無資料類

        [Description("查无资料")]
        NotFoundData = 404001,

        [Description("查无游戏商户信息")]
        NotFoundGameMerchant = 404002,

        [Description("查无玩家信息")]
        NotFoundPlayer = 404003,

        [Description("查无用户信息")]
        NotFoundUser = 404004,

        [Description("查无角色信息")]
        NotFoundRole = 404005,

        [Description("查无组织")]
        NotFoundOrganization = 404006,

        [Description("查无遊戲伺服器")]
        NotFoundServer = 404008,

        [Description("查无指定的游戏类型")]
        NotFoundGame = 404009,

        [Description("查无指定的游戏进房纪录")]
        NotFoundGameRecord = 404010,
        
        [Description("查无指定的钱包资讯")]
        NotFoundPlayerWallet = 404011,

        [Description("查无指定的活动")]
        NotFoundEvent = 404016,

        [Description("亲，没有符合您当前指定条件的游戏房、可能原因为您的游戏金币不足")]
        NotFoundAvailableServer_MerchantApiSelected = 404051,

        [Description("查无渠道游戏配置或尚未开通此游戏")]
        NotFoundAreaGameList = 404052,

        [Description("非对称定义的数字范围")]
        NotFoundDefineEnumValue = 404909,

        #endregion




        [Description("亲，我们的系统目前忙碌中无法回应，请稍后再做尝试")]
        InternalServerError = 500001,

        [Description("系统在此项目尚未建置与实作")]
        NotImplement = 500002,

        [Description("亲，我們的系統不允许此操作")]
        BusinessLogicDeny = 500018,

        [Description("资料尚未正确设置或未完成设定")]
        BusinessLogicDeny_SettingDataNotCompleted = 500019,

        [Description("短时间内有活动或更新纪录的伺服器无法删除")]
        BusinessLogicDeny_NotAllowDeleteData_ModifyRecently = 500020,

        [Description("尚有其他玩家在伺服器当中，不允许删除")]
        BusinessLogicDeny_NotAllowDeleteData_CurrentServer = 500021,

        [Description("资料已重复，请更换其他命名或选择项目")]
        BusinessLogicDeny_DuplicateDataExist = 500029,

        [Description("更新钱包金币发生问题、是否余额不足呢？")]
        BusinessLogicDeny_UpdateWalletBalanceFail = 500061,

        [Description("更新失败，来源与目标不得为相同数值")]
        BusinessLogicDeny_SourceAndTargetAreEqual = 500062,

    }
}
