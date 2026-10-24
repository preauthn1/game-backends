-- =============================================
-- Author:		Jason
-- Create date: 2018/04/03
-- Description:	回傳代理交易紀錄 (機制改寫)
-- =============================================
CREATE PROCEDURE [dbo].[PrPsWeb_GetAgentTransferLog]
(
	@intPlatformID	INT = NULL,
	@intAreaID		INT = NULL,
	@intAgentID		INT = NULL,
	@chvAgentIDStr	VARCHAR(MAX) = NULL,
	@intSourceID	INT = NULL,
	@dtStatTime		DATETIME = NULL,
	@dtEndTime		DATETIME = NULL,
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 20,
	@intPageCount	INT = 0		OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @PlatformID		INT = @intPlatformID
	DECLARE @AreaID			INT = @intAreaID
	DECLARE @AgentIDStr		VARCHAR(MAX) = @chvAgentIDStr
	DECLARE @SourceID		INT = @intSourceID
	DECLARE @LogStartTime	DATETIME = @dtStatTime
	DECLARE @LogEndTime		DATETIME = @dtEndTime

	DECLARE @Temp_AgentUserID	TABLE (AgentID INT)

	INSERT INTO @Temp_AgentUserID
	SELECT ID FROM [dbo].[Fn_GetStringToTbl](@chvAgentIDStr)

	/*  取登入紀錄  */
	------------------------------------------------------
	IF EXISTS (SELECT TOP 1 1 FROM @Temp_AgentUserID)
	BEGIN
		SELECT	@intPageCount = COUNT(1)
		FROM [Agent].[AgentTransferLog] AS A WITH (NOLOCK)
			INNER JOIN @Temp_AgentUserID AS B ON A.AgentID = B.AgentID
		WHERE
			(@PlatformID IS NULL OR [PlatformID] = @PlatformID)
			AND (@AreaID IS NULL OR [AreaID] = @AreaID)
			AND (@intSourceID IS NULL OR [SourceID] = @intSourceID)
			AND (@LogStartTime IS NULL OR [CreateTime] >= @LogStartTime)
			AND (@LogEndTime IS NULL OR [CreateTime] < @LogEndTime)

		;WITH CTE AS
		(
			SELECT	ROW_NUMBER() OVER (ORDER BY A.[CreateTime] DESC) AS rowNum,
					A.[Id],
					A.[PlatformID],
					A.[AreaID],
					A.[AgentID],
					A.[SourceID],
					A.[RelationUserID],
					A.[RelationAgentID],
					A.[SelfAmount],
					A.[RelationAmount],
					A.[UpdateAmount],
					A.[IPAddress],
					A.[CreateTime]
			FROM [Agent].[AgentTransferLog] AS A WITH (NOLOCK)
				INNER JOIN @Temp_AgentUserID AS B ON A.AgentID = B.AgentID
			WHERE
				(@PlatformID IS NULL OR [PlatformID] = @PlatformID)
				AND (@AreaID IS NULL OR [AreaID] = @AreaID)
				AND (@intSourceID IS NULL OR [SourceID] = @intSourceID)
				AND (@LogStartTime IS NULL OR [CreateTime] >= @LogStartTime)
				AND (@LogEndTime IS NULL OR [CreateTime] < @LogEndTime)
		)
		SELECT * FROM CTE WHERE rowNum BETWEEN @intPageSize * (@intPageIndex -1) + 1 AND @intPageSize * @intPageIndex
	END
	ELSE
	BEGIN
		SELECT	@intPageCount = COUNT(1)
		FROM [Agent].[AgentTransferLog] AS A WITH (NOLOCK)
		WHERE
			(@PlatformID IS NULL OR [PlatformID] = @PlatformID)
			AND (@AreaID IS NULL OR [AreaID] = @AreaID)
			AND (@intSourceID IS NULL OR [SourceID] = @intSourceID)
			AND (@LogStartTime IS NULL OR [CreateTime] >= @LogStartTime)
			AND (@LogEndTime IS NULL OR [CreateTime] < @LogEndTime)

		;WITH CTE AS
		(
			SELECT	ROW_NUMBER() OVER (ORDER BY A.[CreateTime] DESC) AS rowNum,
					A.[Id],
					A.[PlatformID],
					A.[AreaID],
					A.[AgentID],
					A.[SourceID],
					A.[RelationUserID],
					A.[RelationAgentID],
					A.[SelfAmount],
					A.[RelationAmount],
					A.[UpdateAmount],
					A.[IPAddress],
					A.[CreateTime]
			FROM [Agent].[AgentTransferLog] AS A WITH (NOLOCK)
			WHERE
				(@PlatformID IS NULL OR [PlatformID] = @PlatformID)
				AND (@AreaID IS NULL OR [AreaID] = @AreaID)
				AND (@intSourceID IS NULL OR [SourceID] = @intSourceID)
				AND (@LogStartTime IS NULL OR [CreateTime] >= @LogStartTime)
				AND (@LogEndTime IS NULL OR [CreateTime] < @LogEndTime)
		)
		SELECT * FROM CTE WHERE rowNum BETWEEN @intPageSize * (@intPageIndex -1) + 1 AND @intPageSize * @intPageIndex
	END
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPsWeb_GetAgentTransferLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPsWeb_GetAgentTransferLog] TO [xbuser]
    AS [dbo];

