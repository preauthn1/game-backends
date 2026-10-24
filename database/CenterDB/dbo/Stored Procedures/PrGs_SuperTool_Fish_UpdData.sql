
-- =============================================
-- Author:  alan
-- Create date: 20180930
-- Description: 捕魚統計資料
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_SuperTool_Fish_UpdData]
(
	@intFlagID int,
	@intFishID int,
	@intAmount bigint,		--一般金額
	@intRewardAmount bigint --送分金額
)
AS
BEGIN

	SET NOCOUNT ON

	Update Game.SuperTool_Fish_Data 
	SET 
	Amount = Amount+@intAmount,
	RewardAmount = RewardAmount+@intRewardAmount
	Where FlagID=@intFlagID and fishID=@intFishID
	and YYYYMMDD = CAST ( CONVERT(varchar,getdate(),112) AS INT)


	IF @@ROWCOUNT = 0
	BEGIN
		insert into Game.SuperTool_Fish_Data(FlagID,FishID,Amount,RewardAmount,YYYYMMDD) values(@intFlagID,@intFishID,@intAmount,@intRewardAmount,CAST ( CONVERT(varchar,getdate(),112) AS INT))
	END

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_Fish_UpdData] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_Fish_UpdData] TO [xbuser]
    AS [dbo];

