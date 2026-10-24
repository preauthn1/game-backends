-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<举报代理>
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_AddReportAgentLog]
	@intAreaID		int,
	@chvAgentName	varchar(32),
	@chvMemo		varchar(512),	
	@chvErrMsg		varchar(128) output
AS
BEGIN
	INSERT INTO Agent.ReportLog(PlatformID,AgentName,Memo,CreateTime)
	SELECT PlatformID, @chvAgentName,@chvMemo,GETDATE()
	FROM Area.Area 
	where AreaID=@intAreaID
	
	set @chvErrMsg = '举报成功'
	return 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_AddReportAgentLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_AddReportAgentLog] TO [xbuser]
    AS [dbo];

