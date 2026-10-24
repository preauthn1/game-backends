-- =============================================
-- Author:		Jason
-- Create date: 2018/03/22
-- Description:	更新使用者兌換紀錄
--	2018/04/12	Jason		取消訂單狀態錯誤判斷
--	2018/04/24	Jason		新增@chvUserName參數及寫入[Operator] = @chvUserName
--	2018/05/01	alan		更改sp名稱 PrPs_UPDConvertLog -> PrWeb_Money_FinishedConvertLog
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Money_FinishedConvertLog]
(
	@intId				INT = NULL,
	@lngRealTaxAmount	BIGINT = NULL,
	@tnyStatus			TINYINT = NULL,
	@chvTransID			VARCHAR(64) = NULL,
	@chvTransData		VARCHAR(MAX) = NULL,
	@dtUpdateTime			DATETIME = NULL,
	@chvUserName		VARCHAR(16),
	@intUserID			INT = NULL			OUTPUT,
	@lngTaxAmount		BIGINT = NULL		OUTPUT,
	@lngConvertAmount	BIGINT = NULL		OUTPUT,
	@lngReturnAmount	BIGINT = NULL		OUTPUT,
	@tnyConvertType		BIGINT = NULL		OUTPUT,
	@chvAccount			VARCHAR(64) = NULL	OUTPUT,
	@intRtnCode			INT					OUTPUT,
	@chvRtnMsg			VARCHAR(64) 		OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	
	DECLARE @ID					INT = @intId
	
	SET @intRtnCode			=	0
	SET @chvRtnMsg			=	'無操作行為'
	SET @lngRealTaxAmount	=	ISNULL(@lngRealTaxAmount,0)
	SET @dtUpdateTime			=	ISNULL(@dtUpdateTime,GETDATE())

	IF NOT EXISTS (SELECT TOP 1 1 FROM [Money].[ConvertLog] WITH (NOLOCK) WHERE Id = @ID)
	BEGIN
		SET @intRtnCode = -1
		SET @chvRtnMsg = '訂單不存在'
		RETURN
	END

	IF EXISTS(SELECT 1 FROM [Money].[ConvertLog] WITH (NOLOCK) WHERE Id = @intId AND Status not in(2,99,100))
	BEGIN
		SET @intRtnCode = -2
		SET @chvRtnMsg = '操作失败:必须审核通过才可打款'
		RETURN
	END
	
	/*  更新兌換申請紀錄  */
	------------------------------------------------------
	DECLARE @Temp_UserConvertLog	TABLE
		(UserID INT,TaxAmount BIGINT,Amount BIGINT,ConvertType TINYINT,Account VARCHAR(64))
	
	UPDATE [Money].[ConvertLog]
	SET	Status = @tnyStatus,
		RealTaxAmount = ISNULL(@lngRealTaxAmount,RealTaxAmount),
		TransID = ISNULL(@chvTransID,TransID),
		TransData = ISNULL(TransData,'') + ISNULL(@chvTransData,''),
		LastUpdateTime = @dtUpdateTime,
		[Operator] = @chvUserName
	OUTPUT INSERTED.UserID,INSERTED.TaxAmount,INSERTED.Amount,INSERTED.ConvertType,INSERTED.Account
		INTO @Temp_UserConvertLog
	WHERE
		Id = @ID
		AND Status IN (2,99,100)

    IF @@ROWCOUNT = 0
	BEGIN
		SET @intRtnCode = -3
		SET @chvRtnMsg = '訂單更新失敗'
		RETURN
	END


	SELECT	@intUserID = UserID,
			@lngTaxAmount = TaxAmount,
			@lngConvertAmount = ABS(Amount),
			@lngReturnAmount = ABS(Amount) + TaxAmount,
			@tnyConvertType = ConvertType,
			@chvAccount = CASE WHEN ConvertType = 1 THEN '支付寶-' + Account ELSE Account END
	FROM @Temp_UserConvertLog

	SET @intRtnCode = 1
	SET @chvRtnMsg = '訂單更新完成'
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_FinishedConvertLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_FinishedConvertLog] TO [xbuser]
    AS [dbo];

