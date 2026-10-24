USE [CenterDB]
GO
/****** Object:  StoredProcedure [dbo].[PrGs_Slot_AddGameLog]    Script Date: 2020/1/1 W 02:53:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		Jason
-- Create date: 2019/05/24
-- Description:	Slot gJC{
-- =============================================
ALTER PROCEDURE [dbo].[PrGs_Slot_AddGameLog]
(
	@intUserID			INT,
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
	@chvGameData		VARCHAR(MAX),
	@chvMsg				VARCHAR(100)	OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON


	IF NOT EXISTS (SELECT TOP 1 1 FROM [Users].[UserInfo] WITH (NOLOCK) WHERE UserID = @intUserID)
	BEGIN
		SET @chvMsg = 'ާ@~ : Τᤣsb'
		RETURN -1
	END

	IF NOT EXISTS (SELECT TOP 1 1 FROM [ServerInfo].[GameServerList] WHERE GameID = @intGameID AND ServerID = @intServerID)
	BEGIN
		SET @chvMsg = 'ާ@~ : AȾsb'
		RETURN -1
	END


	/*  Τx  */
	DECLARE @PlatformID INT
	DECLARE @AreaID		INT

	SELECT	@PlatformID = B.PlatformID,
			@AreaID = A.AreaID
	FROM [Users].[UserInfo] AS A WITH (NOLOCK)
		INNER JOIN [Area].[Area] AS B WITH (NOLOCK) ON A.AreaID = B.AreaID
	WHERE
		A.UserID = @intUserID


	EXEC [LogDB].[dbo].[PrGs_Slot_AddGameLog]
		@intUserID = @intUserID,
		@intPlatformID = @PlatformID,
		@intAreaID = @AreaID,
		@intGameID = @intGameID,
		@intServerID = @intServerID,
		@lngCurrentAmount = @lngCurrentAmount,
		@lngBetMoney = @lngBetMoney,
		@lngWinMoney = @lngWinMoney,
		@lngFinalScore = @lngFinalScore,
		@intGameType = @intGameType,
		@intLogType = @intLogType,
		@chvGameData = @chvGameData

	SET @chvMsg = 'ާ@'
	RETURN 1
END
