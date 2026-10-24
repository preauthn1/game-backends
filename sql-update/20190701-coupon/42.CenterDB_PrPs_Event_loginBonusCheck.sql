USE [CenterDB]
GO

/****** Object:  StoredProcedure [dbo].[PrPs_Event_LoginBonusCheck]    Script Date: 2019/7/3 下午 06:17:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:  Jason
-- Create date: 2018/08/01
-- Description:	GS活動資料查詢

-- 2019/07/02	Eddie	增加到Event.7DaysLoginBonus這個Table取得該玩家對應之平台渠道的7天獎勵金額資料
-- =============================================
ALTER PROCEDURE [dbo].[PrPs_Event_LoginBonusCheck]
(
	@intUserID	INT,
	@LoginTimes	INT = 0				OUTPUT,
	@GiftCash		BIGINT = 0		OUTPUT,
	@chvRtnMsg	VARCHAR(100) = ''	OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	declare @nowtime datetime = getdate()
	declare @chargeamount bigint
	declare @binding varchar(32)
	declare @intAreaID int
	declare @intPlatformID	int

	select @chargeamount = (sumcharge+SumAgentCharge), @binding = [LoginName],@intAreaID = [AreaID] from [Users].[UserInfo] with (nolock) where userid = @intUserID
	select @intPlatformID = PlatformID from [Area].[Area] with (nolock) where [AreaID] = @intAreaID
	select @LoginTimes = Sum([GrantTimes]) from [Users].[EventLog] with (nolock) where eventid = 1 and [LoginName] = @binding
	select @giftcash = [GiftCash] from [Users].[EventSetting] with (nolock) where eventid = 1

	set @LoginTimes = isnull(@LoginTimes,0)
	set @chargeamount = isnull(@chargeamount,0)

	--從7DaysLoginBonus取得獎勵金額
	Declare @findPlatformID	int = @intPlatformID
	Declare @findAreaID	int = @intAreaID

	if not exists (select areaid From [Event].[7DaysLoginBonus] with (nolock) where areaid=@findAreaID)
	Begin 
		set @findAreaID = 0
		if not exists (select platformID From [Event].[7DaysLoginBonus] with (nolock) where platformID=@findPlatformID)
		Begin
			set @findPlatformID = 0
		End
	End	

	select Day1, Day2, Day3, Day4, Day5, Day6, Day7
	From [Event].[7DaysLoginBonus] with (nolock) 
	where platformid = @findPlatformID and areaid = @findAreaID and [Status]=1


	if not exists (select top 1 1 from [Users].[EventSetting] with (nolock) where [Status] = 1 and starttime <= @nowtime and endtime > @nowtime and eventid = 1)
	begin
		set @chvRtnMsg = '活动不存在'
		return -1
	end

	if (@LoginTimes >= 7)
	begin
		set @chvRtnMsg = '已领超过七次限制'
		return -2
	end

	if (@chargeamount < @giftcash)
	begin
		set @chvRtnMsg = '充值金额不足'
		return -3
	end

	if (@binding LIKE 'Fast%')
	begin
		set @chvRtnMsg = '账号未绑定'
		return -4
	end

	if exists (select top 1 1 from [Users].[EventLog] with (nolock) where eventid = 1 and [LoginName] = @binding and datediff(day,[UpdateTime],@nowtime) = 0)
	begin
		set @chvRtnMsg = '今日已领过奖励'
		return 0
	end

	if not exists (select top 1 1 from [Users].[EventLog] with (nolock) where eventid = 1 and [LoginName] = @binding and datediff(day,[UpdateTime],@nowtime) = 0)
	begin
		set @chvRtnMsg = '今日尚未领过奖励'
		return 1
	end
	

END
GO


