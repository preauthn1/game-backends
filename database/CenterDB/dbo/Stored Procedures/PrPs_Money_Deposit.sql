-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<大厅存取款>
-- Changelog:
--		2018/04/09	alan 修改log記錄方式. CAST(@lngCurrentBankAmount  AS varchar) -> dbo.Fn_ShowMoney(@lngCurrentBankAmount)
--		2018/04/18	alan add transaction
--		2018/05/08	Jason	增加取款時銀行存款判斷
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Money_Deposit]
@intUserID				INT,					--用户ID
@tintOpType				TINYINT,				--操作类型 0存款 1取款
@chvBankPwd				varchar(32) = NULL,			--银行取款密码
@lngAmount				BIGINT,					--修改数量
@chvIPAddress			VARCHAR(15),			--IP地址
@lngCurrentCashAmount 	BIGINT OUTPUT,			--当前现金数
@lngCurrentBankAmount 	BIGINT OUTPUT,			--当前银行数
@chvErrMsg				VARCHAR(128) OUTPUT		--返回消息
AS
SET NOCOUNT ON

set xact_abort on

SET @chvBankPwd = [REDACTED], '')

DECLARE @intReturnValue int,@chvMemo VARCHAR(128),@chvCashMemo VARCHAR(128)
SET @chvMemo = '客户端存取款'

--获取当前银行金额
SELECT @lngCurrentBankAmount = Amount FROM Money.Bank WHERE UserID = @intUserID
SET @lngCurrentBankAmount = ISNULL(@lngCurrentBankAmount, 0)

IF (@lngAmount <= 0) OR ((@lngCurrentBankAmount < ABS(@lngAmount)) AND @tintOpType = 1)
BEGIN
	SET @chvErrMsg = '存款或取款金額錯誤'
	return -1
END


BEGIN TRAN

--存款操作
IF @tintOpType = 0
BEGIN
	--PRINT '存款'
	SET @lngAmount = ABS(@lngAmount) * -1
	
	SET @lngCurrentBankAmount = @lngCurrentBankAmount +  ABS(@lngAmount)
	SET @chvCashMemo = ISNULL(@chvMemo,'') + ',当前银行:' + dbo.Fn_ShowMoney(@lngCurrentBankAmount)


	EXEC @intReturnValue = dbo.PrWeb_UpdateUserCash 
		@intUserID,
		12000,		--存款操作
		@lngAmount,
		@chvIPAddress,
		@chvCashMemo,
		NULL,
		@chvErrMsg OUTPUT
	IF @intReturnValue=1
	BEGIN
		SET @lngAmount = ABS(@lngAmount)
		SET @intReturnValue = 0
		EXEC @intReturnvalue = dbo.PrPsWeb_UpdateUserBank
			@intUserID,
			12000,		--存款操作
			@lngAmount,
			@chvIPAddress,
			@chvMemo,
			NULL,
			@chvErrMsg OUTPUT
	END				
END
--取款操作
ELSE IF @tintOpType = 1
BEGIN	
	IF NOT EXISTS(select 1 from Users.UserInfo WHERE UserID= @intUserID AND BankPassword = [REDACTED]
	BEGIN
		SET @chvErrMsg = '银行取款密码错误'
		ROLLBACK TRAN
		RETURN -1
	END
	ELSE
	BEGIN
		SET @lngAmount = ABS(@lngAmount) * -1
	
		SET @lngCurrentBankAmount = @lngCurrentBankAmount - ABS(@lngAmount)
		SET @chvCashMemo = ISNULL(@chvMemo,'') + ',当前银行:' + dbo.Fn_ShowMoney(@lngCurrentBankAmount)
		
		SET @intReturnValue = 0
		EXEC @intReturnvalue = dbo.PrPsWeb_UpdateUserBank
			@intUserID,
			12001,		--取款操作
			@lngAmount,
			@chvIPAddress,
			@chvMemo,
			NULL,
			@chvErrMsg OUTPUT
		
		IF @intReturnValue = 1
		BEGIN
			--PRINT '取款'
			SET @lngAmount = ABS(@lngAmount)	
			EXEC @intReturnValue = dbo.PrWeb_UpdateUserCash 
				@intUserID,
				12001,		--取款操作
				@lngAmount,
				@chvIPAddress,
				@chvCashMemo,
				NULL,
				@chvErrMsg OUTPUT				
		END
	END	
END
ELSE 
BEGIN
	SET @chvErrMsg = '操作失败:操作类型错误'
	ROLLBACK TRAN
	return -1
END

		
IF @intReturnValue=1
BEGIN
	COMMIT TRAN
	SET @chvErrMsg = '操作成功'
END
ELSE
BEGIN
	ROLLBACK TRAN
END


SELECT @lngCurrentCashAmount = Amount FROM Money.Cash WHERE userId = @intUserID
SELECT @lngCurrentBankAmount = Amount FROM Money.Bank WHERE userId = @intUserID
SET @lngCurrentCashAmount = ISNULL(@lngCurrentCashAmount, 0)
SET @lngCurrentBankAmount = ISNULL(@lngCurrentBankAmount, 0)
RETURN @intReturnValue

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Money_Deposit] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Money_Deposit] TO [xbuser]
    AS [dbo];

