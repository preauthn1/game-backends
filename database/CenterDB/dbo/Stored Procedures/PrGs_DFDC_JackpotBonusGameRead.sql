-- =============================================
-- Author:  jason
-- Create date: 2019/05/13
-- Description:	JackpotBonusGame紀錄
-- 2019/06/12	Jason		#1660 修改為吐回相關資訊後不清空資料
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_DFDC_JackpotBonusGameRead]
	@intUserID			INT,		-- 用户ID
	@chvMsg				VARCHAR(100)	OUTPUT
AS
BEGIN

	SET NOCOUNT ON


	DECLARE @Temp_Table TABLE (UserID INT,[TurnID] INT,[Place] INT,[Icon] INT)
	DECLARE @Temp_WinMoney TABLE (Winmoney BIGINT)
	DECLARE @WinMoney BIGINT
	DECLARE	@GameType INT = 2 --1:freespin 2:dfdc

	
	IF NOT EXISTS (SELECT TOP 1 1 FROM [Game].[DFDC_BonusGameRecord] WITH (NOLOCK) WHERE UserID = @intUserID)
	BEGIN
		INSERT INTO [Game].[DFDC_BonusGameRecord]
			([UserID],[TurnID],[Place],[Icon])
		OUTPUT INSERTED.UserID,INSERTED.TurnID,INSERTED.Place,INSERTED.Icon INTO @Temp_Table
		VALUES
			(@intUserID,1,0,0),(@intUserID,2,0,0),(@intUserID,3,0,0),(@intUserID,4,0,0),(@intUserID,5,0,0),(@intUserID,6,0,0),(@intUserID,7,0,0),(@intUserID,8,0,0),(@intUserID,9,0,0)
	END
	ELSE
	BEGIN
		--UPDATE [Game].[DFDC_BonusGameRecord]
		--SET [Place] = 0,
		--	[Icon] = 0
		--OUTPUT DELETED.UserID,DELETED.TurnID,DELETED.Place,DELETED.Icon INTO @Temp_Table
		--WHERE
		--	[UserID] = @intUserID
		INSERT INTO @Temp_Table
		SELECT	UserID,
				[TurnID],
				[Place],
				[Icon]
		FROM [Game].[DFDC_BonusGameRecord] WITH (NOLOCK)
		WHERE
			UserID = @intUserID
	END


	IF((SELECT COUNT(1) FROM @Temp_Table WHERE UserID = @intUserID) <> 9)
	BEGIN
		SET @chvMsg = '操作失敗 : 查無紀錄'
		RETURN -1
	END


	IF NOT EXISTS (SELECT TOP 1 1 FROM [Game].[GameWinMoney] WITH (NOLOCK) WHERE UserID = @intUserID AND GameType = @GameType)
	BEGIN
		INSERT INTO [Game].[GameWinMoney]
			([UserID],[GameType],[WinMoney])
		OUTPUT INSERTED.WinMoney INTO @Temp_WinMoney
		VALUES
			(@intUserID,@GameType,0)
	END
	ELSE
	BEGIN
		--UPDATE [Game].[GameWinMoney]
		--SET [WinMoney] = 0
		--OUTPUT DELETED.WinMoney INTO @Temp_WinMoney
		--WHERE
		--	UserID = @intUserID
		--	AND GameType = @GameType
		INSERT INTO @Temp_WinMoney
		SELECT	Winmoney
		FROM [Game].[GameWinMoney] WITH (NOLOCK)
		WHERE
			UserID = @intUserID
			AND GameType = @GameType
	END

	IF(@@ROWCOUNT <> 1)
	BEGIN
		SET @chvMsg = '操作失敗 : 查無紀錄'
		RETURN -1
	END

	SELECT	@WinMoney = WinMoney FROM @Temp_WinMoney

	SET @WinMoney = ISNULL(@WinMoney,0)

	SELECT	[UserID],
			[TurnID],
			[Place],
			[Icon],
			[WinMoney] = @WinMoney
	FROM @Temp_Table
	
	SET @chvMsg = '操作成功'
	
	RETURN 1	
	
	
END




GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_DFDC_JackpotBonusGameRead] TO [GameUser]
    AS [dbo];

