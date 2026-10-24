-- =============================================
-- Author:		Jason
-- Create date: 2018/05/24
-- Description:	活躍用戶統計
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_Stat_DAULog]
(
	@dtStatTime DATETIME = NULL
)
AS
BEGIN
	SET NOCOUNT ON

	SET @dtStatTime = ISNULL(@dtStatTime,GETDATE())

	DECLARE @LogStartTime			DATETIME
	DECLARE @LogEndTime				DATETIME

	DECLARE @Temp_UserDAULog	TABLE (StatTime DATETIME,[PlatformID] INT,[AreaID] INT,GameID INT,[PCCount] INT,[AndroidCount] INT,[IOSCount] INT,[TotalCount] INT,TotalTimes INT)
	
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
	INSERT INTO @Temp_UserDAULog
	SELECT	StatTime = @LogStartTime,
			PlatformID,
			AreaID,
			GameID,
			PCCounts = ISNULL([1],0),
			AndRoidCounts = ISNULL([2],0),
			iOSCounts = ISNULL([3],0),
			TotalCounts = ISNULL([1],0) + ISNULL([2],0) + ISNULL([3],0),
			TotalTimes
	FROM (
			SELECT	A.PlatformID,
					A.AreaID,
					GameID = 0,
					A.HDType,
					TotalCounts = COUNT(DISTINCT A.UserID),
					TotalTimes
			FROM [Users].[UserLogonLog] AS A WITH (NOLOCK)
				LEFT JOIN (
							SELECT PlatformID,AreaID,COUNT(UserID) AS TotalTimes
							FROM [Users].[UserLogonLog] WITH (NOLOCK)
							WHERE
								CreateTime >= @LogStartTime
								AND CreateTime < @LogEndTime
							GROUP BY PlatformID,AreaID
						) AS B ON A.PlatFormID = B.PlatFormID AND A.AreaID = B.AreaID
			WHERE
				A.PlatformID IS NOT NULL
				AND A.CreateTime >= @LogStartTime
				AND A.CreateTime < @LogEndTime
			GROUP BY A.PlatformID,A.AreaID,HDType,TotalTimes
			UNION ALL
			SELECT	A.PlatformID,
					A.AreaID,
					A.GameID,
					A.HDType,
					TotalCounts = COUNT(DISTINCT A.UserID),
					TotalTimes
			FROM [Game].[GameLog] AS A WITH (NOLOCK)
				INNER JOIN [Users].[UserRegLog] AS B ON A.UserID = B.UserID
				LEFT JOIN (
							SELECT PlatformID,AreaID,GameID,COUNT(UserID) AS TotalTimes
							FROM [Game].[GameLog] WITH (NOLOCK)
							WHERE
								[LogoutTime] >= @LogStartTime
								AND [LogoutTime] < @LogEndTime
							GROUP BY PlatformID,AreaID,GameID
						) AS C ON A.PlatformID = C.PlatformID AND A.AreaID = C.AreaID AND A.GameID = C.GameID
			WHERE
				A.PlatformID IS NOT NULL
				AND [LogoutTime] >= @LogStartTime
				AND [LogoutTime] < @LogEndTime
			GROUP BY A.PlatformID,A.AreaID,A.GameID,A.HDType,TotalTimes
			UNION ALL
			SELECT	PlatformID = 0,
					AreaID = 0,
					GameID = 0,
					A.HDType,
					TotalCounts = COUNT(DISTINCT A.UserID),
					TotalTimes
			FROM [Users].[UserLogonLog] AS A WITH (NOLOCK)
				LEFT JOIN (
							SELECT COUNT(UserID) AS TotalTimes
							FROM [Users].[UserLogonLog] WITH (NOLOCK)
							WHERE
								CreateTime >= @LogStartTime
								AND CreateTime < @LogEndTime
						) AS B ON 1 = 1
			WHERE
				A.PlatformID IS NOT NULL
				AND A.CreateTime >= @LogStartTime
				AND A.CreateTime < @LogEndTime
			GROUP BY HDType,TotalTimes
		) AS A
	PIVOT
		(
			MAX(TotalCounts)
			FOR HDType IN ([1],[2],[3])
		) AS PVT
	------------------------------------------------------


	/*  若已有統計數據則刪除後再重新寫入  */
	------------------------------------------------------
	MERGE [Stat].[StatisticsDAU] AS A
	USING (
			SELECT	StatTime,[PlatformID],[AreaID],GameID,[PCCount],[AndroidCount],[IOSCount],[TotalCount],[TotalTimes]
			FROM @Temp_UserDAULog
			) AS B ON (A.[StatTime] = B.StatTime AND A.[PlatformID] = B.[PlatformID] AND A.[AreaID] = B.[AreaID] AND A.GameID = B.GameID)
	WHEN MATCHED THEN
		UPDATE SET	A.[StatTime] = B.[StatTime],
					A.[PlatformID] = B.[PlatformID],
					A.[AreaID] = B.[AreaID],
					A.GameID = B.GameID,
					A.[PCCount] = ISNULL(B.[PCCount],0),
					A.AndRoidCount = ISNULL(B.AndRoidCount,0),
					A.iOSCount = ISNULL(B.iOSCount,0),
					A.TotalCount = ISNULL(B.TotalCount,0),
					A.[TotalTimes] = ISNULL(B.[TotalTimes],0)
	WHEN NOT MATCHED THEN
		INSERT
			(StatTime,[PlatformID],[AreaID],GameID,[PCCount],[AndroidCount],[IOSCount],[TotalCount],[TotalTimes])
		VALUES
			(
				StatTime,
				[PlatformID],
				[AreaID],
				GameID,
				ISNULL([PCCount],0),
				ISNULL([AndroidCount],0),
				ISNULL([IOSCount],0),
				ISNULL([TotalCount],0),
				ISNULL(B.[TotalTimes],0)
			);
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_Stat_DAULog] TO [xbuser]
    AS [dbo];

