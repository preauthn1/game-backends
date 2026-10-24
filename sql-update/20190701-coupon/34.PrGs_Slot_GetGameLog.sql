USE [CenterDB]
GO

/****** Object:  StoredProcedure [dbo].[PrGs_Slot_GetGameLog]    Script Date: 2019/7/5 U 02:31:25 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Jason
-- Create date: 2019/05/24
-- Description:	Slot C{
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
		SET @chvMsg = 'ާ@~ : Τᤣsb'
		RETURN -1
	END

	IF NOT EXISTS (SELECT TOP 1 1 FROM [ServerInfo].[GameServerList] WHERE GameID = @intGameID)
	BEGIN
		SET @chvMsg = 'ާ@~ : AȾsb'
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
	SET @chvMsg = 'ާ@'
	RETURN 1
END
GO


