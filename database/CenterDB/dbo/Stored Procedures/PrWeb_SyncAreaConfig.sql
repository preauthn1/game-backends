
-- =============================================
-- Author:  alan
-- Create date: 20190506
-- Description:	同步其它渠道
--	2019/07/29	Eddie	增加同步平台活動相關程式
--	2019/08/05	Eddie	增加同步[登入彈跳首充返利活動] ShowFirstDepositAtLogin
--	2019/08/08	Eddie	增加同步「兑换未达成强制显示」與「兑换禁止提示文本」(Redmine:1707)
-- =============================================
CREATE procedure [dbo].[PrWeb_SyncAreaConfig]
@intPlatformID int,					--平台號
@intAreaID int,						--同步參考的渠道號
@chvErrMsg	VARCHAR(50) OUTPUT		--返回消息
As
BEGIN

set xact_abort on


IF NOT EXISTS(Select 1 from Area.Platform WITH (NOLOCK) where PlatformID = @intPlatformID)
BEGIN
	SET @chvErrMsg = '错误:平台编号不存在'
	RETURN -1 
END



DECLARE @chvFAQ varchar(5000)
DECLARE @chvUpGradeMsg varchar(1000)
DECLARE @chvAnnMsg varchar(1000)
DECLARE @chvPayTips varchar(1000)
DECLARE @chvAreaConfig varchar(5000)
DECLARE @chvPayAmountConfig varchar(1000)
DECLARE @intDefaultMoney int = 0
DECLARE @intGiftMoney int=50
DECLARE @intAnnMsgShow tinyint
DECLARE	@intShowCampaignAtLogin	tinyint
DECLARE @intShowFirstDepositAtLogin tinyint
DECLARE @intShowConvertLimit bigint
DECLARE @intShowAllGameLimit bigint
DECLARE @intShowVIPAgent bigint
DECLARE @intShowCS bigint
DECLARE @intShowConvertBindPhone tinyint
DECLARE @intShowAllGameBindPhone tinyint
DECLARE @intShowVIPAgentBindPhone tinyint
DECLARE @intShowCSBindPhone tinyint
DECLARE @intPlaytimeCS int
DECLARE @intPlaytimeConvert int
DECLARE @intPlaytimeVIPAgent int
DECLARE @intShowConvertPopup tinyint
DECLARE @chvConvertPopupText nvarchar(256)

DECLARE @intShopNameID tinyint
DECLARE @intAgentNameID tinyint

--取得參考渠道的設定
select 
@chvFAQ=FAQContent,
@chvPayTips=PayTips,
@chvUpGradeMsg=UpGradeMsg,
@chvAnnMsg=AnnMsg,
@intAnnMsgShow=AnnMsgShow,
@intShowCampaignAtLogin=ShowCampaignAtLogin,
@intShowFirstDepositAtLogin=ShowFirstDepositAtLogin,
@chvAreaConfig=AreaConfig,
@intDefaultMoney=DefaultMoney,
@intGiftMoney=GiftMoney,
@intShowConvertLimit=ShowConvertLimit,
@intShowAllGameLimit=ShowAllGameLimit,
@intShowVIPAgent=ShowVIPAgent,
@intShowCS=ShowCS,
@intShowConvertBindPhone =ShowConvertBindPhone,
@intShowAllGameBindPhone=ShowAllGameBindPhone,
@intShowVIPAgentBindPhone=ShowVIPAgentBindPhone, 
@intShowCSBindPhone=ShowCSBindPhone,
@intPlaytimeCS=PlaytimeCS,
@intPlaytimeConvert=PlaytimeConvert,
@intPlaytimeVIPAgent=PlaytimeVIPAgent,
@intShowConvertPopup=ShowConvertPopup,
@chvConvertPopupText=ConvertPopupText,
@intShopNameID=ShopNameID,
@intAgentNameID=AgentNameID
from Area.Area with (nolock) where PlatformID=@intPlatformID and AreaID=@intAreaID

IF @@ROWCOUNT=0
BEGIN
	set @chvErrMsg = '错误:同步参考的渠道号不存在'
	RETURN 
END



