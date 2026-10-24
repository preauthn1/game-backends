-- =============================================
-- Author:  Jason
-- Create date: 2018/04/19
-- Description:	遊戲桌號/場次
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_GetTradeID]
(
	@lngTradeID	BIGINT = 0	OUTPUT
)
AS   
BEGIN   

	SET NOCOUNT ON

	DECLARE @YearMonth	NVARCHAR(6) = CONVERT(VARCHAR(6),GETDATE(),112)
	DECLARE @Temp_ID	TABLE (ID BIGINT)

	INSERT INTO [dbo].[TradeID]
	OUTPUT INSERTED.ID INTO @Temp_ID
	DEFAULT VALUES

	SELECT	@lngTradeID = CAST(@YearMonth + RIGHT(REPLICATE('0',10) + CAST(ID AS NVARCHAR),10) AS BIGINT)
	FROM @Temp_ID

	SET @lngTradeID	= ISNULL(@lngTradeID,0)
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_GetTradeID] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_GetTradeID] TO [xbuser]
    AS [dbo];

