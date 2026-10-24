-- =============================================
-- Author:  Jason
-- Create date: 2018/04/20
-- Description:	機器人列表
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetRobotSetting]
(
	@intGameID		INT	= NULL,	--遊戲編號
	@intServerID	INT = NULL,	--伺服器編號
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 20,
	@intPageCount	INT = 0		OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @ServerID		INT = @intServerID
	DECLARE @GameID			INT = @intGameID
	DECLARE @Status			TINYINT = 1	--1:開啟 0:關閉/刪除

	
	/*  回傳功能列表  */
	----------------------------------------------------------------------
	SELECT	@intPageCount = COUNT(1)
	FROM [Robot].[ServerConfig] AS A WITH (NOLOCK)
		INNER JOIN [ServerInfo].[GameServerList] AS B WITH (NOLOCK) ON A.[ServerID] = B.[ServerID]
		INNER JOIN [ServerInfo].[GameList] AS C WITH (NOLOCK) ON B.[GameID] = C.[GameID]
	WHERE
		ISNULL(@ServerID,A.[ServerID]) = A.[ServerID]
		AND ISNULL(@GameID,B.[GameID]) = B.[GameID]
		AND A.[Status] = @Status

	;WITH CTE AS
	(
		SELECT	ROW_NUMBER() OVER (ORDER BY ID DESC) AS rowNum,
				C.[GameID],
				C.[GameName],
				A.[ServerID],
				B.[ServerName],
				A.[BeginTime],
				A.[EndTime],
				A.[MinCount],
				A.[MaxCount],
				A.[MinLevel],
				A.[MaxLevel],
				A.[Status]
		FROM [Robot].[ServerConfig] AS A WITH (NOLOCK)
			INNER JOIN [ServerInfo].[GameServerList] AS B WITH (NOLOCK) ON A.[ServerID] = B.[ServerID]
			INNER JOIN [ServerInfo].[GameList] AS C WITH (NOLOCK) ON B.[GameID] = C.[GameID]
		WHERE
			ISNULL(@ServerID,A.[ServerID]) = A.[ServerID]
			AND ISNULL(@GameID,B.[GameID]) = B.[GameID]
			AND A.[Status] = @Status
	)
	SELECT * FROM CTE WHERE rowNum BETWEEN @intPageSize * (@intPageIndex -1) + 1 AND @intPageSize * @intPageIndex
	----------------------------------------------------------------------

	RETURN 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetRobotSetting] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetRobotSetting] TO [xbuser]
    AS [dbo];

