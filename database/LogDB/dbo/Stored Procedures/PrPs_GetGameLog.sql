-- =============================================
-- Author:		Jason
-- Create date: 2018/03/20
-- Description:	回傳遊戲紀錄給Center
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_GetGameLog]
(
	@dtStatTime DATETIME,
	@dtEndTime	DATETIME
)
AS
BEGIN
	SET NOCOUNT ON

	
	DECLARE @StatTime	DATETIME = @dtStatTime
	DECLARE @EndTime	DATETIME = @dtEndTime

	
	/*  回傳紀錄  */
	------------------------------------------------------
	SELECT	UserID,IPAddress,HDCode
	FROM [Game].[GameLog] WITH (NOLOCK)
	WHERE
		LogoutTime >= @StatTime
		AND LogoutTime < @EndTime
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetGameLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetGameLog] TO [xbuser]
    AS [dbo];

