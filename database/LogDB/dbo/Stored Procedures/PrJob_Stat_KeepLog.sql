-- =============================================
-- Author:		Jason
-- Create date: 2018/03/27
-- Description:	統計留存率 (機制改寫)
--	2018/05/23	Jason		#349 建立統計數據報表提供API回傳 新增3 4 5 6 8 9 10日統計
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_Stat_KeepLog]
(
	@dtStatTime DATETIME = NULL
)
AS
BEGIN
	SET NOCOUNT ON
	
	SET @dtStatTime = ISNULL(@dtStatTime,DATEADD(DAY,0,DATEDIFF(DAY,0,GETDATE())))

	DECLARE @LogUserRegStartTime	DATETIME = DATEADD(DAY,-30,DATEDIFF(DAY,0,@dtStatTime))
	DECLARE @LogUserRegEndTime		DATETIME = DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
	DECLARE @LogStartTime			DATETIME = DATEADD(DAY,-1,DATEADD(DAY,0,@dtStatTime))
	DECLARE @LogEndTime				DATETIME = DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))

	DECLARE @Temp_UserLoginLog	TABLE (UserID INT,CreateTime DATETIME,LogTime DATETIME,[PlatFormID] INT,[AreaID] INT,CountDays INT)
	DECLARE @Temp_UpdateLog		TABLE (CreateTime DATETIME,PlatformID INT,AreaID INT,
										[1] INT,[2] INT,[3] INT,[4] INT,[5] INT,[6] INT,[7] INT,[8] INT,[9] INT,[10] INT,[30] INT,[RegCount] INT)

	/*  取登入紀錄  */
	------------------------------------------------------
	INSERT INTO @Temp_UserLoginLog
	SELECT	A.[UserID],
			A.[CreateTime],
			DATEADD(DAY,0,DATEDIFF(DAY,0,A.[LogTime])) AS [LogTime],
			ULL.[PlatFormID],
			ULL.[AreaID],
			DATEDIFF(DAY,A.CreateTime,@LogStartTime) AS CountDays
	FROM (
			SELECT	URL.[UserID],
					DATEADD(DAY,0,DATEDIFF(DAY,0,URL.[CreateTime])) AS [CreateTime],
					MAX(ULL.LogoutTime) AS [LogTime]
			FROM [Users].[UserRegLog] AS URL WITH (NOLOCK)
				LEFT JOIN [Game].[GameLog] AS ULL WITH (NOLOCK)
					ON URL.UserID = ULL.UserID
			WHERE
				URL.CreateTime >= @LogUserRegStartTime
				AND URL.CreateTime < @LogUserRegEndTime
				AND ULL.LogoutTime >= @LogStartTime
				AND ULL.LogoutTime < @LogEndTime
				AND ULL.[PlatFormID] IS NOT NULL
			GROUP BY URL.UserID,URL.[CreateTime]
		) AS A
		INNER JOIN [Game].[GameLog] AS ULL ON A.UserID = ULL.UserID AND A.[LogTime] = ULL.LogoutTime


	INSERT INTO @Temp_UpdateLog
	SELECT	DISTINCT A.[CreateTime],A.[PlatFormID],A.[AreaID],[1],[2],[3],[4],[5],[6],[7],[8],[9],[10],[30],RUL.[TotalRegCount]
	FROM (
			SELECT	[CreateTime],[PlatFormID],[AreaID],[1],[2],[3],[4],[5],[6],[7],[8],[9],[10],[30]
			FROM (
					SELECT	UserID,[CreateTime],[PlatFormID],[AreaID],CountDays
					FROM @Temp_UserLoginLog AS A
					WHERE
						CountDays IN (1,2,3,4,5,6,7,8,9,10,30)
		) AS A
	PIVOT
		(
			COUNT(UserID) FOR CountDays IN ([1],[2],[3],[4],[5],[6],[7],[8],[9],[10],[30])
		) AS PVT
	) AS A
		INNER JOIN [Stat].[StatisticsRegUser] AS RUL WITH (NOLOCK) ON A.CreateTime = RUL.[StatTime] AND A.[PlatFormID] = RUL.[PlatFormID] AND A.[AreaID] = RUL.[AreaID]
	------------------------------------------------------


	/*  若已有統計數據則刪除後再重新寫入  */
	------------------------------------------------------
	MERGE [Users].[KeepLog] AS A
	USING (
			SELECT	[PlatformID],	[AreaID],	[RegCount],	[CreateTime],
					[1],[2],[3],[4],[5],[6],[7],[8],[9],[10],[30]
			FROM @Temp_UpdateLog
		) AS B ON A.[StatTime] = B.[CreateTime] AND A.[PlatformID] = B.[PlatformID] AND A.[AreaID] = B.[AreaID]
	WHEN MATCHED THEN
		UPDATE SET	A.[RegCount] = B.[RegCount],
					A.[OneDayKeep] = CASE WHEN B.[1] = 0 THEN A.[OneDayKeep] ELSE B.[1] END,
					A.[TwoDayKeep] = CASE WHEN B.[2] = 0 THEN A.[TwoDayKeep] ELSE B.[2] END,
					A.[WeekKeep] = CASE WHEN B.[7] = 0 THEN A.[WeekKeep] ELSE B.[7] END,
					A.[MonthKeep] = CASE WHEN B.[30] = 0 THEN A.[MonthKeep] ELSE B.[30] END,
					A.[Third] = CASE WHEN B.[3] = 0 THEN A.[Third] ELSE B.[3] END,
					A.[Fourth] = CASE WHEN B.[4] = 0 THEN A.[Fourth] ELSE B.[4] END,
					A.[Fifth] = CASE WHEN B.[5] = 0 THEN A.[Fifth] ELSE B.[5] END,
					A.[Sixth] = CASE WHEN B.[6] = 0 THEN A.[Sixth] ELSE B.[6] END,
					A.[eighth] = CASE WHEN B.[8] = 0 THEN A.[eighth] ELSE B.[8] END,
					A.[ninth] = CASE WHEN B.[9] = 0 THEN A.[ninth] ELSE B.[9] END,
					A.[tenth] = CASE WHEN B.[10] = 0 THEN A.[tenth] ELSE B.[10] END
	WHEN NOT MATCHED THEN
		INSERT
			(	[PlatformID],	[AreaID],	[RegCount],		[StatTime],		[OneDayKeep],	[TwoDayKeep],	[WeekKeep],	[MonthKeep],
				[Third],[Fourth],	[Fifth],[Sixth],[eighth],	[ninth],[tenth]	)
		VALUES (B.[PlatformID],	B.[AreaID],	B.[RegCount],	B.[CreateTime],	B.[1],			B.[2],			B.[7],		B.[30],
				B.[3],	B.[4],		B.[5],	B.[6],	B.[8],		B.[9],	B.[10]	);
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_Stat_KeepLog] TO [xbuser]
    AS [dbo];

