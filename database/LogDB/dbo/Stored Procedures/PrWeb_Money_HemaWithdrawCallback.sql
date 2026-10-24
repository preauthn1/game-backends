
-- =============================================
-- Author:		minglung
-- Create date: 2019/7/2
-- Description:	河马支付 提现成功异步回调
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Money_HemaWithdrawCallback]
(
	@chvOrderId			INT,				--订单号
	@intTransStatus		VARCHAR(10),		--第三方状态(01-成功 02-失败)
	@chvTransID			VARCHAR(64)=null,	--第三方订单号
	@chvTransData		VARCHAR(MAX)=null,	--第三方数据	
	@chvErrMsg			VARCHAR(64)		OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @intStatus int

	select @intStatus=Status from Money.ConvertLog with (nolock) where ID=@chvOrderId
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

	if @intTransStatus = '01' set @intStatus=3 --打款已经完成	
	else if @intTransStatus = '02' set @intStatus=100 --自动化处理失败
	else
	begin
		SET @chvErrMsg = '狀態無效'
		RETURN 0
	end


	update Money.ConvertLog 
	set Status=@intStatus, TransID=@chvTransID, TransData=@chvTransData, LastUpdateTime=getdate(), Operator='System'
	where Id=@chvOrderId


	SET @chvErrMsg = ''
	RETURN 1
END