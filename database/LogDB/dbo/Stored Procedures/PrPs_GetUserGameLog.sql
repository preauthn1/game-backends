-- =============================================
-- Author:		Jason
-- Create date: 2018/03/21
-- Description:	回傳使用者遊戲紀錄給Center
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_GetUserGameLog]
(
	@intUserID	INT,
	@dtStatTime DATETIME = NULL,
	@dtEndTime	DATETIME = NULL
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @UserID		INT = @intUserID
	DECLARE @StatTime	DATETIME = @dtStatTime
	DECLARE @EndTime	DATETIME = @dtEndTime

	
	/*  回傳紀錄  */
	------------------------------------------------------
	SELECT	L.ServerID,
			ISNULL(L.WinCount,0) AS WinCount,
			ISNULL(L.LoseCount,0) AS LoseCount,
			ISNULL(L.DrawCount,0) AS DrawCount,
			ISNULL(L.Amount,0) AS Score,
			ISNULL(L.GameTime,0) AS GameTime
	FROM [Game].[GameLog] AS L WITH (NOLOCK)
	WHERE
		(@StatTime IS NULL OR L.LogoutTime >= @StatTime)
		AND (@EndTime IS NULL OR L.LogoutTime <= @EndTime)
		AND UserID = @UserID
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetUserGameLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetUserGameLog] TO [xbuser]
    AS [dbo];

