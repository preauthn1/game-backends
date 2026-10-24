


-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 後台取得比賽活動排行資料
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Event_GetContestUserRank]
(
	@intGameID int=null,
	@intUserID int,
	@dtStartTime datetime=null,
	@dtEndTime datetime=null,
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 50,
	@intPageCount	INT = 0	output
)
AS
BEGIN

	--查該用戶每場排名
	select C.name, 
	r.UserID,
	r.Type, --1 日賽排行, 2累積賽排行
	r.Rank, --排行
	r.Score, --分數
	r.LastCalcRank, --是否完成結算 0未完成, 1完成
	r.CreateTime, --排行時間
	ISNULL(r.Reward,0) as Reward--獎勵金額
	INTO #T
	from  
	CenterDB.Event.Ranking r
	left join event.Contest c on r.ContestID=c.ID
	where userid=@intUserID
	and (@intGameID is null or C.GameID=@intGameID)
	and (@dtStartTime is null or R.CreateTime>=@dtStartTime)
	and (@dtEndTime is null or R.CreateTime<=@dtEndTime)


	SELECT	@intPageCount = COUNT(1)
	FROM (
			SELECT * FROM #T WITH (NOLOCK)
	) AS A


	;WITH CTE
	AS (
		SELECT ROW_NUMBER() OVER (ORDER BY A.CreateTime DESC) AS rowNum, *
		FROM (
				SELECT T.* FROM #T AS T

			) AS A
		)
		SELECT * FROM CTE WHERE rowNum BETWEEN @intPageSize * (@intPageIndex -1) + 1 AND @intPageSize * @intPageIndex


END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_GetContestUserRank] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_GetContestUserRank] TO [xbuser]
    AS [dbo];

