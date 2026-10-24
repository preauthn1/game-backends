

-- =============================================
-- Author:  alan
-- Create date: 20180908
-- Description:	後端修改 捕魚系統放分
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_Fish_UpdSystemReward]
(
	@intFlagID int,
	@intFishID int,
	@intAmount bigint=null, --修改放分金額
	@intRate	int=null, --修改爆率
	@chvOperator varchar(20)
)
AS
BEGIN

	SET NOCOUNT ON
	DECLARE @intStatus tinyint
	DECLARE @intPrevFinsih tinyint
	
	--取得目前狀態
	SELECT @intStatus=Status, @intPrevFinsih = Finish  From Game.SuperTool_Fish_SystemReward where FishID = @intFishID and flagID=@intFlagID

	--沒有放分時才能修改
	IF @intStatus = 0
	BEGIN
	    IF @intPrevFinsih = 1 or @intPrevFinsih is null
			BEGIN

				Update Game.SuperTool_Fish_SystemReward 
				SET 
				Amount = ISNULL(@intAmount,Amount),
				Rate = ISNULL(@intRate,Rate),
				FinishAmount=0,
				Finish=0,
				FinishTime=NULL,
				Operator = @chvOperator,
				CreateTime =GETDATE()
				Where FishID = @intFishID and flagID=@intFlagID
			END
		ELSE
			BEGIN
				Update Game.SuperTool_Fish_SystemReward 
				SET 
				Amount = ISNULL(@intAmount,Amount),
				Rate = ISNULL(@intRate,Rate),
				Operator = @chvOperator,
				CreateTime =GETDATE()
				Where FishID = @intFishID and flagID=@intFlagID
			END
	END

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_Fish_UpdSystemReward] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_Fish_UpdSystemReward] TO [xbuser]
    AS [dbo];

