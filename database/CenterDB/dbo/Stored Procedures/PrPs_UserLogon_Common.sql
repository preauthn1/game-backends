
-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<大厅登录_登录公用逻辑>
-- Changelog:
--	2018/03/15	alan		修改增加回傳銀行卡帳號資訊
--	2018/03/22	Jason		新增回傳UnionpayRealName
--	2018/03/22	alan		隱藏真實名與卡號改用Fn_HideAccount Fn_HideRealname
--	2018/03/27	Jason		新增呼叫LOGDBLINKSERVER寫入紀錄及取相關變數值
--	2018/03/29	Jason		新增回傳PAYTYPE & Min Max Value
--	2018/04/03	Jason		新增PAYTYPE status判斷
--	2018/04/17	Jason		获取游戏列表改為直接撈取[Area].[GameList]
--	2018/05/10	Jason		#87 依運營需求調整等級判斷
--	2018/05/14	Jason		#229同一個帳號限制不能登不同的渠道 
--	2018/05/18	alan		#229改成同一個平台下的不同渠道,帳號能互通
--	2018/06/21	alan		檢查IP是否正確
--	2018/06/25	alan		增加客戶端UI順訊位置回傳.  1,2,3,4  =  显示IOS充值,显示在线充值,显示代理充值,显示代理招商
--  2018/07/05	alan		#578 回传游戏热门程度与广告资讯 Behavior 0 qrcode複製 1 直連 2 一鍵入廳 (網址內容由client定義), popular 0 無 1 火2 熱門 3 式玩4 刺激 5推薦	
--  2018/07/17	alan		回傳充值金額/顯示兌換的充值金額/顯示其他遊戲的充值金額
--  2018/07/23	alan		先增用戶提交問題時提示本文/是否顯示彈跳公告
--	2018/08/01	alan		回傳客製遊戲清單
--	2018/08/21	alan		回傳是否開啟推廣代理功能
--  2018/9/4	alan		檢查並自動綁定推廣代理下線 
--	2018/9/6	alan		回傳體驗金
--	2019/04/14	jason		需要針對全平台在新舊用戶登入時即使產生全民代理QR CODE
--	2019/05/30	Jason		回傳充值返利資格
--	2019/06/06	Eddie		回傳是否強制顯示兌換按鈕與對應之訊息內容
--  2019/06/11  minglung	回傳客服域名
--	2019/06/19	Jason		新增寫入禮碼金額欄位/參數
--	2019/07/02	Mars		回傳玩家VIP頭像框ID
--	2019/07/12	Eddie		回傳綁定送金幣的金額
--	2019/07/25	Eddie		回傳登入彈跳活動視窗(ShowCampaignAtLogin)
--	2019/08/05	Eddie		增加回傳登入彈跳首充返利活動視窗欄位(ShowFirstDepositAtLogin)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserLogon_Common]
	@intAreaID INT,										--区域ID
	@intUserID			INT,	                        --用户ID
	@tnyLogonType		TINYINT,                   		--登录类型  0 账号密码登录 1 保存密码登录 2 快速登录 3 机器人登录 (alan: 此參數沒用到)
	@tnyHDType			TINYINT,                      	--硬件类型
	@chvHDCode			VARCHAR(64),                  	--硬件机器码
	@chvVersion 		VARCHAR(16),					--客户端版本号
	@sintPlazaServerID	smallint,						--广场服务器编号
	@chvIPAddress		VARCHAR(15),                	--IP地址	
	@chvCertification	VARCHAR(32) OUTPUT,     		--证书
	@chvErrMsg			VARCHAR(128) OUTPUT		      	--错误消息
AS
SET NOCOUNT ON

--檢查IP是否正確
IF (dbo.Fn_CheckValidIP(@chvIPAddress)=0)
BEGIN
	SET @chvIPAddress='未知IP'
END


--获取游戏信息
DECLARE 
@lngCashAmount		BIGINT ,        	--现金
@lngBankAmount		BIGINT ,        	--银行
@bIsLock			BIT,				--是否金币锁
@intNoTalk			BIT,				--是否在禁言
@chvServerAddr		VARCHAR(128),		--上锁服务器IP
@intLockGameID		INT,				--上锁游戏ID
@intServerPort		INT,				--上锁服务器端口
@bKick				BIT,				--是否踢人
@intUserInfoRet 	INT,					
@intClubOpen		BIT=1				--是否顯示推廣代理功能


