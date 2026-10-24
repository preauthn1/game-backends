-- =============================================
-- Author:  alan
-- Create date: 2018/08/6
-- Description: 取得玩家輸贏排行榜
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Game_GetUserWinLostLeaderboard]
(
	@intAdminID	int,
	@intUserID		int = null,
	@dtStartTime	datetime = null,
	@dtEndTime		datetime = null,
	@tnyColumns 	tinyint = 0, --0:payAmount 1:convertAmount
	@tnySort 		tinyint = 0 --0:升序 asc 1:降序 desc	
)
AS
BEGIN

	SET NOCOUNT ON

	IF @dtStartTime IS NULL AND @dtEndTime IS NULL
	BEGIN
		SET @dtEndTime = GETDATE()
		SET @dtStartTime = DATEADD(DAY,-1,@dtEndTime)
	END

	IF @dtStartTime IS NULL 
	BEGIN
		SET @dtStartTime = GETDATE()
	END

	IF @dtEndTime IS NULL
		SET @dtEndTime = GETDATE()


	--渠道篩選
	DECLARE @Temp_AreaID TABLE (AreaID	INT)
	INSERT INTO @Temp_AreaID Select AreaID From CenterDB.Area.AreaAdminUser where AdminID=@intAdminID

		
	CREATE TABLE #T (
		UserID int,
		payAmount bigint,
		convertAmount bigint
	)

	INSERT INTO #t (UserID, payAmount, convertAmount)
	SELECT CASE WHEN aa.UserID IS NULL THEN bb.UserID ELSE aa.UserID END as UserID, ISNULL(aa.amount,0) as payAmount, ISNULL(bb.amount,0) as convertAmount
	FROM (
		select distinct userid, sum(UpdateAmount/100) as amount 
		from Logdb.Money.MoneyLog a with (nolock) 
		where a.CreateTime >= @dtStartTime and a.CreateTime <= @dtEndTime
		AND a.SourceTypeID >=11001 and a.SourceTypeID <=11020 --此段範圍都屬充值類型
		AND a.AreaID in (select areaid from @Temp_AreaID)
		group by userid

	) AS aa
	full outer join 
	(
		select  distinct userid, sum(Amount/100) as amount from logdb.Money.ConvertLog a with (nolock) 
		where a.Status=3 and a.CreateTime >= @dtStartTime and a.CreateTime <= @dtEndTime
		AND a.AreaID in (select areaid from @Temp_AreaID)
		group by userid
	) AS bb
	ON aa.UserID = bb.UserID



	SELECT * FROM #T
	WHERE ISNULL(@intUserID, USERID) = USERID

	ORDER BY CASE 
	WHEN  @tnyColumns = 0 AND @tnySort = 1 THEN payAmount
	WHEN  @tnyColumns = 0 AND @tnySort = 0 THEN (0-payAmount)
	WHEN  @tnyColumns = 1 AND @tnySort = 1 THEN convertAmount
	WHEN  @tnyColumns = 1 AND @tnySort = 0 THEN (0-convertAmount)
	END DESC

	DROP TABLE #T


END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_GetUserWinLostLeaderboard] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_GetUserWinLostLeaderboard] TO [xbuser]
    AS [dbo];

