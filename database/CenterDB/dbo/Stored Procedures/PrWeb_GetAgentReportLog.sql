-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	取得代理投訴問題
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetAgentReportLog]
@intAdminID			int,
@intPlatformID		int=null,
@chvAgentName		varchar(32)=null,
@dtBeginTime		datetime=null,		
@dtEndTime			datetime=null,
@intPageIndex		int=1,
@intPageSize		int=20,
@intPageCount		varchar(32)		output
AS
BEGIN

	--渠道篩選
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT)
	INSERT INTO @Temp_PlatformID Select PlatformID From CenterDB.Area.PlatformAdminUser where AdminID=@intAdminID


	SELECT @intPageCount = COUNT(1) 
	FROM agent.ReportLog as L with(nolock)
	left JOIN Area.Platform as A ON L.PlatformID=A.PlatformID
	WHERE
	 (@intPlatformID IS NULL OR L.PlatformID = @intPlatformID)
	AND (@dtBeginTime IS NULL OR L.CreateTime >= @dtBeginTime)
	AND (@dtEndTime IS NULL OR L.CreateTime <= @dtEndTime)
	AND (@chvAgentName IS NULL OR L.AgentName = @chvAgentName)
	AND L.PlatformID in (select PlatformID from @Temp_PlatformID)
						
	
	;with tab as 
	(
		select L.*,
		A.Name,
		rowid = ROW_NUMBER() over(order by L.CreateTime desc)
		FROM agent.ReportLog as L with(nolock)
		left JOIN Area.Platform as A ON L.PlatformID=A.PlatformID
		WHERE
		 (@intPlatformID IS NULL OR L.PlatformID = @intPlatformID)
		AND (@dtBeginTime IS NULL OR L.CreateTime >= @dtBeginTime)
		AND (@dtEndTime IS NULL OR L.CreateTime <= @dtEndTime)
		AND (@chvAgentName IS NULL OR L.AgentName = @chvAgentName)
		AND L.PlatformID in (select PlatformID from @Temp_PlatformID)
						 
	)
	SELECT * FROM tab WHERE rowid between @intPageSize * (@intPageIndex -1) + 1 and @intPageSize * @intPageIndex
	

	return 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetAgentReportLog] TO [GameUser]
    AS [dbo];

