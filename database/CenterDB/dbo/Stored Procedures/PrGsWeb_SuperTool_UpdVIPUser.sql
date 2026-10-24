
-- =============================================
-- Author:  alan
-- Create date: 20180903
-- Description:	遊戲SERVER更新遊戲監控"
-- =============================================
CREATE PROCEDURE [dbo].[PrGsWeb_SuperTool_UpdVIPUser]
(
	@intUserID int,
	@intAssignAmount bigint = null, --放分金額 (單位分)
	@intAssignRate int=null, --放分勝率 
	@intStatus tinyint=null, --開始1 暫停0
	@intComplete tinyint=null, --1完成 0未完成
	@intCompleteAmount bigint=null --完成金額 (單位分)
)
AS
BEGIN

	SET NOCOUNT ON
	Declare @dtStartTime datetime = null
	IF @intStatus = 1
		SET @dtStartTime = getdate()

	IF @intComplete = 1
		SET @intStatus = 0
		

	Update  Game.SuperTool_VIPUser SET AssignAmount = ISNULL(@intAssignAmount, AssignAmount),
	AssignRate = ISNULL(@intAssignRate, AssignRate),
	Status = ISNULL(@intStatus, Status),
	Complete = ISNULL(@intComplete, Complete),
	CompleteAmount = ISNULL(@intCompleteAmount, CompleteAmount),
	SumAssignAmount=CASE WHEN @intComplete=1 THEN SumAssignAmount+@intCompleteAmount ELSE SumAssignAmount END,
	UpdateTime =  ISNULL(@dtStartTime, UpdateTime)
	WHERE UserID = @intUserID

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGsWeb_SuperTool_UpdVIPUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGsWeb_SuperTool_UpdVIPUser] TO [xbuser]
    AS [dbo];

