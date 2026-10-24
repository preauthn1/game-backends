-- =============================================
-- Author:		CCC
-- Create date: 2019/07/16
-- Description:	<修改兑换备注>
-- =============================================
CREATE PROCEDURE PrWeb_Money_UpdateConvertLogMemo
	@intId				INT,	--ID
	@chvUserName		VARCHAR(16),
	@chvMemo			VARCHAR(200),	
	@chvErrMsg			VARCHAR(64)		OUTPUT
AS
BEGIN

	IF NOT EXISTS (SELECT TOP 1 1 FROM [Money].[ConvertLog] WITH (NOLOCK) WHERE Id = @intId)
	BEGIN
		SET @chvErrMsg = '訂單不存在'
		RETURN - 1
	END

	UPDATE [Money].[ConvertLog]
		SET [Operator] = @chvUserName,			
			LastUpdateTime = GETDATE(),
			Memo = @chvMemo
		WHERE
			Id = @intId

	IF @@ROWCOUNT=0
	BEGIN		
		SET @chvErrMsg = '訂單更新失敗'
		RETURN -1
	END
	
	SET @chvErrMsg = '訂單更新完成'
	RETURN 1
END