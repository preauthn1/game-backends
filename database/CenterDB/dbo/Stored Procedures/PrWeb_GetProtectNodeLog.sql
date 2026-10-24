-- =============================================
-- Author:  alan
-- Create date: 2018-8-14
-- Description:	取得节点登入记录
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetProtectNodeLog]
@intUserID			int=null,			--用户ID
@chvNodeIPAddress	varchar(32)=null,		--节点ip
@dtBeginTime		datetime=null,		
@dtEndTime			datetime=null,
@intPageIndex		int=1,
@intPageSize		int=20,
@intPageCount		int		output
AS
BEGIN



	SELECT @intPageCount = COUNT(1) 
	FROM Game.ProtectNodeLog as L with(nolock)
	WHERE 
	(@intUserID IS NULL OR L.UserID = @intUserID)
	AND (@chvNodeIPAddress IS NULL OR L.NodeIP = @chvNodeIPAddress)
	AND (@dtBeginTime IS NULL OR L.CreateTime >= @dtBeginTime)
	AND (@dtEndTime IS NULL OR L.CreateTime <= @dtEndTime)
	AND L.CreateTime >= DATEADD(DAY,-3,GETDATE())						
	
	;with tab as 
	(
		select L.*,
		rowid = ROW_NUMBER() over(order by L.ID desc)
		FROM Game.ProtectNodeLog as L with(nolock)
		WHERE
		(@intUserID IS NULL OR L.UserID = @intUserID)
		AND (@chvNodeIPAddress IS NULL OR L.NodeIP = @chvNodeIPAddress)
		AND (@dtBeginTime IS NULL OR L.CreateTime >= @dtBeginTime)
		AND (@dtEndTime IS NULL OR L.CreateTime <= @dtEndTime)
		AND L.CreateTime >= DATEADD(DAY,-3,GETDATE())
						 
	)
	SELECT * FROM tab WHERE rowid between @intPageSize * (@intPageIndex -1) + 1 and @intPageSize * @intPageIndex
	

	return 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetProtectNodeLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetProtectNodeLog] TO [xbuser]
    AS [dbo];

