-- =============================================
-- Author:		Jason
-- Create date: 2018/03/20
-- Description:	回傳金幣紀錄給Center
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_GetMoneyLog]
(
	@dtStatTime			DATETIME,
	@dtEndTime			DATETIME,
	@intSourceTypeID	INT
)
AS
BEGIN
	SET NOCOUNT ON

	
	DECLARE @StatTime		DATETIME = @dtStatTime
	DECLARE @EndTime		DATETIME = @dtEndTime
	DECLARE @SourceTypeID	INT = @intSourceTypeID
	
	/*  回傳紀錄  */
	------------------------------------------------------
	SELECT	UserID,[SourceTypeID],[UpdateAmount]
	FROM [Money].[MoneyLog] WITH (NOLOCK)
	WHERE
		CreateTime >= @StatTime
		AND CreateTime < @EndTime
		AND [SourceTypeID] < @SourceTypeID
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetMoneyLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetMoneyLog] TO [xbuser]
    AS [dbo];

