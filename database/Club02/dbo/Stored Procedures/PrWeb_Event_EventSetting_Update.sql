-- =============================================
-- Author:  jason
-- Create date: 2019/05/22
-- Description: 返利活動 新增活動(底層)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Event_EventSetting_Update]	
(
	@intPlatformID	INT,
	@intAreaID		INT,
	@intEventID		INT,
	@chvEventName	VARCHAR(50) = NULL,
	@dtStartTime	DATETIME = NULL,
	@dtEndTime		DATETIME = NULL,
	@intStatus		INT = NULL,
	@lngLngValue1	BIGINT = NULL,
	@lngLngValue2	BIGINT = NULL,
	@lngLngValue3	BIGINT = NULL,
	@lngLngValue4	BIGINT = NULL,
	@lngLngValue5	BIGINT = NULL,
	@lngLngValue6	BIGINT = NULL,
	@lngLngValue7	BIGINT = NULL,
	@lngLngValue8	BIGINT = NULL,
	@lngLngValue9	BIGINT = NULL,
	@lngLngValue10	BIGINT = NULL,
	@chvMsg		VARCHAR(100) OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON


	DECLARE @NowTime	DATETIME = GETDATE()
	DECLARE @OldEventNAme	VARCHAR(50)
	DECLARE @OldStartTime	DATETIME
	DECLARE @OldEndTime		DATETIME
	DECLARE @OldStatus		INT
	DECLARE @Status			INT = 1	--新增活動時預設值
	DECLARE @StartDate		DATETIME	--新增活動時預設值
	DECLARE @EndDate		DATETIME	--新增活動時預設值

	DECLARE @EventID	INT



	SET @StartDate = ISNULL(@dtStartTime,'1970-01-01')
	SET @EndDate = ISNULL(@dtEndTime,'1971-01-01')
	SET @lngLngValue1 = ISNULL(@lngLngValue1,0)
	SET @lngLngValue2 = ISNULL(@lngLngValue2,0)
	SET @lngLngValue3 = ISNULL(@lngLngValue3,0)
	SET @lngLngValue4 = ISNULL(@lngLngValue4,0)
	SET @lngLngValue5 = ISNULL(@lngLngValue5,0)
	SET @lngLngValue6 = ISNULL(@lngLngValue6,0)
	SET @lngLngValue7 = ISNULL(@lngLngValue7,0)
	SET @lngLngValue8 = ISNULL(@lngLngValue8,0)
	SET @lngLngValue9 = ISNULL(@lngLngValue9,0)
	SET @lngLngValue10 = ISNULL(@lngLngValue10,0)


	IF NOT EXISTS (SELECT TOP 1 1 FROM CenterDB.Area.Area WITH (NOLOCK) WHERE PlatformID = @intPlatformID)
	BEGIN
		SET @chvMsg = '操作錯誤 : 平台不存在'
		RETURN -1
	END

	IF NOT EXISTS (SELECT TOP 1 1 FROM CenterDB.Area.Area WITH (NOLOCK) WHERE AreaID = @intAreaID)
	BEGIN
		SET @chvMsg = '操作錯誤 : 渠道不存在'
		RETURN -1
	END


	IF NOT EXISTS (SELECT TOP 1 1 FROM [dbo].[EventPlatformInfo] WITH (NOLOCK) WHERE [EventID] = @intEventID AND [PlatformID] = @intPlatformID AND [AreaID] = @intAreaID)
	BEGIN
		INSERT INTO [dbo].[EventPlatformInfo]
			([PlatformID],[AreaID],[EventID],[EventStartDate],[EventEndDate],[Status])
		VALUES
			(@intPlatformID,@intAreaID,@intEventID,@StartDate,@EndDate,@Status)
	END
	
	IF NOT EXISTS (SELECT TOP 1 1 FROM [dbo].[EventContent] WITH (NOLOCK) WHERE [EventID] = @intEventID AND [PlatformID] = @intPlatformID AND [AreaID] = @intAreaID)
	BEGIN
		INSERT INTO [dbo].[EventContent]
			([PlatformID],[AreaID],[EventID],[LngValue1],[LngValue2],[LngValue3],[LngValue4],[LngValue5],[LngValue6],[LngValue7],[LngValue8],[LngValue9],[LngValue10])
		VALUES
			(@intPlatformID,@intAreaID,@intEventID,@lngLngValue1,@lngLngValue2,@lngLngValue3,@lngLngValue4,@lngLngValue5,@lngLngValue6,@lngLngValue7,@lngLngValue8,@lngLngValue9,@lngLngValue10)
	END

	
	SELECT	@OldEventNAme = [EventName]
	FROM [dbo].[EventInfo] WITH (NOLOCK)
	WHERE
		[EventID] = @intEventID

	SELECT	@OldStartTime = [EventStartDate],
			@OldEndTime = [EventEndDate],
			@OldStatus = [Status]
	FROM [dbo].[EventPlatformInfo] WITH (NOLOCK)
	WHERE
		[EventID] = @intEventID
		AND [PlatformID] = @intPlatformID
		AND [AreaID] = @intAreaID


	IF (@OldEventNAme <> @chvEventName) AND (@chvEventName IS NOT NULL)
	BEGIN
		UPDATE [dbo].[EventInfo]
		SET [EventName] = @chvEventName
		WHERE
			[EventID] = @intEventID
	END

	IF(@dtStartTime IS NOT NULL AND @dtEndTime IS NOT NULL)
	BEGIN
		UPDATE [dbo].[EventPlatformInfo]
		SET [EventStartDate] = @dtStartTime,
			[EventEndDate] = @dtEndTime
		WHERE
			[EventID] = @intEventID
			AND [PlatformID] = @intPlatformID
			AND [AreaID] = @intAreaID
	END

	IF (@intStatus IS NOT NULL)
	BEGIN
		UPDATE [dbo].[EventPlatformInfo]
		SET [Status] = @intStatus
		WHERE
			[EventID] = @intEventID
			AND [PlatformID] = @intPlatformID
			AND [AreaID] = @intAreaID
	END

	UPDATE [dbo].[EventContent]
	SET [LngValue1] = CASE	WHEN @lngLngValue1 IS NOT NULL AND [LngValue1] <> @lngLngValue1 THEN @lngLngValue1 ELSE [LngValue1] END,
		[LngValue2] = CASE	WHEN @lngLngValue2 IS NOT NULL AND [LngValue2] <> @lngLngValue2 THEN @lngLngValue2 ELSE [LngValue2] END,
		[LngValue3] = CASE	WHEN @lngLngValue3 IS NOT NULL AND [LngValue3] <> @lngLngValue3 THEN @lngLngValue3 ELSE [LngValue3] END,
		[LngValue4] = CASE	WHEN @lngLngValue4 IS NOT NULL AND [LngValue4] <> @lngLngValue4 THEN @lngLngValue4 ELSE [LngValue4] END,
		[LngValue5] = CASE	WHEN @lngLngValue5 IS NOT NULL AND [LngValue5] <> @lngLngValue5 THEN @lngLngValue5 ELSE [LngValue5] END,
		[LngValue6] = CASE	WHEN @lngLngValue6 IS NOT NULL AND [LngValue6] <> @lngLngValue6 THEN @lngLngValue6 ELSE [LngValue6] END,
		[LngValue7] = CASE	WHEN @lngLngValue7 IS NOT NULL AND [LngValue7] <> @lngLngValue7 THEN @lngLngValue7 ELSE [LngValue7] END,
		[LngValue8] = CASE	WHEN @lngLngValue8 IS NOT NULL AND [LngValue8] <> @lngLngValue8 THEN @lngLngValue8 ELSE [LngValue8] END,
		[LngValue9] = CASE	WHEN @lngLngValue9 IS NOT NULL AND [LngValue9] <> @lngLngValue9 THEN @lngLngValue9 ELSE [LngValue9] END,
		[LngValue10] = CASE	WHEN @lngLngValue10 IS NOT NULL AND [LngValue10] <> @lngLngValue10 THEN @lngLngValue10 ELSE [LngValue10] END
	WHERE
		[EventID] = @intEventID
		AND [PlatformID] = @intPlatformID
		AND [AreaID] = @intAreaID


	SET @chvMsg = '操作完成'
	RETURN 1
END
