-- =============================================
-- Author:		Jason
-- Create date: 2019/03/27
-- Description:	統計BankLog 銀行金幣統計改為LogDB計算及同步更新CenterDB
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_Stat_BankSourceLog]
(
	@dtStatTime DATETIME = NULL
)
AS
BEGIN
	SET NOCOUNT ON
	
	SET @dtStatTime = ISNULL(@dtStatTime,GETDATE())

	DECLARE @LogStartTime			DATETIME = DATEADD(DAY,-1,DATEADD(DAY,0,@dtStatTime))
	DECLARE @LogEndTime				DATETIME = DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))

	IF (DATEPART(HOUR,@dtStatTime) = 0) AND (DATEPART(MINUTE,@dtStatTime) < 10)
	BEGIN
		SET @LogStartTime		= DATEADD(DAY,-30,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogEndTime			= DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
	END
	ELSE
	BEGIN
		SET @LogStartTime		= DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogEndTime			= DATEADD(DAY,1,DATEDIFF(DAY,0,@dtStatTime))
	END


	/*  取登入有金幣異動紀錄  */
	------------------------------------------------------
	--新對應 20190327 增加PlatformID , AreaID對應
	DECLARE @Temp_BankSourceLog_New	TABLE ([PlatformID] INT,[AreaID] INT,[SourceTypeID] INT,[Amount] BIGINT,[StatTime] DATETIME)

	INSERT INTO @Temp_BankSourceLog_New
	SELECT	A.[PlatformID],
			A.[AreaID],
			[SourceTypeID],
			[Amount] = SUM(CASE WHEN B.UserID IS NOT NULL THEN [UpdateAmount] ELSE 0 END),
			[StatTime] = DATEADD(DAY,0,DATEDIFF(DAY,0,A.[CreateTime]))
	FROM [Money].[BankLog]	AS A WITH (NOLOCK)
		LEFT JOIN [Users].[UserRegLog] AS B WITH (NOLOCK) ON A.UserID = B.UserID
	WHERE
		A.[CreateTime] >= @LogStartTime
		AND A.[CreateTime] < @LogEndTime
		AND [SourceTypeID] IN (12000,12001,13000,13001,13002,13003)
	GROUP BY A.[PlatformID],A.[AreaID],[SourceTypeID],DATEADD(DAY,0,DATEDIFF(DAY,0,A.[CreateTime]))
	------------------------------------------------------
	


	/*  若已有統計數據則刪除後再重新寫入  */
	------------------------------------------------------
	--新對應 20190327 增加PlatformID , AreaID對應
	MERGE [Stat].[StatisticsBankSourceLog_New] AS A
	USING (
			SELECT	[PlatformID],	[AreaID],	[SourceTypeID],	[Amount],	[StatTime]
			FROM @Temp_BankSourceLog_New
		) AS B ON A.[PlatformID] = B.[PlatformID] AND A.[AreaID] = B.[AreaID] AND A.[SourceTypeID] = B.[SourceTypeID] AND A.[StatTime] = B.[StatTime]
	WHEN MATCHED THEN
		UPDATE SET	A.[Amount] = B.[Amount]
	WHEN NOT MATCHED THEN
		INSERT
			(	[PlatformID],	[AreaID],	[SourceTypeID],		[StatTime],		[Amount]	)
		VALUES (B.[PlatformID],	B.[AreaID],	B.[SourceTypeID],	B.[StatTime],	B.[Amount]	);
	------------------------------------------------------

	RETURN
END