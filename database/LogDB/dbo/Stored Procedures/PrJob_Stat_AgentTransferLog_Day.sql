-- =============================================
-- Author:		Jason
-- Create date: 2018/04/03
-- Description:	統計代理交易日報表 (機制改寫)
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_Stat_AgentTransferLog_Day]
(
	@dtStatTime		DATETIME = NULL
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @LogStartTime	DATETIME = DATEADD(DAY,-1,DATEDIFF(DAY,0,ISNULL(@dtStatTime,GETDATE())))
	DECLARE @LogEndTime		DATETIME = DATEADD(DAY,0,DATEDIFF(DAY,0,ISNULL(@dtStatTime,GETDATE())))

	DECLARE @Temp_SourceID	TABLE	(SourceID INT)
	DECLARE @Temp_AgentID	TABLE (AgentID INT)
	DECLARE @Temp_PlatformArea	TABLE (PlatformID INT,AreaID INT)
	DECLARE @Temp_DatesAndAgents	TABLE (StatTime DATETIME,PlatFormID INT,AreaID INT,AgentID INT,[SourceID] INT)


	/*  取登入紀錄  */
	------------------------------------------------------
	INSERT INTO @Temp_PlatformArea
	SELECT	DISTINCT PlatFormID,AreaID FROM [Agent].[AgentTransferLog] WITH (NOLOCK)

	INSERT INTO @Temp_SourceID
	SELECT DISTINCT [SourceID] FROM [Agent].[AgentTransferLog] WITH (NOLOCK)

	INSERT INTO @Temp_AgentID
	SELECT DISTINCT AgentID FROM [Agent].[AgentTransferLog] WITH (NOLOCK)

	INSERT INTO @Temp_DatesAndAgents
	SELECT	DISTINCT @LogStartTime,
			ISNULL([PlatformID],0),
			ISNULL([AreaID],0),
			ISNULL([AgentID],0),
			ISNULL([SourceID],0)
	FROM (
	SELECT A.PlatFormID,A.AreaID,B.AgentID,C.[SourceID]
	FROM @Temp_PlatformArea AS A
		LEFT JOIN @Temp_AgentID AS B ON 1=1
		LEFT JOIN @Temp_SourceID AS C ON 1=1
		) AS A
	GROUP BY GROUPING SETS
		(
			ROLLUP(a.PlatFormID),
			ROLLUP(a.PlatFormID,A.AreaID),
			ROLLUP(a.PlatFormID,A.AreaID,a.AgentID),
			ROLLUP(a.PlatFormID,A.AreaID,a.AgentID,a.SourceID),
			ROLLUP(A.AreaID),
			ROLLUP(A.AreaID,a.AgentID),
			ROLLUP(A.AreaID,a.AgentID,a.SourceID),
			ROLLUP(a.AgentID),
			ROLLUP(a.AgentID,a.SourceID),
			ROLLUP(a.SourceID)
		)
			
	SELECT	StatTime,
			PlatformID,
			AreaID,
			AgentID,
			SourceID,
			MAX(targetcounts) AS targetcounts,
			SUM(amount) AS amount
	INTO #Temp_StatisticsAgentTransfer_Day
	FROM (
			SELECT  
					DISTINCT
					ISNULL(A.StatTime,@LogStartTime) AS StatTime,
					ISNULL(A.PlatformID,0) AS PlatformID,
					ISNULL(A.AreaID,0) AS AreaID,
					ISNULL(A.AgentID,0) AS AgentID,
					ISNULL(A.SourceID,0) AS SourceID,
					COUNT(DISTINCT b.targetcounts) AS targetcounts, sum(ISNULL(b.amount,0)) AS amount
			FROM @Temp_DatesAndAgents AS A
				LEFT JOIN (
							SELECT	[PlatformID],[AreaID],[AgentID],[SourceID],[RelationUserID] as targetcounts,[UpdateAmount] as amount
							FROM [LogDB].[Agent].[AgentTransferLog] WITH (NOLOCK)
							WHERE
								[CreateTime] >= @LogStartTime
								AND [CreateTime] < @LogEndTime
						) AS B ON A.[PlatformID] = B.[PlatformID] AND A.[AreaID] = B.[AreaID] AND A.[AgentID] = B.[AgentID] AND A.[SourceID] = B.[SourceID]
			GROUP BY CUBE (A.StatTime,a.PlatFormID,A.AreaID,a.AgentID,a.SourceID)
		) AS a
	GROUP BY StatTime,PlatformID,AreaID,AgentID,SourceID

	MERGE [Stat].[StatisticsAgentTransfer_Day] AS A
	USING (
			SELECT	StatTime,PlatformID,AreaID,AgentID,SourceID,Amount,TargetCounts
			FROM #Temp_StatisticsAgentTransfer_Day
		) AS B ON A.[StatTime] = B.[StatTime] AND A.[PlatformID] = B.[PlatformID] AND A.[AreaID] = B.[AreaID] AND A.[AgentID] = B.[AgentID] AND A.[SourceID] = B.[SourceID]
	WHEN MATCHED THEN
		UPDATE SET	A.[Amount] = B.[Amount],
					A.[TargetCounts] = B.[TargetCounts]
    WHEN NOT MATCHED THEN
		INSERT
			([StatTime],[PlatformID],[AreaID],[SourceID],[AgentID],[Amount],[TargetCounts])
		VALUES
			([StatTime],[PlatformID],[AreaID],[SourceID],[AgentID],[Amount],[TargetCounts]);
	------------------------------------------------------

	DROP TABLE #Temp_StatisticsAgentTransfer_Day
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_Stat_AgentTransferLog_Day] TO [xbuser]
    AS [dbo];

