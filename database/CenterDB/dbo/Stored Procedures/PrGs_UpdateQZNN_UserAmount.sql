
-- =============================================
-- Author:  20190325
-- Create date: jason
-- Description:	<更新搶庄牛牛彩金池至用戶保險箱>
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_UpdateQZNN_UserAmount]
(
	@intUserID			int,					--用户ID
	@lngUpdateAmount	BIGINT,					--更新的数量
	@chvErrMsg			VARCHAR(128) OUTPUT     --错误消息
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @sintSourceType INT = 12000		--更新方式
	DECLARE @chvIPAddress	VARCHAR(40) = '0.0.0.0'
	DECLARE @chvMemo		VARCHAR(20) = '抢庄牛牛彩金池发送'	--说明
	DECLARE @chvTitle		VARCHAR(100) = '恭喜您中奖!!!'
	DECLARE @lngCurrentAmount	BIGINT
	--发送到账邮件
	DECLARE @chvContent		VARCHAR(500) = '恭喜您得到彩金'+dbo.Fn_ShowMoney(@lngUpdateAmount)+' , 祝您游玩顺心,好运一直来'

	IF(@lngUpdateAmount < 0)
	BEGIN
		SET @lngCurrentAmount	= 0
		SET @chvErrMsg			= '金额错误'
		RETURN 0
	END

	EXEC [dbo].[PrPsWeb_UpdateUserBank]
		@intUserID			= @intUserID,				--用户ID
		@sintSourceType		= @sintSourceType,			--更新方式
		@lngUpdateAmount	= @lngUpdateAmount,			--更新的数量
		@chvIPAddress		= @chvIPAddress,			--IP地址
		@chvMemo			= @chvMemo,					--说明
		@lngCurrentAmount	= @lngCurrentAmount OUTPUT,	--返回当前银行金币数
		@chvErrMsg			= @chvErrMsg OUTPUT			--错误消息
	
	EXEC [dbo].[PrPs_SendSysMail]
		@intUserID		= @intUserID,			--收件人ID
		@chvTitle		= @chvTitle,			--邮件标题
		@chvContent		= @chvContent,			--邮件内容
		@intSort		= 0,				--排序
		@intMsgID		= NULL		--消息ID

	SET @lngCurrentAmount	= 1
	SET @chvErrMsg			= '成功'
	RETURN 1

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_UpdateQZNN_UserAmount] TO [GameUser]
    AS [dbo];

