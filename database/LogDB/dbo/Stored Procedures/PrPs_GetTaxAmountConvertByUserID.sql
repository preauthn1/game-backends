-- =============================================
-- Author:		Jason
-- Create date: 2018/03/20
-- Description:	回傳遊戲稅收及兌換紀錄給Center
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_GetTaxAmountConvertByUserID]
(
	@intUserID			INT,
	@dtStatTime			DATETIME,
	@dtEndTime			DATETIME,
	@bintTaxAmount		BIGINT = 0		OUTPUT,
	@bintConvertAmount	BIGINT = 0		OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @UserID		INT = @intUserID
	DECLARE @StatTime	DATETIME = @dtStatTime
	DECLARE @EndTime	DATETIME = @dtEndTime
	
	/*  回傳紀錄  */
	------------------------------------------------------
	SELECT	@bintTaxAmount = ISNULL(SUM(TaxAmount),0)
	FROM [Game].[GameLog] WITH (NOLOCK)
	WHERE
		LogoutTime BETWEEN @StatTime AND @EndTime
		AND UserID = @UserID

	SELECT @bintConvertAmount = ISNULL(SUM(Amount) ,0)
	FROM [Money].[ConvertLog] WITH (NOLOCK)
	WHERE
		CreateTime BETWEEN @StatTime AND @EndTime
		AND UserID = @UserID
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetTaxAmountConvertByUserID] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetTaxAmountConvertByUserID] TO [xbuser]
    AS [dbo];