BEGIN TRAN

	--更新其它的渠道基本資料
	update area.Area set
	FAQContent=@chvFAQ,
	PayTips=@chvPayTips,
	UpGradeMsg=@chvUpGradeMsg,
	AnnMsg=@chvAnnMsg,
	AnnMsgShow=@intAnnMsgShow,
	ShowCampaignAtLogin=@intShowCampaignAtLogin,
	ShowFirstDepositAtLogin=@intShowFirstDepositAtLogin,
	DefaultMoney=@intDefaultMoney,
	GiftMoney=@intGiftMoney,
	ShowConvertLimit=@intShowConvertLimit,
	ShowAllGameLimit=@intShowAllGameLimit,
	ShowVIPAgent=@intShowVIPAgent,
	ShowCS=@intShowCS,
	ShowConvertBindPhone =@intShowConvertBindPhone,
	ShowAllGameBindPhone=@intShowAllGameBindPhone,
	ShowVIPAgentBindPhone=@intShowVIPAgentBindPhone, 
	ShowCSBindPhone=@intShowCSBindPhone,
	PlaytimeCS=@intPlaytimeCS,
	PlaytimeConvert=@intPlaytimeConvert,
	PlaytimeVIPAgent=@intPlaytimeVIPAgent,
	ShowConvertPopup=@intShowConvertPopup,
	ConvertPopupText=@chvConvertPopupText,
	ShopNameID=@intShopNameID,
	AgentNameID=@intAgentNameID
	


	where PlatformID=@intPlatformID and AreaID <> @intAreaID

	
	DECLARE @intCurAreaID int
	DECLARE @chvNewAreaConfig varchar(5000)

	DECLARE areaCursor CURSOR FOR

	select AreaID from area.Area with (nolock) where PlatformID=@intPlatformID and AreaID <> @intAreaID

	OPEN areaCursor
	FETCH NEXT FROM areaCursor INTO @intCurAreaID

	WHILE @@FETCH_STATUS = 0
	BEGIN

		--更新areaconfig
		set @chvNewAreaConfig = replace(@chvAreaConfig, '"SiteID":' + convert(varchar,@intAreaId), '"SiteID":' + convert(varchar,@intCurAreaID))

		update area.area set AreaConfig=@chvNewAreaConfig where AreaID=@intCurAreaID

		--刪除渠道的原先遊戲清單
		delete Area.GameList where PlatformID = @intPlatformID and AreaID = @intCurAreaID
		
		--新增渠道遊戲清單
		insert Area.GameList(platformid,areaid,gameid,sort,[status],IconID,Popular)
		select PlatformID, @intCurAreaID as AreaID, GameID, Sort, Status,IconID,Popular from Area.GameList where PlatformID=@intPlatformID and AreaID=@intAreaID


		--删除渠道的原先广告
		delete Area.Advertisement where PlatformID = @intPlatformID and AreaID = @intCurAreaID

		--新增渠道广告清單
		insert Area.Advertisement(platformid,areaid,Picture,Link,Behavior,Status,CreateTime)
		select PlatformID, @intCurAreaID as AreaID,Picture,link,Behavior,Status, getdate() as CreateTime from Area.Advertisement where PlatformID=@intPlatformID and AreaID=@intAreaID


		--删除渠道的原先活動
		delete Area.Campaign where PlatformID = @intPlatformID and AreaID = @intCurAreaID

		--新增渠道活動清單
		insert Area.Campaign([PlatformID],[AreaID],[SeqID],[Picture],[Title],[Tag],[Behavior],[Link],[StartTime],[EndTime],[Status],[CreateTime])
		select PlatformID, @intCurAreaID as AreaID,[SeqID],[Picture],[Title],[Tag],[Behavior],[Link],[StartTime],[EndTime],[Status], getdate() as CreateTime from Area.Campaign where PlatformID=@intPlatformID and AreaID=@intAreaID

	FETCH NEXT FROM areaCursor INTO @intCurAreaID
	END

	CLOSE areaCursor
	DEALLOCATE areaCursor


	

COMMIT TRAN

SET @chvErrMsg = '同步渠道設定成功'
RETURN 1

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SyncAreaConfig] TO [GameUser]
    AS [dbo];