--Systemconfig params
DECLARE @chvConvertRateTip varchar(256),@chvBankDefaultPwdTip varchar(256),
@intServerID int,@chvQuestionContactTip varchar(256),
@tnyConvertBankShow tinyint, @tnyConvertAlipayShow tinyint

--Platform params
DECLARE @chvUIFlag varchar(32),@chvUISort varchar(32),@chvPaySort varchar(512),@chvPayAmountConfig varchar(max)

--Area params
DECLARE @tnyAnnMsgShow tinyint,@chvAnnMsg varchar(256),@chvPayTips varchar(256), @chvUpGradeMsg varchar(256),
@tnyShowCampaignAtLogin	tinyint,			--登入彈跳活動視窗
@tnyShowFirstDepositAtLogin tinyint,		--登入彈跳首充返利活動視窗
@intShowConvertLimit	BIGINT=0,			--顯示兌換的充值金額 (單位分)
@intShowAllGameLimit	BIGINT=0,			--顯示其他遊戲的充值金額 (單位分)
@intShowVIPAgent	BIGINT=0,			--顯示VIP Agent的充值金額 (單位分)
@intShowCS			BIGINT=0,			--显示客服的充值金额 (單位分)
@tnyShowConvertBindPhone	TINYINT=0,			--是否綁定手機顯示兌換
@tnyShowAllGameBindPhone	TINYINT=0,			--是否綁定手機顯示其他遊戲
@tnyShowVIPAgentBindPhone	TINYINT=0,			--是否綁定手機顯示VIP Agent
@tnyShowCSBindPhone	TINYINT=0,			--是否綁定手機显示客服
@intPlatformID	INT,
@intDefaultArea int,
@intShopNameID tinyint=1,
@intAgentNameID tinyint=1,
@intPlaytimeCS int=0,			--游戏多少时间显示客服
@intPlaytimeConvert int=0,		--游戏多少时间显示兑换
@intPlaytimeVIPAgent int=0,		--游戏多少时间显示代理充值
@tnyShowConvertPopup	TINYINT=0,			--是否強制顯示兌換按鈕
@chvConvertPopupText nvarchar(256),			--強制顯示兌換按鈕時的訊息內容
@Company int,					--營運商
@CustomerDomain	varchar(500),	--客服域名
@CouponAmount BIGINT=0,			--禮碼剩餘金額
@intGiftMoney	Bigint=0		--綁定贈送金幣數(單位:分)


--生成证书
SET @chvCertification =REPLACE(NEWID(), '-', '')

EXEC @intUserInfoRet=[dbo].PrPs_UserLogon_GetUserInfo
@intUserID,
@tnyHDType,
@chvCertification output,
@sintPlazaServerID,
@chvIPAddress, 
@lngCashAmount OUTPUT,
@lngBankAmount OUTPUT,
@intNoTalk OUTPUT,
@bIsLock OUTPUT,
@chvServerAddr OUTPUT,
@intLockGameID OUTPUT,
@bKick OUTPUT,
@CouponAmount OUTPUT,
@chvErrMsg output

IF @intUserInfoRet <> 1
BEGIN
	SET @chvErrMsg=@chvErrMsg --'用戶資料取得失敗'
	RETURN -1
END


--獲取渠道資料
SELECT	@intPlatformID=PlatformID, @chvPayTips=PayTips, @chvUpGradeMsg=UpGradeMsg, @chvAnnMsg=AnnMsg, @tnyAnnMsgShow=AnnMsgShow,@tnyShowCampaignAtLogin=ShowCampaignAtLogin,@tnyShowFirstDepositAtLogin=ShowFirstDepositAtLogin,@intShowConvertLimit=ShowConvertLimit, 
@intShowAllGameLimit=ShowAllGameLimit , @intShowVIPAgent=ShowVIPAgent, @intShopNameID=ShopNameID, @intAgentNameID=AgentNameID,
@intShowCS=ShowCS, @intPlaytimeCS=PlaytimeCS, @intPlaytimeConvert=PlaytimeConvert, @intPlaytimeVIPAgent=PlaytimeVIPAgent,
@tnyShowConvertBindPhone=ShowConvertBindPhone, @tnyShowAllGameBindPhone=ShowAllGameBindPhone, @tnyShowVIPAgentBindPhone=ShowVIPAgentBindPhone, @tnyShowCSBindPhone=ShowCSBindPhone,
@tnyShowConvertPopup=ShowConvertPopup, @chvConvertPopupText=ConvertPopupText,@intGiftMoney=GiftMoney
FROM [Area].[Area] WITH (NOLOCK) WHERE [AreaID]=@intAreaID

