
-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 取得比赛场次资讯
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Event_GetContest]
(
	@intContestID int=null, --场次id
	@dtStartTime datetime=null,
	@dtEndTime datetime=null
)
AS
BEGIN

	SET NOCOUNT ON
	
	SELECT C.*,G.GameName from Event.Contest C with (nolock) 
	left join Serverinfo.GameList G with (nolock) on C.GameID=G.GameID
	where 
	(@intContestID is null or ID=@intContestID )
	AND 
	( 
	  @dtStartTime is null or
		( @dtStartTime <= StartTime AND @dtEndTime >= EndTime ) or
		( @dtStartTime <= StartTime AND @dtEndTime >= StartTime) or
		( @dtStartTime >= StartTime AND @dtStartTime <= EndTime) or
		( @dtStartTime >= StartTime AND @dtEndTime <= EndTime)

	) 
	AND 
	( 
	  @dtEndTime is null or
		( @dtStartTime <= StartTime AND @dtEndTime >= EndTime ) or
		( @dtStartTime <= StartTime AND @dtEndTime >= StartTime) or
		( @dtStartTime >= StartTime AND @dtStartTime <= EndTime) or
		( @dtStartTime >= StartTime AND @dtEndTime <= EndTime)
	) 


END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_GetContest] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_GetContest] TO [xbuser]
    AS [dbo];

