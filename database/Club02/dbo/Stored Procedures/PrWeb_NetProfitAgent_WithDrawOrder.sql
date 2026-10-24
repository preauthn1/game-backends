-- =============================================
-- Author:  jason
-- Create date: 2019/04/15
-- Description: USER 提现(净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_NetProfitAgent_WithDrawOrder]
(
	@intUserID		int,	--用户 ID
	@intAmount		bigint,	--提现金额
	@chvMsg			varchar(100)	OUTPUT
)
AS
BEGIN

	set nocount on
	declare @intReturn	int
	declare @chvUnionpayAccount	varchar(64)

	SELECT @chvUnionpayAccount = UnionpayAccount
	  FROM CenterDB.Users.UserInfo 
	 WHERE UserID = @intUserID	
	
	if ISNULL(@chvUnionpayAccount, '') = ''
	begin 
		set @chvMsg = '操作错误 : 尚未绑定银行卡'
		return -1
	end

	exec @intReturn = [dbo].[PrPs_NetProfitAgent_WithDrawOrder]
		@intUserID	= @intUserID,	--用户 ID
		@intAmount	= @intAmount,	--提现金额
		@chvMsg	= @chvMsg	OUTPUT

	return @intReturn
END