--取得系統設定
SELECT @chvConvertRateTip=ConvertRateTip,@chvBankDefaultPwdTip=BankDefaultPwdTip, @chvQuestionContactTip=QuestionContactTip, @tnyConvertBankShow=ConvertBankShow, @tnyConvertAlipayShow=ConvertAlipayShow 
FROM ServerInfo.SystemConfig with (nolock)

--取得平台設定
SELECT @chvUIFlag=UIFlag, @chvUISort=UISort, @chvPaySort=PaySort, @chvPayAmountConfig=PayAmountConfig, @Company = Company
FROM Area.[Platform] with (nolock) WHERE PlatformID=@intPlatformID

--獲取客服域名
SELECT @CustomerDomain = Customer from CenterDB.ServerInfo.DomainConfig with (nolock) WHERE Company = @Company

--如果渠道公告沒有開啟, 則讀取全平台預設公告
if @tnyAnnMsgShow=0
	SELECT @chvAnnMsg=AnnMsg, @tnyAnnMsgShow=AnnMsgShow	FROM [Area].[Area] WITH (NOLOCK) WHERE SetDefault=1

/*  
1. #229同一個帳號限制不能登不同的渠道 
2. 改成同一個平台下的不同渠道,帳號能互通
 */
----------------------------------------------------------------------------------------------
DECLARE @intUserPlatformID INT
DECLARE @intUserAreaID INT
DECLARE @SumFlowAmount BIGINT
--取得用戶渠道/總流水
SELECT @intUserAreaID=AreaID,
		@SumFlowAmount = SumFlowAmount
FROM Users.UserInfo WITH (NOLOCK) WHERE UserID=@intUserID
--取得用戶渠道對應的平台
SELECT @intUserPlatformID=[PlatformID] FROM [Area].[Area] WITH (NOLOCK) WHERE [AreaID]=@intUserAreaID
--比較是否為相同的平台
IF @intUserPlatformID <> @intPlatformID
BEGIN
	SET @chvErrMsg='您的帐号有误，请登入正确的平台'
	RETURN -1
END
----------------------------------------------------------------------------------------------

--更新最后登录数据
DECLARE @UserCreateTime	DATETIME
DECLARE @NowTime		DATETIME=GETDATE()
DECLARE @Temp_UserInfo	TABLE (CreateTime DATETIME,GradeID INT)

UPDATE Users.UserInfo
SET 
LastLogonIPAddress=@chvIPAddress,
LastLogonHDType=@tnyHDType,
LastLogonHDCode=@chvHDCode,
LastLogonTime=@NowTime,
SumLoginTimes=SumLoginTimes + 1
OUTPUT INSERTED.CreateTime,INSERTED.GradeID INTO @Temp_UserInfo
WHERE UserID=@intUserID

SELECT	@UserCreateTime=CreateTime FROM @Temp_UserInfo

DECLARE @lngUserLevel BIGINT,@chvKey varchar(32)
SET @lngUserLevel=ISNULL((SELECT GradeID FROM @Temp_UserInfo),0)
SET @chvKey=dbo.Fn_GetUserKey(@intUserID,@lngUserLevel)
----------------------------------------------------------


--全民代理
declare @platformid int
select	@platformid = platformid from area.area with (nolock) where areaid = @intAreaID
if exists (select top 1 1 from Club02.[dbo].[DomainSetting] with (nolock) where platformid = @platformid)
begin
	if(@platformid = 119)
	begin
		EXEC [Club02].[dbo].[PrWeb_UserAgent_AddUserQRCode]
			@intUserID = @intUserID,
			@intplatformid = @platformid,
			@intAreaID = @intAreaID,
			@chvMsg = ''
	end
end



if @chvServerAddr IS NOT NULL
begin
	SELECT TOP 1 @intServerID=ServerID from ServerInfo.GameServerList with (nolock) where ServerID in(SELECT top 1 ServerID FROM Game.GameUserList with (nolock) where UserID=@intUserID)
end


--获取用户充值到账的通知
DECLARE @chvNotifyFlag varchar(32)		--通知标记(第1位充值到账通知,第2位新邮件通知,后面备用)
DECLARE @chvPayNotifyMsg VARCHAR(MAX)

