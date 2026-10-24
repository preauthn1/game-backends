
-- =============================================
-- Author:  alan
-- Create date: 20180903
-- Description:	游戏捕魚 更新監控
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_SuperTool_Fish_UpdVIPUser]
(
	@intGameID int,
	@intUserID int,
	@intCompleteAmount bigint --完成金额
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @intAssignAmount bigint
	DECLARE @intComplete tinyint=0 --是否已經完成放分 0 未完成 1 完成
	DECLARE @intStatus tinyint=0

	SELECT @intAssignAmount=AssignAmount, @intStatus=Status from  Game.SuperTool_VIPUser with (nolock) where UserID = @intUserID

	--狀態進行中才進行更新
	IF @intStatus = 1
	Begin

		--放分時
		IF @intAssignAmount > 0 AND @intCompleteAmount >= @intAssignAmount
			SET  @intComplete = 1
 
		--殺分時
		IF @intAssignAmount < 0 AND  @intCompleteAmount <= @intAssignAmount
			SET  @intComplete = 1

		UPDATE Game.SuperTool_VIPUser 
		SET 
		CompleteAmount=@intCompleteAmount, 
		Complete=@intComplete, 
		SumAssignAmount=CASE WHEN @intComplete=1 THEN SumAssignAmount+@intCompleteAmount ELSE SumAssignAmount END,
		Status = CASE WHEN @intComplete=1 THEN 0 ELSE 1 END
		Where UserID=@intUserID

	End

	--立即提供server端結果
	SELECT 
	V.UserID
	,V.AssignAmount
	,V.Complete
	,V.CompleteAmount
	,V.Status
	,V.Operator
	,V.SumAssignAmount
	,V.UpdateTime
	,V.FinishTime
	,V.CreateTime
	,ISNULL(R.Value,100) as AssignRate --如果有null, 給最低的放分率
	from Game.SuperTool_VIPUser V with (nolock)
	LEFT JOIN [Game].[SuperTool_RateInfo] R  with (nolock)
	ON V.AssignRate = R.RateID and R.GameID=@intGameID
	where V.Status = 1

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_Fish_UpdVIPUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_Fish_UpdVIPUser] TO [xbuser]
    AS [dbo];

