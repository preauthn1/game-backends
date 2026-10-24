-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<客户端创建充值订单>
-- Changelog:
--		20180905	alan	增加預先寫入@chvTransID, 支持統一支付久聊代理充值
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Pay_CreateOrder]
@intPayTypeID	int,				--充值类型
@intUserID		INT,				--用户ID
@lngAmount		bigint,				--充值金额
@chvIPAddress	varchar(15),		--IP地址
@chvTransID		varchar(32)=null,	--第三方标识
@intOrderID		int output,			--订单号
@chvErrMsg		varchar(128) OUTPUT	--返回消息
AS
SET NOCOUNT ON


declare @intReturn int
exec @intReturn = dbo.PrWeb_Pay_CreateOrder
@intPayTypeID = @intPayTypeID,
@intUserID = @intUserID,
@lngAmount = @lngAmount,
@chvIPAddress = @chvIPAddress,
@chvTransID = @chvTransID,
@chvTransData = '客户端',
@intOrderID = @intOrderID output,
@chvErrMsg = @chvErrMsg output

RETURN @intReturn

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Pay_CreateOrder] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Pay_CreateOrder] TO [xbuser]
    AS [dbo];

