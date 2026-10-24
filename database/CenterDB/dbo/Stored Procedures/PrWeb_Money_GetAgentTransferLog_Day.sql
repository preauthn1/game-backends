-- =============================================
-- Author:  Jason
-- Create date: 2018/04/09
-- Description:	代理管理-代理交易查詢-交易日報表
--	2018/04/18	Jason		優化查詢
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Money_GetAgentTransferLog_Day]
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
			[LogID]				INT,
			[StatTime]			DATETIME,
			[PlatformID]		INT,
			[AreaID]			INT,
			[AgentID]			INT,
			[SourceID]			INT,
			[Amount]			BIGINT,
			[TargetCounts]		INT
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
	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrPsWeb_GetAgentTransferLog_Day]
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


	/*  更新資料  */
	-------------------------------------------------------------------------------------------
	SELECT	A.[StatTime],
			A.PlatformID,
			[PlatformName] = ISNULL(PA.[PlatformName],'全部平台'),
			A.AreaID,
			AreaName = ISNULL(PA.AreaName,'全部渠道'),
			AgentID = A.[AgentID],
			AgentUserID = ISNULL(E.[UserID],0),
			AgentName = ISNULL(D.Name,'全部代理'),
			[SourceID] = ISNULL(F.[SourceID],0),
			SourceName = ISNULL(F.SourceName,'全部流向'),
			A.[Amount],
			A.[TargetCounts]
	FROM @Temp_AgentTransferLog AS A
		LEFT JOIN @Temp_PlatformIDArea AS PA ON A.PlatformID = PA.PlatformID AND A.AreaID = PA.AreaID
		LEFT JOIN [Agent].[Users] AS D WITH (NOLOCK) ON A.AgentID = D.AgentID
		LEFT JOIN [Users].[UserInfo] AS E WITH (NOLOCK) ON D.UserID = E.UserID
		LEFT JOIN [Agent].[TransferType] AS F WITH (NOLOCK) ON A.SourceID = F.SourceID
	WHERE
		A.[AgentID] > 0 
	ORDER BY [rowNum] ASC
	-------------------------------------------------------------------------------------------

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_GetAgentTransferLog_Day] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_GetAgentTransferLog_Day] TO [xbuser]
    AS [dbo];

