-- =============================================
-- Author:  jason
-- Create date: 2019/04/15
-- Description: USER 可提现金额查询(净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_NetProfitAgent_GetBonusAmount]
(
	@intUserID		int	--用户 ID
)
AS
BEGIN

	set nocount on

	select	[poolamount] = [PoolAmount] - [WithDrawAmount]
	from [dbo].[NetProfitAgent_Pool] with (nolock)
	where
		userid = @intUserID	
	
	return 1
END





