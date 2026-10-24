USE [CenterDB]
GO
/****** Object:  StoredProcedure [dbo].[PrPs_User_GetUserInfo]    Script Date: 2019/6/19 下午 06:34:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<获取用户所有信息>
--	2018/03/22	Jason		新增回傳UnionpayRealName	
--	2018/03/22	alan		隱藏真實名與卡號改用Fn_HideAccount Fn_HideRealname
--	2018/03/29	Jason		新增回傳PAYTYPE & Min Max Value
--	2018/04/03	Jason		新增PAYTYPE status判斷
--	2018/04/17	Jason		获取游戏列表改為直接撈取[Area].[GameList]
--	2018/05/10	Jason		#87 依運營需求調整等級判斷
--	2018/06/25	alan		增加客戶端UI順訊位置回傳.  1,2,3,4  =  显示IOS充值,显示在线充值,显示代理充值,显示代理招商
--  2018/07/05	alan		#578 回传游戏热门程度与广告资讯 Behavior 0 qrcode複製 1 直連 2 一鍵入廳 (網址內容由client定義), popular 0 無 1 火2 熱門 3 式玩4 刺激 5推薦
--  2018/07/17	alan		回傳充值金額/顯示兌換的充值金額/顯示其他遊戲的充值金額
--  2018/07/23	alan		先增用戶提交問題時提示本文/是否顯示彈跳公告
--	2018/08/01	alan		回傳客製遊戲清單
--	2018/08/21	alan		回傳是否開啟推廣代理功能
--	2018/9/6	alan		回傳體驗金
--	2019/05/30	Jason		回傳充值返利資格
--	2019/06/06	Eddie		回傳是否強制顯示兌換按鈕與對應之訊息內容
--  2019/06/11  minglung	回傳客服域名
--	2019/06/19	Jason		新增寫入禮碼金額欄位/參數
--	2019/07/02	Mars		回傳玩家VIP頭像框ID
-- =============================================
ALTER PROCEDURE [dbo].[PrPs_User_GetUserInfo]
	@intUserID			INT OUTPUT,	                    --用户ID		
	@intAreaID			int,							--渠道号
	@chvErrMsg			varchar(128) output,			--返回消息
	@chvCertification   varchar(32) OUTPUT
AS
SET NOCOUNT ON


--获取游戏信息
DECLARE 
@intPlatformID		INT,
@lngCashAmount		BIGINT=0,        	--现金
@lngBankAmount		BIGINT=0,        	--银行
@bIsLock			BIT=0,				--是否金币锁
@intNoTalk			BIT=0,				--是否在禁言
@chvServerAddr		VARCHAR(128),		--上锁服务器IP
@intLockGameID		INT,				--上锁游戏ID
@bKick				BIT=0,				--是否踢人
@intClubOpen		BIT=1					--是否顯示推廣代理功能



--Platform params
DECLARE @chvUIFlag varchar(32),@chvUISort varchar(32),@chvPaySort varchar(512),@chvPayAmountConfig varchar(max)

--Systemconfig params
DECLARE @chvConvertRateTip varchar(256),@chvBankDefaultPwdTip varchar(256),
@intServerID int,@chvQuestionContactTip varchar(256),
@tnyConvertBankShow tinyint, @tnyConvertAlipayShow tinyint

--Area params
DECLARE @tnyAnnMsgShow tinyint,@chvAnnMsg varchar(256),@chvPayTips varchar(256), @chvUpGradeMsg varchar(256),
@intShowConvertLimit	BIGINT=0,			--顯示兌換的充值金額 (單位分)
@intShowAllGameLimit	BIGINT=0,			--顯示其他遊戲的充值金額 (單位分)
@intShowVIPAgent	BIGINT=0,			--顯示VIPAGENT的充值金額 (單位分)
@intShowCS			BIGINT=0,			--显示客服的充值金额 (單位分)
@tnyShowConvertBindPhone	TINYINT=0,			--是否綁定手機顯示兌換
@tnyShowAllGameBindPhone	TINYINT=0,			--是否綁定手機顯示其他遊戲
@tnyShowVIPAgentBindPhone	TINYINT=0,			--是否綁定手機顯示VIP Agent
@tnyShowCSBindPhone	TINYINT=0,			--是否綁定手機显示客服
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
@CouponAmount BIGINT=0			--禮碼剩餘金額

--依據遊戲帶入的渠道獲取渠道資料
SELECT	@intPlatformID=[PlatformID], @chvPayTips=PayTips, @chvUpGradeMsg=UpGradeMsg, @chvAnnMsg=AnnMsg, @tnyAnnMsgShow=AnnMsgShow,
@intShowConvertLimit=ShowConvertLimit, @intShowAllGameLimit=ShowAllGameLimit , @intShowVIPAgent=ShowVIPAgent, @intShopNameID=ShopNameID, @intAgentNameID=AgentNameID,
@intShowCS=ShowCS, @intPlaytimeCS=PlaytimeCS, @intPlaytimeConvert=PlaytimeConvert, @intPlaytimeVIPAgent=PlaytimeVIPAgent,
@tnyShowConvertBindPhone=ShowConvertBindPhone, @tnyShowAllGameBindPhone=ShowAllGameBindPhone, @tnyShowVIPAgentBindPhone=ShowVIPAgentBindPhone, @tnyShowCSBindPhone=ShowCSBindPhone,
@tnyShowConvertPopup=ShowConvertPopup, @chvConvertPopupText=ConvertPopupText 
FROM [Area].[Area] WITH (NOLOCK) WHERE [AreaID]=@intAreaID

--取得系統設定
SELECT @chvConvertRateTip=ConvertRateTip, @chvBankDefaultPwdTip=BankDefaultPwdTip, @chvQuestionContactTip=QuestionContactTip, @tnyConvertBankShow=ConvertBankShow, @tnyConvertAlipayShow=ConvertAlipayShow 
FROM ServerInfo.SystemConfig  WITH (NOLOCK)

--取得平台設定
SELECT @chvUIFlag=UIFlag, @chvUISort=UISort, @chvPaySort=PaySort, @chvPayAmountConfig=PayAmountConfig, @Company = Company
FROM Area.[Platform] with (nolock) WHERE PlatformID=@intPlatformID

--如果渠道公告沒有開啟, 則讀取全平台預設公告
if @tnyAnnMsgShow=0
	SELECT @chvAnnMsg=AnnMsg, @tnyAnnMsgShow=AnnMsgShow	FROM [Area].[Area] WITH (NOLOCK) WHERE SetDefault=1


SELECT @chvCertification=CerCode from game.Certification  WITH (NOLOCK) where UserID=@intUserID
SELECT @lngCashAmount=Amount,@CouponAmount = ISNULL(CouponAmount, 0) FROM Money.Cash with(nolock) WHERE UserID=@intUserID
SELECT @lngBankAmount=Amount FROM Money.Bank with(nolock) WHERE UserID=@intUserID

SELECT @bIsLock=1 FROM Game.GameUserList with(nolock) where UserID=@intUserID

--获取用户服务器信息
SELECT @chvServerAddr=gs.ClientAddr,
@intLockGameID=gs.GameID
FROM Game.GameUserList gu WITH (NOLOCK), ServerInfo.GameServerList gs  WITH (NOLOCK)
WHERE gu.UserID=@intUserID
AND gu.ServerID=gs.ServerID

IF @chvServerAddr IS NOT NULL
	SELECT TOP 1 @intServerID=ServerID from ServerInfo.GameServerList WITH (NOLOCK) where ServerID in (SELECT top 1 ServerID FROM Game.GameUserList  WITH (NOLOCK) where UserID=@intUserID)


DECLARE @lngUserLevel bigint,@chvKey varchar(32)
SET @lngUserLevel=ISNULL((SELECT GradeID FROM [Users].[UserInfo] WITH (NOLOCK) WHERE UserID=@intUserID),0)
SET @chvKey=dbo.Fn_GetUserKey(@intUserID,@lngUserLevel)

--獲取客服域名
SELECT @CustomerDomain = Customer from CenterDB.ServerInfo.DomainConfig with (nolock) WHERE Company = @Company


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
0 AS IsTopWindow,							--是否弹窗
@intShowConvertLimit as ShowConvertLimit, --顯示兌換的充值金額 (單位分)
@intShowAllGameLimit as ShowAllGameLimit, --顯示所有遊戲充值金額 (單位分)
@intShowVIPAgent as ShowVIPAgent, --顯示VIPAgent充值金額 (單位分)
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
@bKick AS IsKick,										--是否踢人
CASE WHEN UserType=3 THEN 0 ELSE 1 END  AS IsBindGuest,					--是否绑定游客,
CASE WHEN ISNULL(AlipayAccount, '')='' THEN 0 ELSE 1 END AS IsBindZhifubao,			--是否绑定支付宝
dbo.Fn_HideAccount(AlipayAccount) as AlipayAccount,	--支付宝账号
dbo.Fn_HideRealName(RealName) as RealName,			--支付宝账号姓名
CASE WHEN ISNULL(UnionpayAccount, '')='' THEN 0 ELSE 1 END AS IsBindUnionpayBank,		--是否绑定銀行卡
ISNULL(UnionpayBank,'') as UnionpayBank,				--銀行卡代號
dbo.Fn_HideAccount(UnionpayAccount) as UnionpayAccount,	--銀行卡卡號
dbo.Fn_HideRealName(UnionpayRealName) as UnionpayRealName,	--銀行卡账号姓名
CASE WHEN UnionpayBank IS NULL THEN '' ELSE (select Bankname from Money.UnionpayBankCode  WITH (NOLOCK) where UnionpayBank=Code ) END as UnionpayBankName, -- 銀行名稱
ISNULL(@lngUserLevel,0) as UserLevel,					--用户等级
ISNULL(@chvKey,'') as LevelKey,							--加密字符串
ISNULL(@chvAnnMsg,'') as AnnMsg,						--大厅登录成功后的弹窗公告消息
ISNULL(@tnyAnnMsgShow,0) as AnnMsgShow,					--大厅登录成功后是否顯示弹窗公告消息
ISNULL(@chvConvertRateTip,'') as ConvertRateTipMsg,		--兑换比例提示文本
ISNULL(@chvBankDefaultPwdTip,'') as BankPwdTipMsg,	--银行默认密码提示文本
ISNULL(@chvUpGradeMsg,'') as UpGradeMsg,			--升级账号提示文本
ISNULL(@chvQuestionContactTip,'') as QuestionContactTip, -- 用戶提交問題提示本文
@chvUIFlag as UIFlag,								--客户端UI标记开关，预留
@chvUISort as UISort,								--客戶端UI順訊位置
@chvPaySort as PaySort,								--充值类型排序及开关
@chvPayAmountConfig as PayAmountConfig,				--充值金额配置
'' as PayNotifyMsg,									--充值到账提示(此处不直接返回空字符串)
'' as NotifyFlag,									--通知标记(第1位表示是否有新邮件通知)此处直接返回空字符串
ISNULL(@chvPayTips,'') as PayTips,								--充值提示说明
@intClubOpen as ClubOpen,							--是否開啟推廣代理
ExperienceCash,										--體驗金
@tnyConvertBankShow as ConvertBankShow,				--是否顯示兌換銀行顯示
@tnyConvertAlipayShow as ConvertAlipayShow,			--是否顯示兌換支付寶顯示
ISNULL(@CouponAmount,0) AS CouponAmount
FROM Users.UserInfo u  WITH (NOLOCK)
WHERE u.UserID=@intUserID


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
	SELECT G.GameID, G.Popular, A.IconName, A.AliasName , A.IconTitle, A.Skin
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


/*  取得金額清單 */
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
DECLARE @platformid INT
DECLARE @SumFlowAmount BIGINT

SET @platformid = @intPlatformID

SELECT	@SumFlowAmount = SumFlowAmount
FROM Users.UserInfo WITH (NOLOCK)
WHERE
	UserID = @intUserID

SET @SumFlowAmount = ISNULL(@SumFlowAmount,0)

DECLARE @ChargeBonusInfo	TABLE (FlowAmountNow BIGINT,FlowAmountLimit BIGINT,BonusRate INT,FlowAmountRate INT,EventStatus INT)
DECLARE @UnionPlatform INT

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


SET @chvErrMsg ='获取用户信息成功'

RETURN 1








