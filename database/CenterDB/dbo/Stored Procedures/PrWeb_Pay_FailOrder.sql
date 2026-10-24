
-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<支付失败>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Pay_FailOrder]
	@intOrderID			INT,					--订单ID
	@chvTransID			VARCHAR(32),			--第三方订单号
	@chvTransData		VARCHAR(max)			--第三方数据
AS
BEGIN
	--更新订单状态
	UPDATE Pay.OrderList
	SET Status = 3
	,TransID = @chvTransID
	,TransData = TransData + '  支付失败:' + @chvTransData
	WHERE Id = @intOrderID AND Status=1 AND CreateTime > DATEADD(WW,-1,GETDATE())

	RETURN 1
END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Pay_FailOrder] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Pay_FailOrder] TO [xbuser]
    AS [dbo];

