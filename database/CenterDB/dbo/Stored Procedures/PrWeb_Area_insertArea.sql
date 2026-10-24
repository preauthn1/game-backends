-- =============================================
-- Author:  <xxxx>
-- Create date: <xxxx>
-- Description:	<创建渠道>
--	2018/7/31	alan　	渠道增加充值金额设定，可显示兑换与全部游戏
--  2018/11/29	alan	擴增渠道資料
--	2019/07/25	Eddie	增加登入彈跳活動視窗欄位(ShowCampaignAtLogin)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Area_InsertArea]
@intAreaID INT,
@chvAreaName varchar(50),
@chvClientPlatform varchar(20),
@intPlatformID INT,
@chvMemo varchar(50) = '',
@intAdminID INT=1,
@intSort INT,
@chvShareURL varchar(512) = '',

@intShowConvertLimit bigint = 0,
@intShowAllGameLimit bigint = 0,
@intShowCS bigint = 0,
@intShowVIPAgent bigint = 0,

@intPlaytimeCS int = 0,
@intPlaytimeConvert int = 0,
@intPlaytimeVIPAgent int = 0,

@chvFAQContent varchar(1000),
@chvPayTips varchar(256),
@chvUpgradeMsg varchar(256),
@chvAnnMsg varchar(256),
@tnyAnnMsgShow tinyint,
@tnyShowCampaignAtLogin tinyint,
@tnyShowFirstDepositAtLogin tinyint,
@tnySetDefault tinyint = 0,
@chvAreaConfig varchar(5000),
@lngDefaultMoney bigint,
@lngGiftMoney bigint,
@chvErrMsg	NVARCHAR(50) OUTPUT		--返回消息		   
AS
BEGIN



	IF EXISTS(SELECT 1 from [CenterDB].[Area].[Area] with(nolock) where [AreaID] = @intAreaID)
	BEGIN
		SET @chvErrMsg = '操作失败:渠道编号已经存在'
		RETURN -1
	END


	INSERT INTO [CenterDB].[Area].[Area] ([AreaID], [AreaName], [ClientPlatform], [PlatformID], [Memo], [AdminID], [Sort], [ShareURL],ShowConvertLimit,ShowAllGameLimit, ShowVIPAgent, ShowCS, PlaytimeCS, PlaytimeConvert, PlaytimeVIPAgent, FAQContent,PayTips,UpGradeMsg,AnnMsg,AnnMsgShow,ShowCampaignAtLogin,ShowFirstDepositAtLogin,SetDefault,AreaConfig,DefaultMoney,GiftMoney)
	VALUES (@intAreaID, @chvAreaName, @chvClientPlatform, @intPlatformID, @chvMemo, @intAdminID, @intSort, @chvShareURL,@intShowConvertLimit,@intShowAllGameLimit, @intShowVIPAgent, @intShowCS, @intPlaytimeCS, @intPlaytimeConvert, @intPlaytimeVIPAgent, @chvFAQContent,@chvPayTips,@chvUpgradeMsg,@chvAnnMsg,@tnyAnnMsgShow,@tnyShowCampaignAtLogin,@tnyShowFirstDepositAtLogin,@tnySetDefault,@chvAreaConfig,@lngDefaultMoney,@lngGiftMoney);
	RETURN 1
END



GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Area_insertArea] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Area_insertArea] TO [xbuser]
    AS [dbo];

