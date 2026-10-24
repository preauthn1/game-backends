USE [LogDB]
GO

/****** Object:  StoredProcedure [dbo].[PrGs_Slot_GetGameLog]    Script Date: 2019/12/13 U 06:34:03 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Jason
-- Create date: 2019/05/24
-- Description:	Slot C{
-- =============================================
ALTER PROCEDURE [dbo].[PrGs_Slot_GetGameLog]
(
	@intUserID			INT,
	@intGameID			INT,
	@dtStartTime		DATETIME = NULL,
	@dtEndTime			DATETIME = NULL,
	@intPageIndex		INT	= 1,
	@intPageSize		INT = 20,
	@intPageCount		INT = 0			OUTPUT,
	@chvMsg				VARCHAR(100)	OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON


	DECLARE @StartTime	DATETIME = ISNULL(@dtStartTime,GETDATE()-7)
	DECLARE @EndTime	DATETIME = ISNULL(DATEADD(DAY,1,@dtEndTime),GETDATE()+1)


	SELECT	@intPageCount = COUNT(1)
	FROM [Game].[SlotGameHistoryLog] WITH (NOLOCK)
	WHERE
		[UserID] = @intUserID
		AND [GameID] = @intGameID
		AND [CreateTime] >= @StartTime
		AND [CreateTime] < @EndTime


	;WITH CTE
	AS (
			SELECT	ROW_NUMBER() OVER (ORDER BY [LogID] DESC) AS rowNum,
					[UserID] = @intUserID,
					[CreateTime],
					[OrderID] = [LogID],
					[BetMoney],
					[FinalScore] = ISNULL([FinalScore],0) - ISNULL([CurrentAmount],0)
			FROM [Game].[SlotGameHistoryLog] WITH (NOLOCK)
			WHERE
				[UserID] = @intUserID
				AND [GameID] = @intGameID
				AND [CreateTime] >= @StartTime
				AND [CreateTime] < @EndTime
		)
	SELECT * FROM CTE WHERE rowNum BETWEEN @intPageSize * (@intPageIndex -1) + 1 AND @intPageSize * @intPageIndex
	ORDER BY rowNum ASC

	SET @chvMsg	= ''
	RETURN 1
END
GO


