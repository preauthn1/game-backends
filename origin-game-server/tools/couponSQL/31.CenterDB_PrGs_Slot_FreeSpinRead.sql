USE [CenterDB]
GO
/****** Object:  StoredProcedure [dbo].[PrGs_Slot_FreeSpinRead]    Script Date: 2019/6/28 下午 04:47:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:  jason
-- Create date: 2019/05/13
-- Description:	寫入FreeSpin紀錄
--
-- =============================================
ALTER PROCEDURE [dbo].[PrGs_Slot_FreeSpinRead]
	@intUserID			INT,		-- 用户ID
	@intServerID		INT,		-- ServerID
	@chvMsg				VARCHAR(100)	OUTPUT
AS
BEGIN

	SET NOCOUNT ON


	DECLARE @Temp_Table TABLE (UserID INT,ServerID INT,UserBet INT,FreeSpinGroup INT,Times INT,ChooseRate INT,IsCoupon TINYINT)
	DECLARE @Temp_WinMoney	TABLE (WinMoney BIGINT)
	DECLARE @WinMoney BIGINT
	DECLARE @GameType INT = 1 --1:freespin 2:dfdc


	IF NOT EXISTS (SELECT TOP 1 1 FROM [Game].[Slot_FreeSpinRecord] WITH (NOLOCK) WHERE UserID = @intUserID)
	BEGIN
		INSERT INTO [Game].[Slot_FreeSpinRecord]
			([UserID],[ServerID],[UserBet],[FreeSpinGroup],[Times],[ChooseRate],[IsCoupon])
		OUTPUT INSERTED.UserID,INSERTED.ServerID,INSERTED.UserBet,INSERTED.FreeSpinGroup,INSERTED.Times,INSERTED.ChooseRate,INSERTED.IsCoupon INTO @Temp_Table
		VALUES
			(@intUserID,@intServerID,0,0,0,0,0)
	END
	ELSE
	BEGIN
		UPDATE [Game].[Slot_FreeSpinRecord]
		SET [UserBet] = 0,
			[FreeSpinGroup] = 0,
			[Times] = 0,
			[ChooseRate] = 0,
			[IsCoupon] = 0
		OUTPUT DELETED.UserID,DELETED.ServerID,DELETED.UserBet,DELETED.FreeSpinGroup,DELETED.Times,DELETED.ChooseRate,DELETED.IsCoupon INTO @Temp_Table
		WHERE
				UserID = @intUserID
				AND [ServerID] = @intServerID
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
			[ServerID],
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