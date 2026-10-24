-- =============================================
-- Author:  jason
-- Create date: 2019/05/28
-- Description: 活動 新增活動(外層)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Event_EventInfo_Update]	
(
	@intPlatformID	INT,
	@intAreaID		INT,
	@intEventID		INT = NULL,
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


	DECLARE @NowTime DATETIME = GETDATE()
	DECLARE @MinNo INT
	DECLARE @MaxNo INT
	DECLARE @Temp_PlatformID INT
	DECLARE @Temp_AreaID INT
	DECLARE @EventID	INT
	DECLARE @Temp_Platform	TABLE (Num INT,PlatformID INT,AreaID INT)


	IF NOT EXISTS (SELECT TOP 1 1 FROM CenterDB.Area.Area WITH (NOLOCK) WHERE PlatformID = @intPlatformID) AND (@intPlatformID <> 0)
	BEGIN
		SET @chvMsg = '操作錯誤 : 平台不存在'
		RETURN -1
	END

	IF NOT EXISTS (SELECT TOP 1 1 FROM CenterDB.Area.Area WITH (NOLOCK) WHERE AreaID = @intAreaID) AND (@intAreaID <> 0)
	BEGIN
		SET @chvMsg = '操作錯誤 : 渠道不存在'
		RETURN -1
	END

	IF NOT EXISTS (SELECT TOP 1 1 FROM [dbo].[EventInfo] WITH (NOLOCK) WHERE [EventID] = @intEventID) AND (@dtStartTime IS NULL AND @dtEndTime IS NULL)
	BEGIN
		SET @chvMsg = '操作錯誤 : 新增活動日期錯誤'
		RETURN -1
	END


	IF(@intEventID IS NULL)
	BEGIN
		INSERT INTO [dbo].[EventInfo] ([EventName],[CreateTime]) VALUES (@chvEventName,@NowTime)
		SET @EventID = SCOPE_IDENTITY()
	END
	ELSE
	BEGIN
		SET @EventID = @intEventID
	END


	INSERT INTO @Temp_Platform
	SELECT	ROW_NUMBER() OVER (ORDER BY [PlatformID] ASC,[AreaID] ASC) AS Num,
			[PlatformID],
			[AreaID]
	FROM (
			SELECT	DISTINCT PlatformID,
					AreaID
			FROM CenterDB.Area.Area WITH (NOLOCK)
			WHERE
				([PlatformID] = @intPlatformID OR @intPlatformID = 0)
				AND ([AreaID] = @intAreaID OR @intAreaID = 0)
		) AS A

	SELECT	@MinNo = MIN(Num),
			@MaxNo = MAX(Num)
	FROM @Temp_Platform

	WHILE(@MinNo <= @MaxNo)
	BEGIN
		SELECT	@Temp_PlatformID = [PlatformID],
				@Temp_AreaID = [AreaID]
		FROM @Temp_Platform
		WHERE
			Num = @MinNo

		EXEC [dbo].[PrWeb_Event_EventSetting_Update]
			@intPlatformID = @Temp_PlatformID,
			@intAreaID = @Temp_AreaID,
			@intEventID = @EventID,
			@chvEventName = @chvEventName,
			@dtStartTime = @dtStartTime,
			@dtEndTime = @dtEndTime,
			@intStatus = @intStatus,
			@lngLngValue1 = @lngLngValue1,
			@lngLngValue2 = @lngLngValue2,
			@lngLngValue3 = @lngLngValue3,
			@lngLngValue4 = @lngLngValue4,
			@lngLngValue5 = @lngLngValue5,
			@lngLngValue6 = @lngLngValue6,
			@lngLngValue7 = @lngLngValue7,
			@lngLngValue8 = @lngLngValue8,
			@lngLngValue9 = @lngLngValue9,
			@lngLngValue10 = @lngLngValue10,
			@chvMsg = @chvMsg OUTPUT

		SET @MinNo = @MinNo + 1
	END


	SET @chvMsg = '操作完成'
	RETURN 1
END
