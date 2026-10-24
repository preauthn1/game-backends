-- =============================================
-- Author:  Jason
-- Create date: 2019/03/19
-- Description:	總遊戲在線人數 (原本使用ORM改為呼叫SP取資料)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetOnlineUserCount]
(
	@intPlatformID	INT = 0,			--平台編號 0:全部 1:
	@intAreaID		INT = 0,			--渠道編號
	@dtStartTime	DATETIME = NULL,	--開始時間
	@dtEndTime		DATETIME = NULL		--結束時間
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @PlatformID	INT = ISNULL(@intPlatformID,0)
	DECLARE @AreaID		INT = ISNULL(@intAreaID,0)
	DECLARE @StartTime	DATETIME = ISNULL(@dtStartTime,'2000-01-01 00:00:00.000')
	DECLARE @EndTime	DATETIME = ISNULL(@dtEndTime,'2100-01-01 00:00:00.000')


	IF(DATEDIFF(DAY,@StartTime,@EndTime) > 3)
	BEGIN
		SET @StartTime = DATEADD(DAY,-3,@dtEndTime)
		SET @EndTime = @dtEndTime
	END

	/*  回傳統計數據  */
	------------------------------------------------------------------------------------------------------
	IF(@PlatformID = 0)
	BEGIN
		SELECT	[Id],
				[PlatformID],
				AreaName = '全部',
				[TotalCount],
				[PCCount],
				[IOSCount],
				[AndroidCount],
				[StatTime]
		FROM [Stat].[TotalGameOnLineLog] WITH (NOLOCK)
		WHERE
			[PlatformID] = @PlatformID
			AND [StatTime] >= @StartTime AND [StatTime] <= @EndTime
		ORDER BY [StatTime] DESC
	END
	ELSE
	BEGIN
		SELECT	B.Id,
				A.[PlatformID],
				A.AreaName,
				B.TotalCount,
				B.PCCount,
				B.IOSCount,
				B.AndroidCount,
				B.[StatTime]
		FROM [Area].[Area] AS A WITH (NOLOCK)
			INNER JOIN [Stat].[TotalGameOnLineLogArea] AS B ON A.AreaID = B.AreaID
		WHERE
			 (@PlatformID = A.[PlatformID] OR @PlatformID = 0)
			AND (@AreaID = A.[AreaID] OR @AreaID = 0)
			AND [StatTime] >= @StartTime AND [StatTime] <= @EndTime
		ORDER BY [StatTime] DESC
	END
	------------------------------------------------------------------------------------------------------
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetOnlineUserCount] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetOnlineUserCount] TO [xbuser]
    AS [dbo];

