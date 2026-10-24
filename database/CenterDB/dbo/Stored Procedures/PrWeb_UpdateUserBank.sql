-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<管理员更新银行>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UpdateUserBank]
@intAdminID			INT,
@intUserID			INT,			        --用户ID
@sintSourceType		SMALLINT,				--更新方式
@lngUpdateAmount	BIGINT,					--更新的数量
@chvIPAddress		VARCHAR(15),			--IP地址
@chvMemo			VARCHAR(128),           --说明
@lngCurrentAmount	BIGINT OUTPUT,			--返回当前银行金币数
@chvErrMsg			VARCHAR(128) OUTPUT     --错误消息
AS
BEGIN

	SET NOCOUNT ON
	DECLARE @intResult int=0
	EXEC @intResult=dbo.PrPsWeb_UpdateUserBank
		@intUserID = @intUserID,
		@sintSourceType = @sintSourceType,
		@lngUpdateAmount = @lngUpdateAmount,
		@chvIPAddress = @chvIPAddress,
		@chvMemo = @chvMemo,
		@lngCurrentAmount = @lngCurrentAmount out,
		@chvErrMsg = @chvErrMsg out


	--更新管理員增減金幣紀錄
	IF @lngUpdateAmount <> 0 AND @intResult=1
	BEGIN
		UPDATE Users.UserInfo SET SumAdminMoney=SumAdminMoney+@lngUpdateAmount where UserID=@intUserID
	END

	RETURN @intResult
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UpdateUserBank] TO [GameUser]
    AS [dbo];

