-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<查询游戏在线人数曲线图数据>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetGameOnlineChatData]
	@intPlatformID	int,
	@dtBeginTime	datetime,
	@dtEndTime		datetime,	
	@intGameID		int=null,
	@tintDatType	int
AS
BEGIN

	IF @tintDatType = 1
	BEGIN
		SELECT Convert(varchar,StatTime,102) as StatTime
		,MAX(TotalCount) as TotalCount
		,MAX(PCCount) as PCCount
		,MAX(IOSCount) as IOSCount
		,MAX(AndroidCount) as AndroidCount	
		FROM Stat.GameOnLineUserLog where (PlatformID = @intPlatformID) AND (@intGameID IS NULL OR GameID = @intGameID) AND (@dtBeginTime IS NULL OR StatTime>=@dtBeginTime) AND (@dtEndTime IS NULL OR StatTime<=@dtEndTime)
		Group by Convert(varchar,StatTime,102)		
		order by StatTime
	END
	ELSE IF @tintDatType = 2
	BEGIN
		SELECT Convert(varchar,min(StatTime),102)  as StatTime
		,MAX(TotalCount) as TotalCount
		,MAX(PCCount) as PCCount
		,MAX(IOSCount) as IOSCount
		,MAX(AndroidCount) as AndroidCount	
		FROM Stat.GameOnLineUserLog where (PlatformID = @intPlatformID) AND (@intGameID IS NULL OR GameID = @intGameID) AND (@dtBeginTime IS NULL OR StatTime>=@dtBeginTime) AND (@dtEndTime IS NULL OR StatTime<=@dtEndTime)
		Group by datepart(ww,StatTime)	
		order by StatTime
	END
	ELSE IF @tintDatType=3
	BEGIN
		SELECT Convert(varchar(7),min(StatTime),102)  as StatTime
		,MAX(TotalCount) as TotalCount
		,MAX(PCCount) as PCCount
		,MAX(IOSCount) as IOSCount
		,MAX(AndroidCount) as AndroidCount	
		FROM Stat.GameOnLineUserLog where (PlatformID = @intPlatformID) AND (@intGameID IS NULL OR GameID = @intGameID) AND (@dtBeginTime IS NULL OR StatTime>=@dtBeginTime) AND (@dtEndTime IS NULL OR StatTime<=@dtEndTime)
		Group by datepart(m,StatTime)
		order by StatTime
	END
	
	
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetGameOnlineChatData] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetGameOnlineChatData] TO [xbuser]
    AS [dbo];

