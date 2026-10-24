

-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 取得比賽排行
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Event_GetContestRank]
(
@intContestID int,
@dtRankDate datetime=null,
@intRankType int
)
AS
BEGIN

	SET NOCOUNT ON
	declare @dtNow datetime

	IF @dtRankDate IS NULL SET @dtNow=getdate() ELSE SET @dtNow=@dtRankDate

	select top 10 UserID,Rank,Score,Type from Event.Ranking 
	where 
	ContestID=@intContestID 
	and CAST(CONVERT(varchar,CreateTime,112) AS INT)=CAST(CONVERT(varchar,@dtNow,112) AS INT)
	and Type = @intRankType
	order by Rank asc



End

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_GetContestRank] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_GetContestRank] TO [xbuser]
    AS [dbo];

