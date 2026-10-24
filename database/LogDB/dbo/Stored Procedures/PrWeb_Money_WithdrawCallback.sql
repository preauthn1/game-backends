-- =============================================
-- Author:		CCC
-- Create date: 2019/7/17
-- Description:	提现成功异步回调
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Money_WithdrawCallback]
(   
    @intOrderId			INT,			--兑换单号
	@intStatus			INT,			--兑换状态
    @chvTransId			varchar(200),	--第三方单号
    @chvTransData		varchar(200),	--第三方讯息		
	@chvErrMsg			VARCHAR(64)	OUTPUT
)
AS
BEGIN
	
	DECLARE @intConvertStatus int

	select @intConvertStatus=Status from Money.ConvertLog with (nolock) where ID=@intOrderId
	if @@ROWCOUNT=0
	begin
		SET @chvErrMsg = '兑换订单号不存在'
		RETURN 0
	end

	if @intConvertStatus=3
	begin
		SET @chvErrMsg = '無效處理,打款已经完成'
		RETURN 1
	end	

	update [LogDB].[Money].[ConvertLog] 
	   set Status=@intStatus, 
	       TransId=@chvTransId, 
	       TransData=@chvTransData,
		   LastUpdateTime=getdate(),
		   Operator='System'
	 where Id=@intOrderId 

	 SET @chvErrMsg = '更新成功'
	 RETURN 1
END