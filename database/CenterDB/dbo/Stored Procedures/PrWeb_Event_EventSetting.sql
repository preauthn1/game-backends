-- =============================================
-- Author:  Jason
-- Create date: 2018/07/31
-- Description:	活動設定更新
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Event_EventSetting]
(
	@sintEventID	smallint = null,
	@chvEventName	varchar(32) = null,
	@dtStatTime		datetime = null,
	@dtEndTime		datetime = null,
	@tnyStatus		int = null,
	@lngGiftCash	bigint = null,
	@updater		varchar(20),
	@chvRtnMsg		varchar(200) = ''	OUTPUT,
	@intRtnCode		INT = 0	OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	/*  置換  */
	------------------------------------------------------------------------------------
	set @dtStatTime		= ISNULL(@dtStatTime,DATEADD(DAY,0,DATEDIFF(DAY,0,GETDATE())))
	set @dtEndTime		= ISNULL(@dtEndTime,DATEADD(DAY,0,DATEDIFF(DAY,0,'2100-01-01')))
	set @tnyStatus		= ISNULL(@tnyStatus,1)
	set @lngGiftCash	= ISNULL(@lngGiftCash,0)

	declare @nowtime datetime = getdate()
	------------------------------------------------------------------------------------


	/*  確認活動是否存在  */
	------------------------------------------------------------------------------------
	IF NOT EXISTS (select top 1 1 from [Users].[EventSetting] WITH (NOLOCK) WHERE @sintEventID = [EventID])
		AND EXISTS (SELECT TOP 1 1 FROM [Users].[EventSetting] WITH (NOLOCK) WHERE [EventName] = @chvEventName)
	BEGIN
		SET @chvRtnMsg	= '活动名称已存在'
		SET @intRtnCode		= -1
		RETURN
	END

	IF (@sintEventID is not null) and EXISTS (SELECT TOP 1 1 FROM [Users].[EventSetting] WITH (NOLOCK) WHERE [EventName] = @chvEventName AND @sintEventID <> [EventID])
	BEGIN
		SET @chvRtnMsg	= '不能更名为已被使用的活动名称'
		SET @intRtnCode		= -2
		RETURN
	END

	if (@dtStatTime >= @dtEndTime)
	begin
		SET @chvRtnMsg	= '活动日期错误'
		SET @intRtnCode		= -3
		RETURN
	end
	------------------------------------------------------------------------------------

	
	/*  更新用戶資料  */
	------------------------------------------------------------------------------------
	if not exists (select top 1 1 from [Users].[EventSetting] with (nolock) where eventid = @sintEventID)
	begin
		insert into [Users].[EventSetting]
			([EventID],[EventName],[StartTime],[EndTime],[Status],[GiftCash],[Updater],[UpdateTime])
		values
			(@sintEventID,@chvEventName,@dtStatTime,@dtEndTime,@tnyStatus,@lngGiftCash,@updater,@nowtime)

		if @@rowcount = 1
		begin
			set @chvRtnMsg	= '新增完成'
		end
		else
		begin
			set @chvRtnMsg	= '新增失败'
			set @intRtnCode		= -4
			return
		end
	end
	else if (@sintEventID is not null and @tnyStatus = -1)
	begin
		update [Users].[EventSetting]
		set status = @tnyStatus,
			[Updater] = @updater,
			[UpdateTime] = @nowtime
		where
			eventid = @sintEventID

		if @@rowcount = 1
		begin
			set @chvRtnMsg	= '删除完成'
		end
		else
		begin
			set @chvRtnMsg	= '删除失败'
			set @intRtnCode		= -5
			return
		end
	end
	else
	begin
		update [Users].[EventSetting]
		set [EventName] = case when [EventName] <> @chvEventName then @chvEventName else [EventName] end,
			[StartTime] = case when [StartTime] <> @dtStatTime then @dtStatTime else [StartTime] end,
			[EndTime] = case when [EndTime] <> @dtEndTime then @dtEndTime else [EndTime] end,
			[Status] = case when [Status] <> @tnyStatus then @tnyStatus else [Status] end,
			[GiftCash] = case when [GiftCash] <> @lngGiftCash then @lngGiftCash else [GiftCash] end,
			[Updater] = @updater,
			[UpdateTime] = @nowtime
		where
			eventid = @sintEventID

		if @@rowcount = 1
		begin
			set @chvRtnMsg	= '更新完成'
		end
		else
		begin
			set @chvRtnMsg	= '更新失败'
			set @intRtnCode		= -6
			return
		end
	end


	set @intRtnCode		= 1
	return
	------------------------------------------------------------------------------------


END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_EventSetting] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_EventSetting] TO [xbuser]
    AS [dbo];

