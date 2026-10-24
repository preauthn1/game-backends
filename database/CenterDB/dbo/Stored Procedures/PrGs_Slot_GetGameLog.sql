
-- =============================================
-- Author:		Jason
-- Create date: 2019/05/24
-- Description:	Slot 取遊戲歷程
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_Slot_GetGameLog]
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


	IF NOT EXISTS (SELECT TOP 1 1 FROM [Users].[UserInfo] WITH (NOLOCK) WHERE UserID = @intUserID)
	BEGIN
		SET @chvMsg = '操作錯誤 : 用戶不存在'
		RETURN -1
	END

	IF NOT EXISTS (SELECT TOP 1 1 FROM [ServerInfo].[GameServerList] WHERE GameID = @intGameID)
	BEGIN
		SET @chvMsg = '操作錯誤 : 服務器不存在'
		RETURN -1
	END


	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrGs_Slot_GetGameLog]
		@intUserID = @intUserID,
		@intGameID = @intGameID,
		@dtStartTime = @dtStartTime,
		@dtEndTime = @dtEndTime,
		@intPageIndex = @intPageIndex,
		@intPageSize = @intPageSize,
		@intPageCount = @intPageCount	OUTPUT,
		@chvMsg = @chvMsg	OUTPUT
	SET @chvMsg = '操作完成'
	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_Slot_GetGameLog] TO [GameUser]
    AS [dbo];

