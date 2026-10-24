USE [LogDB]
GO
/****** Object:  StoredProcedure [dbo].[PrGs_Slot_AddGameLog]    Script Date: 2020/1/1 W 02:59:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		Jason
-- Create date: 2019/05/24
-- Description:	CenterDB SPIs gJC{
-- 2019/06/20 ק logid 榡YMDHM [y@16X
-- =============================================
ALTER PROCEDURE [dbo].[PrGs_Slot_AddGameLog]
(
	@intUserID			INT,
	@intPlatformID		INT,
	@intAreaID			INT,
	@intGameID			INT,
	@intServerID		INT,
	@lngCurrentAmount	BIGINT,
	@lngCurrentCoupon	BIGINT = 0,
	@lngBetMoney		BIGINT,
	@lngBetCoupon		BIGINT = 0,
	@lngWinMoney		BIGINT,
	@lngFinalScore		BIGINT,
	@lngFinalCoupon		BIGINT = 0,
	@intGameType		TINYINT,
	@intLogType			TINYINT,
	@chvGameData		VARCHAR(MAX)
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @NowTime	DATETIME = GETDATE()

	/*    */
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
			[CurrentAmount],[BetMoney],[WinMoney],[FinalScore],[GameType],[LogType],[GameData]
		)
	VALUES
		(
			@LogID,@NowTime,@intPlatformID,@intAreaID,@intGameID,@intServerID,@intUserID,
			@lngCurrentAmount,@lngBetMoney,@lngWinMoney,@lngFinalScore,@intGameType,@intLogType,@chvGameData
		)

	RETURN
END
