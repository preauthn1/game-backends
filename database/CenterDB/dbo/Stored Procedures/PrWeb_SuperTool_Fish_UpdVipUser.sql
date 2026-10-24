


-- =============================================
-- Author:  alan
-- Create date: 20180908
-- Description:	後台修改 捕魚監控放分
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_Fish_UpdVipUser]
(
	@intUserID			int,
	@intRate			int, --修改爆率
	@intAssignAmount	bigint, --指定放分金額 "分"
	@intStatus			tinyint, --0 暂停 1 继续放分
	@chvOperator		varchar(20)
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @intPrevFinsih tinyint

	--取得最後狀態是否已經完成放分: 1完成 0否
	SELECT @intPrevFinsih = Finish from Game.SuperTool_Fish_VIPUser with (nolock) where UserID = @intUserID
	
	--重新指定放分	
	IF @intPrevFinsih = 1 or @intPrevFinsih is null
	BEGIN

		UPDATE Game.SuperTool_Fish_VIPUser 
		SET AssignAmount=@intAssignAmount,
		Rate=@intRate,
		Status=@intStatus,
		Finish=0, 
		FinishAmount=0, 
		FinishTime=NULL,
		Operator=@chvOperator, 
		CreateTime=Getdate()
		Where UserID=@intUserID

	END
	ELSE
		--暫停 or 開始
		UPDATE Game.SuperTool_Fish_VIPUser SET AssignAmount=@intAssignAmount, Rate=@intRate, Status=@intStatus, CreateTime=Getdate()
		Where UserID=@intUserID

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_Fish_UpdVipUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_Fish_UpdVipUser] TO [xbuser]
    AS [dbo];