--其他機制取代
--SELECT top 3 
--	@chvPayNotifyMsg=ISNULL(@chvPayNotifyMsg,'') + case when @chvPayNotifyMsg IS NOT NULL THEN '
--	' ELSE '' END + CONVERT(varchar(16),CreateTime,120) +'您充值' + dbo.Fn_ShowMoney(PayAmount)+'元,已经到账' FROM Pay.PayNotify with (nolock) WHERE UserID=@intUserID ORDER BY CreateTime DESC
----清除通知
--DELETE FROM Pay.PayNotify WHERE UserID=@intUserID
--IF @chvPayNotifyMsg IS NOT NULL
--BEGIN
--	SET @chvNotifyFlag=dbo.Fn_SetFlag(@chvNotifyFlag,1,'1')
--	SET @chvPayNotifyMsg=@chvPayNotifyMsg
--END


--是否有新邮件
IF EXISTS(
SELECT 1 FROM Message.MessageList m WITH(NOLOCK),Message.ReceiveUser mu WITH(NOLOCK) WHERE m.MsgID=mu.MsgID 
AND mu.UserID=@intUserID
AND mu.Status=0
AND m.CreateTime > dbo.Fn_EmailStartTime()
)
BEGIN
	SET @chvNotifyFlag=dbo.Fn_SetFlag(@chvNotifyFlag,2,'1')
END	


SELECT 
dbo.Fn_HideTel(u.LoginName) as LoginName,	--账号名
u.NickName,									--昵称
u.UserType,									--用户类型
u.Sex,										--性别
u.FaceID AS  HeadID,						--头像ID
ISNULL(u.AvatarFrame, 1) AS AvatarFrame,	--头像框
(u.SumCharge + u.SumAgentCharge) as SumCharge, --充值金額
@lngCashAmount AS CashAmount,				--现金数量
@lngBankAmount as BankAmount,				--银行现金数量
1 AS IsTopWindow,							--是否弹窗
@intShowConvertLimit as ShowConvertLimit, --顯示兌換的充值金額 (單位分)
@intShowAllGameLimit as ShowAllGameLimit, --顯示所有遊戲充值金額 (單位分)
@intShowVIPAgent as ShowVIPAgent, --顯示VIPAGENT 充值金額 (單位分)
@intShowCS as ShowCS, --顯示客服充值金額 (單位分)

@tnyShowConvertBindPhone as ShowConvertBindPhone, --是否綁定手機顯示兌換
@tnyShowAllGameBindPhone as ShowAllGameBindPhone, --是否綁定手機顯示所有遊戲
@tnyShowVIPAgentBindPhone as ShowVIPAgentBindPhone, --是否綁定手機顯VIP代理
@tnyShowCSBindPhone as ShowCSBindPhone, --是否綁定手機顯示客服

@tnyShowConvertPopup as ShowConvertPopup,		--是否強制顯示兌換按鈕
ISNULL(@chvConvertPopupText,'') as ConvertPopupText,		--強制顯示兌換按鈕時的訊息內容

@intPlaytimeCS as PlaytimeCS,  --游戏多久显示客服
@intPlaytimeConvert as PlaytimeConvert,  --游戏多久显示兑换
@intPlaytimeVIPAgent as PlaytimeVIPAgent, --游戏多久显示充值代理
@intShopNameID as ShopNameID,
@intAgentNameID as AgentNameID,
@intNoTalk AS NoTalk,						--是否禁言
@bIsLock AS IsGaming,							--是否游戏中
ISNULL(@chvServerAddr,'') AS ServerAddr,			--游戏服务器地址
CASE WHEN @intServerID IS NULL THEN '' ELSE dbo.Fn_GetRoomName(@intServerID) END as ServerName,			--游戏服务器名称
ISNULL(@intLockGameID,0) AS LockGameID,				--上锁游戏ID
@bKick AS IsKick,							--是否踢人
CASE WHEN UserType=3 THEN 0 ELSE 1 END  AS IsBindGuest,					--是否绑定游客,

CASE WHEN ISNULL(AlipayAccount, '')='' THEN 0 ELSE 1 END AS IsBindZhifubao,			--是否绑定支付宝
dbo.Fn_HideAccount(AlipayAccount) as AlipayAccount,	--支付宝账号
dbo.Fn_HideRealName(RealName) as RealName,			--支付宝账号姓名

