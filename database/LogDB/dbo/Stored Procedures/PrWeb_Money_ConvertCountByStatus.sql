-- =============================================
-- Author:		Jason
-- Create date: 2018/03/22
-- Description:	回傳兌換記錄數量給後台
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Money_ConvertCountByStatus]
(
	@dtStatTime		DATETIME,		--申请时间
	@tnyStatus		TINYINT			--申请状态
)
AS
BEGIN
	SET NOCOUNT ON

	
	DECLARE @StatTime		DATETIME = @dtStatTime
	DECLARE @Status			DATETIME = @tnyStatus

	
	/*  回傳紀錄  */
	------------------------------------------------------
	SELECT	UserID
	FROM [Money].[ConvertLog] WITH (NOLOCK)
	WHERE
		Status = @Status
		AND CreateTime >= @StatTime
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_ConvertCountByStatus] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_ConvertCountByStatus] TO [xbuser]
    AS [dbo];

