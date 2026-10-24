USE [CenterDB]
GO

/****** Object:  StoredProcedure [dbo].[PrPs_Event_LoginBonus]    Script Date: 2019/7/3 下午 06:17:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:  Jason
-- Create date: 2018/08/01
-- Description:	活動資料回傳&贈送金幣

-- 2019/07/02	Eddie	增加到Event.7DaysLoginBonus這個Table取得該玩家對應之平台渠道的7天獎勵金額資料
-- =============================================
ALTER PROCEDURE [dbo].[PrPs_Event_LoginBonus]
(
	@intUserID			INT,
	@chvIPAddress		VARCHAR(40),
	@LoginTimes			INT = 0				OUTPUT,
	@GiftCash		BIGINT = 0			OUTPUT,
	@lngCurrentAmount	BIGINT = 0			OUTPUT,
	@chvRtnMsg			VARCHAR(100) = ''	OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	declare @nowtime datetime = getdate()
	declare @sintSourceType int = 21001
	declare @chvMemo varchar(20) = '登入奖励送金币'
	declare @bonusamount bigint = 0  --預設值改為0

	set @LoginTimes = 0
	set @chvRtnMsg = ''

	--select @LoginTimes = [GrantTimes] from [Users].[EventLog] with (nolock) where eventid = 1 and userid = @intUserID

	if not exists (select top 1 1 from [Users].[EventSetting] with (nolock) where [Status] = 1 and starttime <= @nowtime and endtime > @nowtime and eventid = 1)
	begin
		set @chvRtnMsg = '活动不存在'
		return -1
	end

	--declare @giftcash bigint
	declare @intPlatformID int
	declare @chargeamount bigint
	declare @binding varchar(32)
	declare @intAreaid int

	select @chargeamount = (sumcharge+SumAgentCharge), @binding = [LoginName],@intAreaid = [AreaID] from [Users].[UserInfo] with (nolock) where userid = @intUserID
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

	Declare @intDay1 bigint, @intDay2 bigint, @intDay3 bigint, @intDay4 bigint, @intDay5 bigint, @intDay6 bigint, @intDay7 bigint

	select @intDay1=Day1, @intDay2=Day2, @intDay3=Day3, @intDay4=Day4, @intDay5=Day5, @intDay6=Day6, @intDay7=Day7
	From [Event].[7DaysLoginBonus] with (nolock) 
	where platformid = @findPlatformID and areaid = @findAreaID and [Status]=1

	if exists (select top 1 1 from [Users].[EventLog] with (nolock) where eventid = 1 and [LoginName] = @binding and datediff(day,[UpdateTime],@nowtime) = 0)
	begin
		set @chvRtnMsg = '今日已领过奖励'
		return 0
	end

	if (@LoginTimes > 7)
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

	--select @giftcash = [GiftCash] from [Users].[EventSetting] with (nolock) where [EventID] = 1

	if exists (select top 1 1 from [Users].[EventLog] with (nolock) where eventid = 1 and [LoginName] = @binding and userid = @intUserID)
	begin
		update [Users].[EventLog]
		set [GrantTimes] = [GrantTimes] + 1,
			[UpdateTime] = @nowtime,
			[LoginName] = @binding
		where
			eventid = 1
			and [LoginName] = @binding and UserID=@intUserID
	end
	else
	begin
		insert into [Users].[EventLog]
			([EventID],[UserID],[UpdateTime],[GrantTimes],[LoginName])
		values
			(1,@intUserID,@nowtime,1,@binding)
	end

	if (@@rowcount <> 1)
	begin
		set @chvRtnMsg = '新增活动资格失败'
		return -5
	end

	Select @bonusamount = (Case
		when @LoginTimes=0 Then @intDay1
		when @LoginTimes=1 Then @intDay2
		when @LoginTimes=2 Then @intDay3
		when @LoginTimes=3 Then @intDay4
		when @LoginTimes=4 Then @intDay5
		when @LoginTimes=5 Then @intDay6
		when @LoginTimes=6 Then @intDay7
		End)


	insert into [Users].[EventDetailLog]
		([EventID],[UserID],[CreateTime],[GiftCash],[IPAddress],[LoginName] )
	values
		(1,@intUserID,@nowtime,@bonusamount,@chvIPAddress,@binding)

	if (@@rowcount <> 1)
	begin
		set @chvRtnMsg = '新增活动纪录失败'
		return -6
	end

	exec [dbo].[PrPsWeb_UpdateUserCash]
		@intUserID			= @intUserID,		--用户ID
		@sintSourceType		= @sintSourceType,	--来源
		@lngUpdateAmount	= @bonusamount,		--更新数量
		@chvIPAddress		= @chvIPAddress,	--IP
		@chvMemo			= @chvMemo,			--说明
		@lngCurrentAmount	= @lngCurrentAmount			OUTPUT, --当前金币数
		@lngTaxAmount		= 0,				--税收
		@intAreaID			= @intAreaID,		--渠道
		@intPlatformID		= @intPlatformID	--平台

	select @LoginTimes = Sum([GrantTimes]) from [Users].[EventLog] with (nolock) where eventid = 1 and [LoginName] = @binding
	set @lngCurrentAmount = @lngCurrentAmount + @bonusamount

	set @LoginTimes = isnull(@LoginTimes,0)

	set @chvRtnMsg = '发送完成'
	return 1
	------------------------------------------------------------------------------------

END
GO

