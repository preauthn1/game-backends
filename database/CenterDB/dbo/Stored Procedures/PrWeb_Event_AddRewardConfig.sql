

-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 新增/更新比賽活動獎勵贈送設定
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Event_AddRewardConfig]
(
	@intID	int=null, --主key
	@intContestID int, --場次ID
	@intRank int, --排名
	@tnyType tinyint, --日結算1 區間結算2
	@lngMoney bigint, --贈送金幣
	@chvRtnMsg	varchar(50) = '' OUTPUT, --錯誤訊息
	@intRtnCode int OUTPUT --錯誤代碼
)
AS
BEGIN

	SET NOCOUNT ON


	IF @intID is null
	BEGIN
		Insert Event.Reward(ContestID,Type,Rank,Money) values(@intContestID,@tnyType,@intRank,@lngMoney) 		
	END
	ELSE
	BEGIN
		Update Event.Reward SET Rank=@intRank, Money=@lngMoney, Type=@tnyType where ID = @intID
	END

	SET @chvRtnMsg = ''
	SET @intRtnCode = 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_AddRewardConfig] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_AddRewardConfig] TO [xbuser]
    AS [dbo];

