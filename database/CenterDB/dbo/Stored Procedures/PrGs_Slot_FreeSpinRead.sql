
-- =============================================
-- Author:  jason
-- Create date: 2019/05/13
-- Description:	寫入FreeSpin紀錄
-- 2019/07/17	Eddie	調整對應欄位和參數(ServerID -> GameID, add FlagID)
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_Slot_FreeSpinRead]
	@intUserID			INT,		-- 用户ID
	@intGameID		    INT,		-- GameID
    @intFlagID		    INT,		-- FlagID
	@chvMsg				VARCHAR(100)	OUTPUT
AS
BEGIN

	SET NOCOUNT ON


	DECLARE @Temp_Table TABLE (UserID INT,GameID INT,FlagID INT,UserBet INT,FreeSpinGroup INT,Times INT,ChooseRate INT,IsCoupon TINYINT)
	DECLARE @Temp_WinMoney	TABLE (WinMoney BIGINT)
	DECLARE @WinMoney BIGINT
	DECLARE @GameType INT = 1 --1:freespin 2:dfdc


	IF NOT EXISTS (SELECT TOP 1 1 FROM [Game].[Slot_FreeSpinRecord] WITH (NOLOCK) WHERE UserID = @intUserID AND GameID = @intGameID AND FlagID = @intFlagID)
	BEGIN
		INSERT INTO [Game].[Slot_FreeSpinRecord]
			([UserID],[GameID],[FlagID],[UserBet],[FreeSpinGroup],[Times],[ChooseRate],[IsCoupon])
		OUTPUT INSERTED.UserID,INSERTED.GameID,INSERTED.FlagID,INSERTED.UserBet,INSERTED.FreeSpinGroup,INSERTED.Times,INSERTED.ChooseRate,INSERTED.IsCoupon INTO @Temp_Table
		VALUES
			(@intUserID,@intGameID,@intFlagID,0,0,0,0,0)
	END
	ELSE
	BEGIN
		UPDATE [Game].[Slot_FreeSpinRecord]
		SET [UserBet] = 0,
			[FreeSpinGroup] = 0,
			[Times] = 0,
			[ChooseRate] = 0,
			[IsCoupon] = 0
		OUTPUT DELETED.UserID,DELETED.GameID,DELETED.FlagID,DELETED.UserBet,DELETED.FreeSpinGroup,DELETED.Times,DELETED.ChooseRate,DELETED.IsCoupon INTO @Temp_Table
		WHERE
				UserID = @intUserID
				AND [GameID] = @intGameID
                AND [FlagID] = @intFlagID
	END

	
	IF NOT EXISTS (SELECT TOP 1 1 FROM @Temp_Table)
	BEGIN
		SET @chvMsg = '操作失敗 : 查無紀錄'
		RETURN -1
	END


	IF NOT EXISTS (SELECT TOP 1 1 FROM [Game].[GameWinMoney] WITH (NOLOCK) WHERE UserID = @intUserID AND [GameType] = @GameType)
	BEGIN
		INSERT INTO [Game].[GameWinMoney]
			([UserID],[GameType],[WinMoney])
		OUTPUT INSERTED.WinMoney INTO @Temp_WinMoney
		VALUES
			(@intUserID,@GameType,0)
	END
	ELSE
	BEGIN
		UPDATE [Game].[GameWinMoney]
		SET [WinMoney] = 0
		OUTPUT DELETED.WinMoney INTO @Temp_WinMoney
		WHERE
			UserID = @intUserID
			AND [GameType] = @GameType
	END


	IF NOT EXISTS (SELECT TOP 1 1 FROM @Temp_Table)
	BEGIN
		SET @chvMsg = '操作失敗 : 查無紀錄'
		RETURN -1
	END


	SELECT	@WinMoney = WinMoney FROM @Temp_WinMoney

	SET @WinMoney = ISNULL(@WinMoney,0)


	SELECT	[UserID],
			[GameID],
            [FlagID],
			[UserBet],
			[FreeSpinGroup],
			[Times],
			[ChooseRate],
			[WinMoney] = @WinMoney,
			[IsCoupon]
	FROM @Temp_Table

	
	SET @chvMsg = '操作成功'
	
	RETURN 1	
	
	
END
GO