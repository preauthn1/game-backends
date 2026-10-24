-- =============================================
-- Author:  Jason
-- Create date: 2018/03/27
-- Description:	<统计留存率>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetKeepLog]
(
	@intAdminID		INT = null,
	@intPlatformID	INT = 0,			--平台編號 0:全部
	@intAreaID		INT = 0,			--渠道編號
	@dtStartTime	DATETIME = NULL,	--開始時間
	@dtEndTime		DATETIME = NULL		--結束時間
)
AS
BEGIN
	SET NOCOUNT ON

	--渠道篩選
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT)
	INSERT INTO @Temp_PlatformID Select PlatformID From CenterDB.Area.PlatformAdminUser where AdminID=@intAdminID

	Declare @intOpUserType int
	select @intOpUserType=UserType from AdminSys.Admin.Users with (nolock) where UserID=@intAdminID

	if (@intOpUserType <> 10)  --聯運的不能查詢全平台
	begin
		insert into @Temp_PlatformID
		select 0 as platformid
	end

	DECLARE @PlatformID	INT = ISNULL(@intPlatformID,0)
	DECLARE @AreaID		INT = ISNULL(@intAreaID,0)
	DECLARE @StartTime	DATETIME = ISNULL(@dtStartTime,'2000-01-01 00:00:00.000')
	DECLARE @EndTime	DATETIME = ISNULL(@dtEndTime,'2100-01-01 00:00:00.000')



	SELECT	A.[PlatformID],A.[AreaID],
			[RegCount] = ISNULL(A.TotalRegCount,0),
			[OneDayKeep] = ISNULL([OneDayKeep],0),
			[TwoDayKeep] = ISNULL([TwoDayKeep],0),
			[WeekKeep] = ISNULL([WeekKeep],0),
			[MonthKeep] = ISNULL([MonthKeep],0),
			A.[StatTime]
	FROM Stat.RegUserLog AS A
		INNER JOIN [Stat].[KeepLog] AS B WITH (NOLOCK) ON A.PlatformID = B.PlatformID AND A.AreaID = B.AreaID AND A.StatTime = B.StatTime
	WHERE
		(@PlatformID = A.[PlatformID])
		AND (@AreaID = A.[AreaID])
		AND (@StartTime IS NULL OR @StartTime <= A.[StatTime])
		AND (@EndTime IS NULL OR @EndTime >= A.[StatTime])
		AND A.[StatTime] < DATEADD(DAY,0,DATEDIFF(DAY,0,GETDATE()))
		AND A.PlatformID in (select platformID from @Temp_PlatformID)
	ORDER BY A.[StatTime] DESC


END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetKeepLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetKeepLog] TO [xbuser]
    AS [dbo];

