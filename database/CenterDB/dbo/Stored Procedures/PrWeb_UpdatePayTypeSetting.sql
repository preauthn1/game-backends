
-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<更新PayTypeSetting>
--	2018/04/09	Harry  新增@chvTradeCode
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UpdatePayTypeSetting]
@intGWID INT,							--第三方厂商
@intTypeID INT,							--第三方支付方式
@chvAmountSort VARCHAR(512),			--支付区间
@chvTradeCode VARCHAR(20),			--支付区间
@bitIsRange BIT,						--是否有范围
@bitStatus BIT,							--是否启动
@intMin bigint,							--最小值
@intMax bigint							--最大值
AS
BEGIN
	--更新PayTypeSetting
	UPDATE [CenterDB].[Pay].[PayType]
	SET AmountSort = @chvAmountSort,
	TradeCode = @chvTradeCode,
	IsRange = @bitIsRange,
	Status = @bitStatus,
	MinValue = @intMin,
	MaxValue = @intMax
	WHERE [GWID] = @intGWID AND [TypeID] = @intTypeID

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UpdatePayTypeSetting] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UpdatePayTypeSetting] TO [xbuser]
    AS [dbo];

