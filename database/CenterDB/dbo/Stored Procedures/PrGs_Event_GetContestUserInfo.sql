


-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 取得比賽活動個人資料
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_Event_GetContestUserInfo]
(
	@intContestID int, --場次ID
	@intUserID int
)
AS
BEGIN

	SET NOCOUNT ON
	Declare @dtNow datetime=GETDATE()
	Declare @intYYYYMMDD int=CAST(CONVERT(varchar,@dtNow,112) AS INT)

	--取得我的最高得分
	declare @lngMyHighScore bigint
	select @lngMyHighScore=Max(score) from Event.Score with (nolock) where UserID=@intUserID and ContestID=@intContestID
	
	--取得我的排名
	declare @intRank int
	select @intRank=Rank from Event.Ranking with (nolock) where UserID=@intUserID and ContestID = @intContestID
	and CAST(CONVERT(varchar,CreateTime,112) AS INT)=@intYYYYMMDD and Type=1
	order by Rank
	
	--取得今日免費次數
	declare @intFreeTicket int
	select @intFreeTicket=Count(ID) from Event.Ticket with (nolock) where UserID=@intUserID 
	and Type=1 --free
	and UpdateTime IS NULL --尚未使用
	--不判斷時間, 尚未使用的也算進去
	--and CAST(CONVERT(varchar,CreateTime,112) AS INT)=@intYYYYMMDD --今日有效
	and ContestID=@intContestID

	select 
	ISNULL(@lngMyHighScore,0) as HighScore
	,ISNULL(@intRank,0) as [Rank]
	,ISNULL(@intFreeTicket,0) as FreeTicket

	RETURN 1
END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_Event_GetContestUserInfo] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_Event_GetContestUserInfo] TO [xbuser]
    AS [dbo];

