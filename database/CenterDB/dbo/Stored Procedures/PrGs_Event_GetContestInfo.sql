

-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 獲取比賽場次資訊
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_Event_GetContestInfo]
(
	@intGameID int=null --遊戲類型
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @intContestID int
	DECLARE @dtNow datetime=GETDATE()
	
	SELECT top 1 @intContestID=ID From Event.Contest 
	Where 
	(@intGameID is NULL or GameID=@intGameID)
	and @dtNow >=StartTime and @dtNow < EndTime and Status=1 
	order by id asc

	SELECT ID as ContestID,Name,GameID,StartTime,EndTime,Status,Show,Entertime,Deadline, PeriodEntertime, PeriodDeadline, Charge,PlayTime*60 as PlayTime,Coin from Event.Contest where ID=@intContestID
	SELECT * from Event.Bonus where ContestID=@intContestID  order by OrderReach asc
	SELECT * from Event.Reward where ContestID=@intContestID  order by type,rank,money asc

	return 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_Event_GetContestInfo] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_Event_GetContestInfo] TO [xbuser]
    AS [dbo];

