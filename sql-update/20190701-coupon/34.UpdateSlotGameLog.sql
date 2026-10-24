
-- Update Table
ALTER TABLE [LogDB].[Game].[SlotGameHistoryLog] ADD [CurrentCoupon] bigint NOT NULL DEFAULT 0 WITH VALUES
ALTER TABLE [LogDB].[Game].[SlotGameHistoryLog] ADD [BetCoupon] bigint NOT NULL DEFAULT 0 WITH VALUES
ALTER TABLE [LogDB].[Game].[SlotGameHistoryLog] ADD [FinalCoupon] bigint NOT NULL DEFAULT 0 WITH VALUES

-- Update SP [CenterDB][dbo].[PrGs_Slot_AddGameLog]
GO
USE [CenterDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jason
-- Create date: 2019/05/24
-- Description:	Slot 寫入遊戲歷程
-- =============================================
ALTER PROCEDURE [dbo].[PrGs_Slot_AddGameLog]
(
	@intUserID			INT,
	@intGameID			INT,
	@intServerID		INT,
	@lngCurrentAmount	BIGINT,
	@lngCurrentCoupon	BIGINT,
	@lngBetMoney		BIGINT,
	@lngBetCoupon		BIGINT,
	@lngWinMoney		BIGINT,
	@lngFinalScore		BIGINT,
	@lngFinalCoupon		BIGINT,
	@intGameType		TINYINT,
	@intLogType			TINYINT,
	@chvGameData		VARCHAR(MAX),
	@chvMsg				VARCHAR(100)	OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON


	IF NOT EXISTS (SELECT TOP 1 1 FROM [Users].[UserInfo] WITH (NOLOCK) WHERE UserID = @intUserID)
	BEGIN
		SET @chvMsg = '操作錯誤 : 用戶不存在'
		RETURN -1
	END

	IF NOT EXISTS (SELECT TOP 1 1 FROM [ServerInfo].[GameServerList] WHERE GameID = @intGameID AND ServerID = @intServerID)
	BEGIN
		SET @chvMsg = '操作錯誤 : 服務器不存在'
		RETURN -1
	END


	/*  取用戶對應平台  */
	DECLARE @PlatformID INT
	DECLARE @AreaID		INT

	SELECT	@PlatformID = B.PlatformID,
			@AreaID = A.AreaID
	FROM [Users].[UserInfo] AS A WITH (NOLOCK)
		INNER JOIN [Area].[Area] AS B WITH (NOLOCK) ON A.AreaID = B.AreaID
	WHERE
		A.UserID = @intUserID


	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrGs_Slot_AddGameLog]
		@intUserID = @intUserID,
		@intPlatformID = @PlatformID,
		@intAreaID = @AreaID,
		@intGameID = @intGameID,
		@intServerID = @intServerID,
		@lngCurrentAmount = @lngCurrentAmount,
		@lngCurrentCoupon = @lngCurrentCoupon,
		@lngBetMoney = @lngBetMoney,
		@lngBetCoupon = @lngBetCoupon,
		@lngWinMoney = @lngWinMoney,
		@lngFinalScore = @lngFinalScore,
		@lngFinalCoupon = @lngFinalCoupon,
		@intGameType = @intGameType,
		@intLogType = @intLogType,
		@chvGameData = @chvGameData

	SET @chvMsg = '操作完成'
	RETURN 1
END
GO

-- Update SP [LogDB][dbo].[PrGs_Slot_GetGameLog]
USE [LogDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jason
-- Create date: 2019/05/24
-- Description:	Slot 取遊戲歷程
-- =============================================
-- =============================================
-- Author:		Jason
-- Create date: 2019/05/24
-- Description:	Slot 取遊戲歷程
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
			SELECT	ROW_NUMBER() OVER (ORDER BY [LogID] ASC) AS rowNum,
					[UserID] = @intUserID,
					[CreateTime],
					[OrderID] = [LogID],
					[BetMoney] = ISNULL([BetMoney],0) + ISNULL([BetCoupon],0),
					[FinalScore] = (ISNULL([FinalScore],0) - ISNULL([CurrentAmount],0)) + (ISNULL([FinalCoupon],0) - ISNULL([CurrentCoupon],0))
			FROM [Game].[SlotGameHistoryLog] WITH (NOLOCK)
			WHERE
				[UserID] = @intUserID
				AND [GameID] = @intGameID
				AND [CreateTime] >= @StartTime
				AND [CreateTime] < @EndTime
		)
	SELECT * FROM CTE WHERE rowNum BETWEEN @intPageSize * (@intPageIndex -1) + 1 AND @intPageSize * @intPageIndex
	ORDER BY rowNum ASC

	SET @chvMsg	= '完成'
	RETURN 1
END
GO


-- Update SP [LogDB][dbo].[PrGs_Slot_AddGameLog]

USE [LogDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jason
-- Create date: 2019/05/24
-- Description:	給CenterDB SP呼叫用 寫入遊戲歷程
-- 2019/06/20 修改 logid 格式YMDHM 加流水號共16碼
-- =============================================
ALTER PROCEDURE [dbo].[PrGs_Slot_AddGameLog]
(
	@intUserID			INT,
	@intPlatformID		INT,
	@intAreaID			INT,
	@intGameID			INT,
	@intServerID		INT,
	@lngCurrentAmount	BIGINT,
	@lngCurrentCoupon	BIGINT,
	@lngBetMoney		BIGINT,
	@lngBetCoupon		BIGINT,
	@lngWinMoney		BIGINT,
	@lngFinalScore		BIGINT,
	@lngFinalCoupon		BIGINT,
	@intGameType		TINYINT,
	@intLogType			TINYINT,
	@chvGameData		VARCHAR(MAX)
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @NowTime	DATETIME = GETDATE()

	/*  取號  */
	DECLARE @TempLogID	INT
	DECLARE @LogID	BIGINT
	DECLARE @YMD	VARCHAR(6)
	DECLARE @HM	    VARCHAR(4)

	EXEC [dbo].[PrWeb_Slot_GetSequence]
		@intLogID = @TempLogID OUTPUT

	SET @YMD = RIGHT(CONVERT(VARCHAR(10),@NowTime,112),6)
	SET @HM = LEFT(Replace(Convert(varchar(8),@NowTime,108),':',''), 4)

	SELECT	@LogID = CAST(@YMD + @HM + RIGHT(REPLICATE('0',16) + CAST(@TempLogID AS NVARCHAR),6) AS BIGINT)


	INSERT INTO [Game].[SlotGameHistoryLog]
		(	
			[LogID],[CreateTime],[PlatformID],[AreaID],[GameID],[ServerID],[UserID],
			[CurrentAmount],[CurrentCoupon],[BetMoney],[BetCoupon],[WinMoney],[FinalScore],[FinalCoupon],[GameType],[LogType],[GameData]
		)
	VALUES
		(
			@LogID,@NowTime,@intPlatformID,@intAreaID,@intGameID,@intServerID,@intUserID,
			@lngCurrentAmount,@lngCurrentCoupon,@lngBetMoney,@lngBetCoupon,@lngWinMoney,@lngFinalScore,@lngFinalCoupon,@intGameType,@intLogType,@chvGameData
		)

	RETURN
END