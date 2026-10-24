
-- =============================================
-- Author:  jason
-- Create date: 20190214
-- Description: 21點新增/更新系統水池输赢记录
-- 2019/02/19 @lngBetMoney,@lngBetCounts由原本的當日0點往前推30日改為查詢當下時間往前推30日
--			  更新BetMoney,BetCounts,PoolAmount欄位改為壘加
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_SuperTool_TwentyOne_UpdSystemPoolWinLost]
(
	@intFlagID int, --初1 中2 高3 级场
	@lngBetMoney bigint, --該21點玩家總押注金額
	@lngBetCounts bigint, --總下注次數
	@lngPoolAmount bigint --系統抽水金額
)
AS
BEGIN

	SET NOCOUNT ON

	/* 2019/02/19
	--DECLARE @PoolDate DATETIME = DATEADD(DAY,0,DATEDIFF(DAY,0,GETDATE()))
	--DECLARE @PoolAmount BIGINT
	--DECLARE @PerMonth DATETIME = DATEADD(DAY,-30,DATEDIFF(DAY,0,GETDATE()))

	update [Game].[SuperTool_TwentyOne_SystemPoolWinLost]
	set BetMoney = @lngBetMoney,
		BetCounts = @lngBetCounts,
		PoolAmount = @lngPoolAmount
	where
		PoolDate = @PoolDate
		and FlagID = @intFlagID
	*/
	DECLARE @LogDate DATETIME = DATEADD(DAY,0,DATEDIFF(DAY,0,GETDATE()))
	DECLARE @PoolDate DATETIME = GETDATE()
	DECLARE @PoolAmount BIGINT
	DECLARE @PerMonth DATETIME = GETDATE()-30

	update [Game].[SuperTool_TwentyOne_SystemPoolWinLost]
	set BetMoney = BetMoney + @lngBetMoney,
		BetCounts = BetCounts + @lngBetCounts,
		PoolAmount = PoolAmount + @lngPoolAmount
	where
		PoolDate = @LogDate
		and FlagID = @intFlagID

	if @@ROWCOUNT = 0
	BEGIN
		INSERT into [Game].[SuperTool_TwentyOne_SystemPoolWinLost](PoolDate,FlagID,BetMoney,BetCounts,PoolAmount)
		Values(@PoolDate,@intFlagID,@lngBetMoney,@lngBetCounts,@lngPoolAmount)
	END

	select @PoolAmount = SUM([PoolAmount]) from [Game].[SuperTool_TwentyOne_SystemPoolWinLost] A with (nolock)
	WHERE FlagID = @intFlagID
	
	set @PoolAmount = ISNULL(@PoolAmount,0)
	
	Select FlagID,SUM(BetMoney) AS BetMoney,SUM(BetCounts) AS BetCounts,@PoolAmount AS PoolAmount
	from [Game].[SuperTool_TwentyOne_SystemPoolWinLost] A with (nolock) where PoolDate >= @PerMonth and PoolDate <= @PoolDate and FlagID = @intFlagID
	group by FlagID

	return 1

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_TwentyOne_UpdSystemPoolWinLost] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_TwentyOne_UpdSystemPoolWinLost] TO [xbuser]
    AS [dbo];

