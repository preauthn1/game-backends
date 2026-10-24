
-- =============================================
-- Author:  alan
-- Create date: 20181108
-- Description: 新增/更新系統水池输赢记录
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_SuperTool_Fish_UpdSystemPoolWinLost]
(
	@intFlagID int, --初1 中2 高3 级场
	@lngBetMoney bigint, --該魚場玩家總押注金額
	@lngBullet bigint, --總子彈發數
	@lngPoolAmount bigint --系統抽水金額
)
AS
BEGIN

	SET NOCOUNT ON

	IF @lngBetMoney >0 and @lngBullet>0
	BEGIN
		Update [Game].[SuperTool_Fish_SystemPoolWinLost] SET BetMoney=@lngBetMoney, Bullet=@lngBullet
		Where FlagID = @intFlagID 

		if @@ROWCOUNT = 0
		BEGIN
			INSERT into [Game].[SuperTool_Fish_SystemPoolWinLost](FlagID,BetMoney,Bullet)
			Values(@intFlagID,@lngBetMoney,@lngBullet)
		END
	END


	--Update Game.Fish_SystemPool Set Amount=Amount+@lngPoolAmount where FlagID=0 --全部房間共用,不分房
	Update Game.Fish_SystemPool Set Amount=Amount+@lngPoolAmount Where FlagID = @intFlagID  --全部房間共用,不分房
	IF @@ROWCOUNT = 0
		Insert into Game.Fish_SystemPool(FlagID,Amount) Values (@intFlagID,@lngPoolAmount)
	
	Declare @lngSystemPoolAmount bigint
	Select @lngSystemPoolAmount=Amount from Game.Fish_SystemPool where FlagID=@intFlagID

	--取得30天前的用戶輸贏總額
	DECLARE @lngPlayerTotalWinLost bigint
	SELECT	
	@lngPlayerTotalWinLost=SUM(L.Amount) + @lngSystemPoolAmount --用戶輸贏加上系統水池

	FROM Stat.MoneySourceLog AS L WITH (NOLOCK)
	LEFT JOIN ServerInfo.GameServerList AS S WITH (NOLOCK) ON L.SourceID = S.ServerID
	LEFT JOIN ServerInfo.GameList AS G WITH (NOLOCK) ON S.GameID = G.GameID	
	where G.gameid=14 and L.StatTime >= getdate()-30 and s.ShowName=@intFlagID
	AND L.StatTime >= '2018/11/20'--捞取上线时间后的资料
	AND CAST(Convert(varchar,L.StatTime,112) as int) not in (20181216,20181217,20181218,20181219,20181220)--排除特定日期
	GROUP BY S.ShowName


	Select *, ISNULL(@lngPlayerTotalWinLost,0) as PlayerTotalWinLost
	from [Game].[SuperTool_Fish_SystemPoolWinLost] A with (nolock) where FlagID=@intFlagID

	return 1

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_Fish_UpdSystemPoolWinLost] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_Fish_UpdSystemPoolWinLost] TO [xbuser]
    AS [dbo];

