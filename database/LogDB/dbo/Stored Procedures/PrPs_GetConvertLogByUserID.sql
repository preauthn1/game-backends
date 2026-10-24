-- =============================================
-- Author:		Jason
-- Create date: 2018/03/20
-- Description:	回傳使用者兌換紀錄給Center
--	2018/03/22	Jason		因應調整寫紀錄方式改回傳內容
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_GetConvertLogByUserID]
(
	@intUserID		INT,
	@logInBeginTime DATETIME = NULL, 
	@logInEndTime	DATETIME = NULL,
	@tnyStatus		TINYINT = NULL
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @UserID		INT = @intUserID
	DECLARE @StatTime	DATETIME = @logInBeginTime
	DECLARE @EndTime	DATETIME = @logInEndTime
	DECLARE @Status		TINYINT = @tnyStatus
	
	/*  回傳紀錄  */
	------------------------------------------------------
	SELECT	Amount,
			TaxAmount,
			Status,
			--CASE  WHEN IsNull(UnionpayAccount,'') = '' THEN '支付寶-' + AlipayAccount ELSE UnionpayBankName +'-' + UnionpayAccount END AS Payment,
			CASE  WHEN ConvertType = 1 THEN '支付宝-' + dbo.Fn_HideAccount(Account) ELSE '银联卡-' + BankName +'-' + dbo.Fn_HideAccount(Account) END AS Payment,
			CreateTime
	FROM LogDB.Money.ConvertLog WITH (NOLOCK)
	WHERE
		UserID = @UserID
		AND (@logInBeginTime IS NULL OR CreateTime >= @StatTime)
		AND (@logInEndTime IS NULL OR CreateTime <= @EndTime)
		AND (@Status IS NULL OR Status = @Status)
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetConvertLogByUserID] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetConvertLogByUserID] TO [xbuser]
    AS [dbo];

