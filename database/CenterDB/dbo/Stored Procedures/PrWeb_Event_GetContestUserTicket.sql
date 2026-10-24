

-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 後台取得比賽活動個人資料
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Event_GetContestUserTicket]
(
	@intGameID int,
	@intUserID int,
	@dtStartTime datetime=null,
	@dtEndTime datetime=null,
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 50,
	@intPageCount	INT = 0	 output
)
AS
BEGIN

	--查免費券
	Select 
	C.Name, --活動名稱
	T.UserID,
	T.CreateTime, --領取時間
	T.UpdateTime, --使用時間
	T.Type --1免費 2自費
	INTO #T
	from Event.Ticket T
	left join Event.Contest C on T.ContestID=C.ID
	where UserID=@intUserID 
	and C.GameID=@intGameID
	and (@dtStartTime is null or T.CreateTime>=@dtStartTime)
	and (@dtEndTime is null or T.CreateTime<=@dtEndTime)
	


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
    ON OBJECT::[dbo].[PrWeb_Event_GetContestUserTicket] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_GetContestUserTicket] TO [xbuser]
    AS [dbo];