CASE WHEN ISNULL(UnionpayAccount, '')='' THEN 0 ELSE 1 END AS IsBindUnionpayBank,		--是否绑定銀行卡
ISNULL(UnionpayBank,'') as UnionpayBank,				--銀行卡代號
dbo.Fn_HideAccount(UnionpayAccount) as UnionpayAccount,	--銀行卡卡號
dbo.Fn_HideRealName(UnionpayRealName) as UnionpayRealName, --銀行卡账号姓名

CASE WHEN UnionpayBank IS NULL THEN '' ELSE (select Bankname from Money.UnionpayBankCode with (nolock) where UnionpayBank=Code ) END as UnionpayBankName, -- 銀行名稱
ISNULL(@lngUserLevel,0) as UserLevel,					--用户等级
ISNULL(@chvKey,'') as LevelKey,							--加密字符串
ISNULL(@chvAnnMsg,'') as AnnMsg,						--大厅登录成功后的弹窗公告消息
ISNULL(@tnyAnnMsgShow,0) as AnnMsgShow,					--大厅登录成功后是否顯示弹窗公告消息
ISNULL(@tnyShowCampaignAtLogin,0) as ShowCampaignAtLogin,					--大厅登录成功后是否顯示弹跳活動視窗
ISNULL(@tnyShowFirstDepositAtLogin,0) as ShowFirstDepositAtLogin,			--大厅登录成功后是否顯示弹跳首充返利活動視窗
ISNULL(@chvConvertRateTip,'') as ConvertRateTipMsg,		--兑换比例提示文本
ISNULL(@chvBankDefaultPwdTip,'') as BankPwdTipMsg,	--银行默认密码提示文本
ISNULL(@chvUpGradeMsg,'') as UpGradeMsg,			--升级账号提示文本
ISNULL(@chvQuestionContactTip,'') as QuestionContactTip, -- 用戶提交問題提示本文
@chvUIFlag as UIFlag,								--客户端UI标记开关，预留
@chvUISort as UISort,								--客戶端UI順訊位置
@chvPaySort as PaySort,								--充值类型排序及开关
@chvPayAmountConfig as PayAmountConfig,				--充值
ISNULL(@chvPayNotifyMsg,'') as PayNotifyMsg,		--充值到账提示，没有提示则为空字符串
ISNULL(@chvNotifyFlag,'') as NotifyFlag,			--通知标记
ISNULL(@chvPayTips,'') as PayTips,					--充值提示说明
@intClubOpen as ClubOpen,							--是否開啟推廣代理
ISNULL(ExperienceCash,0) as ExperienceCash,			--體驗金
@tnyConvertBankShow as ConvertBankShow,				--是否顯示兌換銀行顯示
@tnyConvertAlipayShow as ConvertAlipayShow,			--是否顯示兌換支付寶顯示
@CouponAmount AS CouponAmount,
@intGiftMoney as GiftMoney							--綁定贈送金幣數(單位:分)
FROM Users.UserInfo u with (nolock)
WHERE u.UserID=@intUserID

--增加登入log
DECLARE @tnyLogType TINYINT=2
EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrPs_AddUserLog]
	@tnyLogType		= @tnyLogType,
	@intUserID		= @intUserID,
	@dtStatTime		= @NowTime,
	@tnyHDType		= @tnyHDType,
	@chvHDCode		= @chvHDCode,
	@dtCreateTime	= @UserCreateTime,
	@intServerID	= @intServerID,
	@intAreaID		= @intAreaID,
	@chvIPAddress	= @chvIPAddress,
	@intUserLevel	= @lngUserLevel,
	@intPlatformID	= @intPlatformID



/*  獲取遊戲清單 */
-----------------------------------------------------------
--如果渠道有設定遊戲清單, 則獲取渠道遊戲清單, 否則獲取全平台預設的清單
IF EXISTS(select top 1 1 from Area.GameList with (nolock) where PlatformID=@intPlatformID and AreaID=@intAreaID)
BEGIN
	SELECT G.GameID,G.Popular, A.IconName, A.AliasName, A.IconTitle, A.Skin
	FROM Area.GameList G with (nolock) LEFT JOIN Area.GameAliasName A with (nolock) ON G.IconID=A.IconID
	WHERE G.AreaID=@intAreaID
	AND [Status]=1
	ORDER BY Sort DESC
