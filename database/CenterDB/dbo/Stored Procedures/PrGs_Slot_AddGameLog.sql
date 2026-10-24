
-- =============================================
-- Author:		Jason
-- Create date: 2019/05/24
-- Description:	Slot 寫入遊戲歷程
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_Slot_AddGameLog]
(
	@intUserID			INT,
	@intGameID			INT,
	@intServerID		INT,
	@lngCurrentAmount	BIGINT,
	@lngBetMoney		BIGINT,
	@lngWinMoney		BIGINT,
	@lngFinalScore		BIGINT,
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
		@lngBetMoney = @lngBetMoney,
		@lngWinMoney = @lngWinMoney,
		@lngFinalScore = @lngFinalScore,
		@intGameType = @intGameType,
		@intLogType = @intLogType,
		@chvGameData = @chvGameData

	SET @chvMsg = '操作完成'
	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_Slot_AddGameLog] TO [GameUser]
    AS [dbo];

