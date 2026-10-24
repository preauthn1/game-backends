-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<获取AreaGame信息>
--	2018/04/18	Jason		調整為回傳分頁資訊
--	2018/06/20	Emily		#494 遊戲管理頁面調整, 只需平台對應遊戲 -取消Join Area.Area
--	2018/7/12 emily	#583 后台新增广告与游戏热门程度管理 -- 新增欄位Popular
--	2018/11/29 alan	增加areaid欄位
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetGetAreaGame]
(
	@intPlatformID	INT = NULL,
	@intAreaID		INT = NULL,
	@intPageIndex	INT = 1,
	@intPageSize	INT	= 20,
	@intPageCount	INT			OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON


	SELECT	@intPageCount = COUNT(1)
	FROM [Area].[GameList] AS G WITH (NOLOCK)
	WHERE
		ISNULL(@intPlatformID,G.PlatformID) = G.PlatformID
		AND ISNULL(@intAreaID,G.AreaID) = G.AreaID


	;WITH CTE AS
	(
		SELECT	ROW_NUMBER() OVER (ORDER BY G.PlatformID, G.GameID) AS rowNum,
				G.Id,
				G.PlatformID,
				P.Name AS PlatformName,
				G.GameID,
				G.Sort,
				G.Status,
				G.Popular,
				A.IconID,
				A.IconName,
				A.AliasName,
				A.Skin
		FROM [Area].[GameList] AS G WITH (NOLOCK)
			LEFT JOIN [CenterDB].[Area].[Platform] AS P WITH (NOLOCK) on G.PlatformID = P.PlatformID
			LEFT JOIN [CenterDB].[Area].[GameAliasName] AS A WITH (NOLOCK) on G.IconID = A.IconID
			
		WHERE
			ISNULL(@intPlatformID,G.PlatformID) = G.PlatformID
			AND ISNULL(@intAreaID,G.AreaID) = G.AreaID
	)
	SELECT * FROM CTE WHERE rowNum BETWEEN @intPageSize * (@intPageIndex -1) + 1 AND @intPageSize * @intPageIndex
END




GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetGetAreaGame] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetGetAreaGame] TO [xbuser]
    AS [dbo];