END
ELSE
BEGIN
	SELECT @intDefaultArea=AreaID From Area.Area with (nolock) WHERE SetDefault=1
	SELECT G.GameID, G.Popular, A.IconName, A.AliasName, A.IconTitle, A.Skin
	FROM Area.GameList G with (nolock) LEFT JOIN Area.GameAliasName A with (nolock) ON G.IconID=A.IconID
	WHERE G.AreaID=@intDefaultArea
	AND [Status]=1
	ORDER BY Sort DESC
END
-----------------------------------------------------------


/*  新增回傳PAYTYPE & Min Max Value  */
-----------------------------------------------------------
SELECT	PayTypeID=TypeID,
		MinValue=MIN(MinValue),
		MaxValue=MAX(MaxValue)
FROM [CenterDB].[Pay].[PayType] WITH (NOLOCK)
WHERE
	Status=1
	AND MinValue IS NOT NULL
	AND MaxValue IS NOT NULL
GROUP BY TypeID
-----------------------------------------------------------


/*  获取广告列表  */
-----------------------------------------------------------
--如果渠道沒有設定廣告, 則獲取全平台預設的廣告內容
IF EXISTS (SELECT 1 FROM [Area].[Advertisement] WITH (NOLOCK) WHERE PlatformID=@intPlatformID and AreaID=@intAreaID AND [Status]=1)
	SELECT Picture, Link, Behavior  FROM [Area].[Advertisement] WITH (NOLOCK) WHERE PlatformID=@intPlatformID and AreaID=@intAreaID AND [Status]=1
ELSE
BEGIN
	SELECT @intDefaultArea=AreaID From Area.Area with (nolock) WHERE SetDefault=1
	SELECT Picture, Link, Behavior FROM [Area].[Advertisement] WITH (NOLOCK) WHERE AreaID=@intDefaultArea  AND [Status]=1
END
-----------------------------------------------------------

/*  取得固定金額清單 */
-----------------------------------------------------------
DECLARE @UnionnPlatform TABLE (PlatformID int,GWID int,TypeID int,[Status] tinyint)

INSERT INTO @UnionnPlatform 
SELECT PlatformID,GWID,TypeID,[Status] 
FROM [CenterDB].[Pay].[UnionPlatformPayType] with (nolock) 
WHERE PlatformID = @intPlatformID
AND Status=1

SELECT P.GWID,P.TypeID,P.TypeName,P.AmountSort
FROM [CenterDB].[Pay].[PayType] P with (nolock) 
JOIN @UnionnPlatform S on P.GWID=S.GWID and P.TypeID=S.TypeID
WHERE P.Status=1
-----------------------------------------------------------

/*  充值返利資格  */
-----------------------------------------------------------
DECLARE @ChargeBonusInfo	TABLE (FlowAmountNow BIGINT,FlowAmountLimit BIGINT,BonusRate INT,FlowAmountRate INT,EventStatus INT)
DECLARE @UnionPlatform INT

SET @SumFlowAmount = ISNULL(@SumFlowAmount,0)

INSERT INTO @ChargeBonusInfo
EXEC [dbo].[PrPs_Event_ChargeBonusAmount_Get]
	@intUserID = @intUserID,
	@chvMsg = ''

IF EXISTS(SELECT TOP 1 1 FROM [Area].[UnionPlatform] WHERE PlatformID = @platformid)
BEGIN
	SET @UnionPlatform = 1
END
ELSE
BEGIN
	SET @UnionPlatform = 0
END
/*
[UnionPlatform] => 0 (是否為聯運 0:方周 1:聯運)
[FlowAmountLimit] => 4500 (流水限制)
[FlowAmountNow] => 900 (目前流水)
[BonusRate] => 5 (返利趴數)
[OpenStatus] => 1 (是否達標)
[FlowAmountRate] => 9999(流水大於充值金額倍數)
[Status] => 1(目前跟OpenStatus相同)
[CustomerDomain] => 客服域名
*/
SELECT	UnionPlatform = @UnionPlatform,
		FlowAmountLimit,
		FlowAmountNow = @SumFlowAmount,
		BonusRate,
		OpenStatus = CASE WHEN @SumFlowAmount >= FlowAmountLimit AND FlowAmountLimit > 0 THEN 1 ELSE 0 END,
		FlowAmountRate,
		Status = EventStatus,
		CustomerDomain = @CustomerDomain
FROM @ChargeBonusInfo

-----------------------------------------------------------

SET @chvErrMsg='登录成功'
RETURN 1



GO


