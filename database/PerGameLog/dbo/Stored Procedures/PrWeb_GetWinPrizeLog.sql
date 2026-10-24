
-- =============================================
-- Author:  alan
-- Create date: 2018/7/26
-- Description:	获取指定开奖记录
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetWinPrizeLog]	
@intGameID			int,
@intServerID		int,
@intTableNo			int,
@dtBeginTime		datetime=null,		
@dtEndTime			datetime=null,
@intPageIndex		int=1,
@intPageSize		int=20,
@intPageCount		int output
AS
BEGIN


	SELECT @intPageCount = COUNT(1) 
	FROM [PerLog].[WinPrizeLog] with(nolock)
	WHERE
	GameID = @intGameID
	AND ServerID = @intServerID
	AND TableNo = @intTableNo
	AND (@dtBeginTime IS NULL OR CreateTime >= @dtBeginTime)
	AND (@dtEndTime IS NULL OR CreateTime <= @dtEndTime)

	;with tab as 
	(
		select *,
		rowid = ROW_NUMBER() over(order by Id desc)
		FROM [PerLog].[WinPrizeLog] with(nolock)
		WHERE
		GameID = @intGameID
		AND ServerID = @intServerID
		AND TableNo = @intTableNo
		AND (@dtBeginTime IS NULL OR CreateTime >= @dtBeginTime)
		AND (@dtEndTime IS NULL OR CreateTime <= @dtEndTime)
	)
	SELECT * FROM tab WHERE rowid between @intPageSize * (@intPageIndex -1) + 1 and @intPageSize * @intPageIndex
	

	return 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetWinPrizeLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetWinPrizeLog] TO [xbuser]
    AS [dbo];

