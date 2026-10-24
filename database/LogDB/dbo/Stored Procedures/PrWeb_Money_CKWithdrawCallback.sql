-- =============================================
-- Author:		minglung
-- Create date: 2019/6/1
-- Description:	CK支付 提现成功异步回调
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Money_CKWithdrawCallback]
(
	@chvOrderId			INT,			--订单号
	@intTransStatus		INT,			--第三方状态(1:已提交,2:已处理,3:已退回,4.已出账)
	@chvTransID			VARCHAR(64),	--第三方订单号
	@chvTransData		VARCHAR(MAX),	--第三方数据	
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

	if @intTransStatus = 4 set @intStatus=3 --打款已经完成
	else if @intTransStatus IN (1,2) set @intStatus=99 --自动化进行中
	else if @intTransStatus = 3 set @intStatus=100 --自动化处理失败
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


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_CKWithdrawCallback] TO [GameUser]
    AS [dbo];

