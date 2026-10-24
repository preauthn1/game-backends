
-- =============================================
-- Author:  alan
-- Create date: 20181108
-- Description: 新增/更新玩家个人水池输赢记录
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_SuperTool_Fish_UpdPlayerPoolWinLost]
(
	@intFlagID int, --初1 中2 高3 级场
	@intUserID int,
	@lngHitMoney bigint, --打死魚的總金額
	@lngSpendMoney bigint --玩家在這個魚場花出的總金額
)
AS
BEGIN

	SET NOCOUNT ON

	Update [Game].[SuperTool_Fish_PlayerPoolWinLost] SET [HitMoney]=@lngHitMoney, SpendMoney=@lngSpendMoney
	Where FlagID = @intFlagID and UserID=@intUserID

	if @@ROWCOUNT = 0
	BEGIN
		INSERT into [Game].[SuperTool_Fish_PlayerPoolWinLost](FlagID,UserID,HitMoney,SpendMoney)
		Values(@intFlagID,@intUserID,@lngHitMoney,@lngSpendMoney)
	END

	--找捕魚上線的玩家(如果没有线上玩家或userID=0, 则回传全部资料)
	select userID into #onlineuser from game.GameUserList with (nolock) where [ServerID] in (select serverid from ServerInfo.GameServerList with (nolock) where gameid=14)
	IF @@ROWCOUNT = 0 OR @intUserID=0
	Begin
		Select A.* 
		from [Game].[SuperTool_Fish_PlayerPoolWinLost] A with (nolock) 
		left join [Game].[SuperTool_Fish_PlayerPool] B with (nolock) on A.FlagID=B.FlagID
		Where (@intFlagID is NULL or A.FlagID = @intFlagID)
		and B.Status=1
		return 1
	END

	Select A.* 
	from [Game].[SuperTool_Fish_PlayerPoolWinLost] A with (nolock) 
	left join [Game].[SuperTool_Fish_PlayerPool] B with (nolock) on A.FlagID=B.FlagID
	Where (@intFlagID is NULL or A.FlagID = @intFlagID)
	and B.Status=1
	and A.UserID in (select UserID from #onlineuser)
	return 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_Fish_UpdPlayerPoolWinLost] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_Fish_UpdPlayerPoolWinLost] TO [xbuser]
    AS [dbo];

