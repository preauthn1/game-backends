-- =============================================
-- Author:		Jason
-- Create date: 2018/03/20
-- Description:	回傳兌換紀錄給Center
--	2018/03/22	Jason		新增訂單編號條件
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_GetConvertLog]
(
	@intId		INT = NULL,
	@dtStatTime DATETIME,
	@dtEndTime	DATETIME,
	@tnyStatus	TINYINT = NULL
)
AS
BEGIN
	SET NOCOUNT ON

	
	DECLARE @StatTime	DATETIME = @dtStatTime
	DECLARE @EndTime	DATETIME = @dtEndTime
	DECLARE @Status		TINYINT = @tnyStatus
	
	/*  回傳紀錄  */
	------------------------------------------------------
	SELECT	UserID,CreateTime,[Status],Amount,TaxAmount
	FROM [Money].[ConvertLog] WITH (NOLOCK)
	WHERE
		CreateTime >= @StatTime
		AND CreateTime < @EndTime
		AND (@Status IS NULL OR [Status] = @Status)
		AND (@intId IS NULL OR @intId = [Id])
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetConvertLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetConvertLog] TO [xbuser]
    AS [dbo];

