

-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 紀錄活動比賽分數
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_Event_AddContestScore]
(
	@intContestID int, --場次ID
	@intUserID int,
	@lngScore bigint

)
AS
BEGIN

	SET NOCOUNT ON
	declare @dtNow datetime=GETDATE()
	declare @dtTime time=CAST(@dtNow as Time)
	declare @intYYYYMMDD int=CAST(CONVERT(varchar,@dtNow,112) AS INT)
	declare @dtDeadline time
	declare @intRewardAfterMin int
	declare @dtEndTime datetime
	declare @tnyType tinyint 
	select @intRewardAfterMin=RewardAfterMin,@dtDeadline=Deadline,@dtEndTime=EndTime from Event.Contest where ID=@intContestID

	----檢查是否已經過了截止時間, 不能進行寫分
	--IF  @dtTime >= dateadd(MINUTE,@intRewardAfterMin,@dtDeadline)
	--	return

	--新增成績

	--Update Event.Score set Score=@intScore 
	--where UserID=@intUserID and ContestID=@intContestID and CAST(CONVERT(varchar,CreateTime,112) AS INT)=@intYYYYMMDD
	--IF @@ROWCOUNT=0
	--	Insert into Event.Score(ContestID, UserId, Score, Robot) values(@intContestID, @intUserID, @intScore, 0)

	Insert into Event.Score(ContestID, UserId, Score, Robot) values(@intContestID, @intUserID, @lngScore, 0)

	RETURN 1	
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_Event_AddContestScore] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_Event_AddContestScore] TO [xbuser]
    AS [dbo];

