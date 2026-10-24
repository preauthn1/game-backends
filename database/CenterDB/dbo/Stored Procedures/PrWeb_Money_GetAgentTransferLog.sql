-- =============================================
-- Author:  Jason
-- Create date: 2018/04/03
-- Description:	代理管理-代理交易查詢-交易明細
--	2018/04/18	Jason		優化查詢及刪除不必要JOIN
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Money_GetAgentTransferLog]
(
	@intPlatformID	INT = NULL,
	@intAreaID		INT = NULL,
	@intAgentID		INT = NULL,
	@chvAgentName	VARCHAR(32) = NULL,
	@intSourceID	INT = NULL,
	@dtStatTime		DATETIME = NULL,
	@dtEndTime		DATETIME = NULL,
	@intPageIndex	INT = 1,
	@intPageSize	INT = 20,
	@intPageCount	INT = 0		OUTPUT
)
AS
BEGIN
SET NOCOUNT ON

	DECLARE @PlatformID		INT = @intPlatformID
	DECLARE @AreaID			INT = @intAreaID
	DECLARE @LogStatTime	DATETIME = @dtStatTime
	DECLARE @LogEndTijme	DATETIME = @dtEndTime
	DECLARE @chvAgentIDStr	VARCHAR(MAX)


	DECLARE @Temp_AgentTransferLog TABLE
		(
			[rowNum]			INT,
			[ID]				INT,
			[PlatformID]		INT,
			[AreaID]			INT,
			[AgentID]			INT,
			[SourceID]			INT,
			[RelationUserID]	INT,
			[RelationAgentID]	INT,
			[SelfAmount]		BIGINT,
			[RelationAmount]	BIGINT,
			[UpdateAmount]		BIGINT,
			[IPAddress]			VARCHAR(15),
			[CreateTime]		DATETIME
		)

	DECLARE @Temp_PlatformIDArea	TABLE
		(
			PlatformID		INT,
			AreaID			INT,
			PlatformName	VARCHAR(32),
			AreaName		VARCHAR(32)
		)

	/*  取資料暫存  */
	-------------------------------------------------------------------------------------------
	SELECT @chvAgentIDStr = AgentString
	FROM (
		SELECT DISTINCT STUFF((	SELECT ',' + CAST(A.[AgentID] AS VARCHAR(20))
								FROM [Agent].[Users] AS A
								WHERE
									([Name] LIKE '%' + @chvAgentName + '%')
								FOR XML PATH('')
							),1,1,'') AS AgentString
		FROM [Agent].[Users] B
		) AS A
	

	INSERT INTO @Temp_AgentTransferLog
	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrPsWeb_GetAgentTransferLog]
		@intPlatformID	= @intPlatformID,
		@intAreaID		= @intAreaID,
		@intAgentID		= @intAgentID,
		@chvAgentIDStr	= @chvAgentIDStr,
		@intSourceID	= @intSourceID,
		@dtStatTime		= @dtStatTime,
		@dtEndTime		= @dtEndTime,
		@intPageIndex	= @intPageIndex,
		@intPageSize	= @intPageSize,
		@intPageCount	= @intPageCount		OUTPUT

	INSERT INTO @Temp_PlatformIDArea
	SELECT	P.PlatformID,
			A.AreaID,
			[PlatformName] = P.Name,
			A.AreaName
	FROM [Area].[Platform] AS P WITH (NOLOCK)
		INNER JOIN [Area].[Area] AS A WITH (NOLOCK) ON P.PlatformID = A.PlatformID
	-------------------------------------------------------------------------------------------


	/*  回傳資料  */
	-------------------------------------------------------------------------------------------
	SELECT	DE.PlatformID,
			[PlatformName] = DE.[PlatformName],
			A.AreaID,
			DE.AreaName,
			A.[CreateTime],
			AgentID = A.[Id],
			AgentUserID = C.[UserID],
			AgentName = B.Name,
			F.[SourceID],
			F.SourceName,
			TargetAgentID = A.[RelationAgentID],
			TargetUserID = A.[RelationUserID],
			TargetName = CASE	WHEN A.[RelationAgentID] IS NULL THEN '玩家: ' + CAST(A.[RelationUserID] AS VARCHAR(20))
								ELSE '代理: ' + CAST(J.[Name] AS VARCHAR(20)) END,
			A.SelfAmount,
			A.RelationAmount,
			A.UpdateAmount,
			A.IPAddress
	FROM @Temp_AgentTransferLog AS A
		INNER JOIN [Agent].[Users] AS B ON A.[AgentID] = B.[AgentID]
		INNER JOIN [Users].[UserInfo] AS C ON B.[UserID] = C.[UserID]
		LEFT JOIN @Temp_PlatformIDArea AS DE ON C.[AreaID] = DE.[AreaID] AND B.[PlatformID] = DE.[PlatformID]
		INNER JOIN [Agent].[TransferType] AS F ON A.[SourceID] = F.[SourceID]
		LEFT OUTER JOIN [Agent].[Users] AS J ON A.[RelationAgentID] = J.[AgentID]
	ORDER BY [rowNum] ASC
	-------------------------------------------------------------------------------------------

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_GetAgentTransferLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_GetAgentTransferLog] TO [xbuser]
    AS [dbo];

