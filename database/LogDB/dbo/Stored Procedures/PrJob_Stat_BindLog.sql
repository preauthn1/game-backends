-- =============================================
-- Author:		Jason
-- Create date: 2018/05/28
-- Description:	統計用戶綁定數 #349
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_Stat_BindLog]
(
	@dtStatTime DATETIME = NULL
)
AS
BEGIN
	SET NOCOUNT ON

	SET @dtStatTime = ISNULL(@dtStatTime,GETDATE())

	DECLARE @LogStartTime			DATETIME
	DECLARE @LogEndTime				DATETIME

	DECLARE @Temp_UserBind	TABLE ([StatTime] DATETIME,[PlatformID] INT,[AreaID] INT,[TotalBindCount] INT,[SameDayReg] INT,[IOSCount] INT,[AndroidCount] INT,[PCCount] INT)


	IF (DATEPART(HOUR,@dtStatTime) = 0) AND (DATEPART(MINUTE,@dtStatTime) < 10)
	BEGIN
		SET @LogStartTime		= DATEADD(DAY,-1,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogEndTime			= DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
	END
	ELSE
	BEGIN
		SET @LogStartTime		= DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogEndTime			= DATEADD(DAY,1,DATEDIFF(DAY,0,@dtStatTime))
	END


	/*  取登入紀錄  */
	------------------------------------------------------
	INSERT INTO @Temp_UserBind
	SELECT	DISTINCT *
	FROM (
			SELECT	[StatTime] = CONVERT(VARCHAR(10),A.[CreateTime],23),
					[PlatformID] = ISNULL(A.[PlatformID],0),
					[AreaID] = ISNULL(A.[AreaID],0),
					[TotalBindCount] = COUNT(1),
					[SameDayReg] = SUM(CASE WHEN  Convert(date,A.CreateTime) <> Convert(date,B.CreateTime) THEN 0 ELSE 1 END),
					[IOSCount] = SUM(CASE WHEN A.[HDType] = 3 THEN 1 ELSE 0 END),
					[AndroidCount] = SUM(CASE WHEN A.[HDType] = 2 THEN 1 ELSE 0 END),
					[PCCount] = SUM(CASE WHEN A.[HDType] = 1 THEN 1 ELSE 0 END)			
			FROM [Users].[UserBindLog] AS A WITH (NOLOCK)
				LEFT JOIN [Users].[UserRegLog] AS B ON A.USerID = B.UserID
			WHERE
				A.[CreateTime] >= @LogStartTime
				AND A.[CreateTime] < @LogEndTime
			GROUP BY ROLLUP(CONVERT(VARCHAR(10),A.[CreateTime],23),A.[PlatformID],A.[AreaID])
		) AS A
	WHERE
		[StatTime] IS NOT NULL
	------------------------------------------------------


	/*  若已有統計數據則刪除後再重新寫入  */
	------------------------------------------------------
	MERGE [Stat].[StatisticsUserBind] AS A
	USING (
			SELECT	[StatTime],[PlatformID],[AreaID],[TotalBindCount],[SameDayReg],[IOSCount],[AndroidCount],[PCCount]
			FROM @Temp_UserBind
			) AS B ON (A.[StatTime] = B.[StatTime] AND A.[PlatformID] = B.[PlatformID] AND A.[AreaID] = B.[AreaID])
	WHEN MATCHED THEN
		UPDATE SET	A.[TotalBindCount] = ISNULL(B.[TotalBindCount],0),
					A.[SameDayReg] = ISNULL(B.[SameDayReg],0),
					A.[IOSCount] = ISNULL(B.[IOSCount],0),
					A.[AndroidCount] = ISNULL(B.[AndroidCount],0),
					A.[PCCount] = ISNULL(B.[PCCount],0)
	WHEN NOT MATCHED THEN
		INSERT
			([StatTime],[PlatformID],[AreaID],[TotalBindCount],[SameDayReg],[IOSCount],[AndroidCount],[PCCount])
		VALUES
			(
				[StatTime],
				[PlatformID],
				[AreaID],
				ISNULL([TotalBindCount],0),
				ISNULL([SameDayReg],0),
				ISNULL([IOSCount],0),
				ISNULL([AndroidCount],0),
				ISNULL([PCCount],0)
			);
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_Stat_BindLog] TO [xbuser]
    AS [dbo];

