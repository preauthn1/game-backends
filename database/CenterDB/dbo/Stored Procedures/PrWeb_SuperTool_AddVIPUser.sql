

-- =============================================
-- Author:  alan
-- Create date: 20180903
-- Description:	後台添加或更新遊戲監控
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_AddVIPUser]
(
	@intUserID int,
	@intAssignAmount bigint, --指定放分金額, 單位"分"
	@intAssignRate int, -- 放分勝率
	@intStatus	int, --开始暂停按钮 1 開始 0 暫停
	@chvOperator varchar(50)
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @intComplete tinyint
	DECLARE @intCompleteAmount bigint

	IF NOT EXISTS (Select 1 from Game.SuperTool_VIPUser with (nolock) where UserID = @intUserID)
	BEGIN
		INSERT INTO Game.SuperTool_VIPUser(UserID, AssignAmount, AssignRate, Complete, Status, SumAssignAmount, Operator)
		VALUES(@intUserID, @intAssignAmount, @intAssignRate, 0, @intStatus, @intAssignAmount, @chvOperator)

	END
	ELSE
	BEGIN
		--是否完成: 1完成 0否
		SELECT @intComplete = Complete, @intCompleteAmount=CompleteAmount from Game.SuperTool_VIPUser with (nolock) where UserID = @intUserID
		IF @@ROWCOUNT = 0
			return 0
		
		IF @intComplete = 1
		BEGIN
			--已經完成, 金額/狀態歸0
			SET @intCompleteAmount = 0
			SET @intComplete = 0
		END

		--启动/暂停/编辑
		UPDATE Game.SuperTool_VIPUser 
		SET 
		AssignAmount=@intAssignAmount, 
		AssignRate=@intAssignRate, 
		Status=@intStatus, 
		Operator=@chvOperator, 
		CompleteAmount=@intCompleteAmount, 
		Complete=@intComplete,
		UpdateTime =GETDATE()
		Where UserID=@intUserID

	END

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_AddVIPUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_AddVIPUser] TO [xbuser]
    AS [dbo];

