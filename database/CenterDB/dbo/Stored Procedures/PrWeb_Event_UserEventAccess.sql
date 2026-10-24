-- =============================================
-- Author:  Jason
-- Create date: 2018/07/31
-- Description:	活動資格更新
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Event_UserEventAccess]
(
	@sintEventID	smallint = null,
	@intUserID		int = null,
	@updater		varchar(20),
	@tnyLoginTimes	tinyint = null,
	@chvRtnMsg		varchar(200) = '' OUTPUT,
	@intRtnCode		INT = 0	OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON


	/*  確認活動  */
	------------------------------------------------------------------------------------
	IF NOT EXISTS (SELECT TOP 1 1 FROM [Users].[EventSetting] WITH (NOLOCK) WHERE @sintEventID = [EventID])
	BEGIN
		SET @chvRtnMsg	= '修改失败,活动不存在'
		SET @intRtnCode		= -1
		RETURN
	END

	IF NOT EXISTS (SELECT TOP 1 1 FROM [Users].[EventLog] WITH (NOLOCK) WHERE @sintEventID = [EventID] AND UserID = @intUserID)
	BEGIN
		SET @chvRtnMsg	= '修改失败,使用者尚未参加活动'
		SET @intRtnCode		= -2
		RETURN
	END

	IF NOT EXISTS (SELECT TOP 1 1 FROM [Users].[EventSetting] WITH (NOLOCK) WHERE @sintEventID = [EventID] AND Status = 1)
	BEGIN
		SET @chvRtnMsg	= '修改失败,活动不存在'
		SET @intRtnCode		= -3
		RETURN
	END

	IF NOT EXISTS (SELECT TOP 1 1 FROM [Users].[EventSetting] WITH (NOLOCK) WHERE @sintEventID = [EventID] AND GETDATE() between [StartTime] and [EndTime])
	BEGIN
		SET @chvRtnMsg	= '修改失败,活动已过期'
		SET @intRtnCode		= -4
		RETURN
	END

	IF (@tnyLoginTimes is null)
	BEGIN
		SET @chvRtnMsg	= '修改失败,需指定调整登入次数'
		SET @intRtnCode		= -5
		RETURN
	END
	------------------------------------------------------------------------------------

	
	/*  更新用戶資料  */
	------------------------------------------------------------------------------------
	update [Users].[EventLog]
	set	[GrantTimes] = @tnyLoginTimes,
		[Updater] = @updater
		--,[UpdateTime] = dateadd(day,-1,[UpdateTime])
	where
		@sintEventID = [EventID]
		AND UserID = @intUserID

	if @@rowcount <> 1
	begin
		set @intRtnCode		= 1
		set @chvRtnMsg		= '更新失敗'
		return
	end

	set @intRtnCode		= 1
	set @chvRtnMsg		= '更新完成'
	return
	------------------------------------------------------------------------------------


END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_UserEventAccess] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_UserEventAccess] TO [xbuser]
    AS [dbo];

