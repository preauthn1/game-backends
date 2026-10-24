

-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 更新比賽活動用戶資料
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_Event_UpdateContestUserData]
(
	@intGameID int,
	@intUserID int,
	@intContestID int,
	@intContestNo int,
	@intFlagID int,
	@intTableNo int,
	@intPlaytime int,
	@lngRemainScore bigint,
	@lngScore bigint
)
AS
BEGIN

	Update Event.ContestUserData
	SET 
	ContestID=@intContestID,
	ContestNo=@intContestNo,
	FlagID=@intFlagID,
	TableNo=@intTableNo,
	Playtime=@intPlaytime,
	RemainScore=@lngRemainScore,
	Score=@lngScore 
	where GameID=@intGameID and UserID=@intUserID and ContestID=@intContestID
	if @@ROWCOUNT=0
	begin
		insert into Event.ContestUserData(GameID,UserID,ContestID,ContestNo,FlagID,TableNo,Playtime,RemainScore,Score,CreateTime)
		values(@intGameID,@intUserID,@intContestID,@intContestNo,@intFlagID,@intTableNo,@intPlaytime,@lngRemainScore,@lngScore,GETDATE())
	end

	RETURN 1	
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_Event_UpdateContestUserData] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_Event_UpdateContestUserData] TO [xbuser]
    AS [dbo];

