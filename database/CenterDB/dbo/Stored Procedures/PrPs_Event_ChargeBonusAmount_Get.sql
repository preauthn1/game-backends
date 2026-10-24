-- =============================================
-- Author:  jason
-- Create date: 2019/05/28
-- Description: 充值返利活動  取得個人流水及限制
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Event_ChargeBonusAmount_Get]	
(
	@intUserID	INT,
	@chvMsg		VARCHAR(100) OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON

	EXEC [Club02].[dbo].[PrWeb_Event_ChargeBonusAmount_Get]	
		@intUserID = @intUserID,
		@chvMsg = @chvMsg OUTPUT

	/*  回傳欄位  */
	--SELECT	NowFlowAmount = @NowFlowAmount,
	--		FlowAmountLimit = @FlowAmountLimit,
	--		Rate = @Rate
	--		FlowAmountMultiple = @FlowAmountMultiple


	SET @chvMsg = '操作完成'
	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Event_ChargeBonusAmount_Get] TO [GameUser]
    AS [dbo];

