-- =============================================
-- Author:		alan
-- Create date: 2019/5/7
-- Description:	统一支付 提现成功异步回调
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Money_TungyiWithdrawCallback]
(
	@chvOrderID			varchar(50), --支付中心订单号
	@chvTeamOrderId		varchar(50), --项目组订单号
	@intOrderAmount		bigint,		 --支付金额,单位:分
	@intrealAmount		bigint,		 --实际支付金额,单位:分
	@chvStatus			varchar(20), --订单状态,pending:处理中,success:处理成功,fail:处理失败
	@chvTime			varchar(50), --交易结束时间,unix秒级时间戳
	@chvErrMsg			VARCHAR(64)		OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @intStatus int

	select @intStatus=Status from Money.ConvertLog with (nolock) where ID=Convert(int,@chvTeamOrderId)
	if @@ROWCOUNT=0
	begin
		SET @chvErrMsg = '兑换订单号不存在'
		RETURN 0
	end

	if @intStatus=3
	begin
		SET @chvErrMsg = '無效處理,打款已经完成'
		RETURN 1
	end



	if lower(@chvStatus) = 'success' set @intStatus=3 --打款已经完成
	else if lower(@chvStatus) = 'pending' set @intStatus=99 --自动化进行中
	else if lower(@chvStatus) = 'fail' set @intStatus=100 --自动化处理失败
	else
	begin
		SET @chvErrMsg = '狀態無效(' + isnull(@chvStatus,'null') + ')'
		RETURN 0
	end


	update Money.ConvertLog 
	set Status=@intStatus, TransID=@chvOrderID, TransData=@chvStatus, LastUpdateTime=getdate(), Operator='System'
	where Id=@chvTeamOrderId


	SET @chvErrMsg = ''
	RETURN 1
END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_TungyiWithdrawCallback] TO [GameUser]
    AS [dbo];

