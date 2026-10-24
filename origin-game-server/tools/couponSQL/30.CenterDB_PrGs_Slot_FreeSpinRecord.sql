USE [CenterDB]
GO
/****** Object:  StoredProcedure [dbo].[PrGs_Slot_FreeSpinRecord]    Script Date: 2019/6/25 下午 03:31:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:  jason
-- Create date: 2019/05/13
-- Description:	寫入FreeSpin紀錄
-- 2019/06/25 增加是否禮碼投注
-- =============================================
ALTER PROCEDURE [dbo].[PrGs_Slot_FreeSpinRecord]
	@intUserID			INT,		-- 用户ID
	@intServerID		INT,		-- ServerID
	@intUserBet			INT,		-- 玩家下注金額
	@intFreeSpinGroup	INT,		-- FreeSpin群組
	@intTimes			INT,		-- FreeSpin次數
	@intChooseRate		INT,		-- 玩家選擇賠率
	@lngWinMoney		BIGINT,		-- 玩家輸贏金額
	@tnyIsCoupon		TINYINT = NULL,    -- 是否禮碼投注
	@chvMsg				VARCHAR(100)	OUTPUT
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @GameType INT = 1 --1:freespin 2:dfdc
	
	IF NOT EXISTS (SELECT TOP 1 1 FROM [Game].[Slot_FreeSpinRecord] WITH (NOLOCK) WHERE UserID = @intUserID AND [ServerID] = @intServerID)
	BEGIN
		INSERT INTO [Game].[Slot_FreeSpinRecord]
			([UserID],[ServerID],[UserBet],[FreeSpinGroup],[Times],[ChooseRate],[IsCoupon])
		VALUES
			(@intUserID,@intServerID,@intUserBet,@intFreeSpinGroup,@intTimes,@intChooseRate,@tnyIsCoupon)
	END
	ELSE
	BEGIN
		UPDATE [Game].[Slot_FreeSpinRecord]
		SET [UserBet] = @intUserBet,
			[FreeSpinGroup] = @intFreeSpinGroup,
			[Times] = @intTimes,
			[ChooseRate] = @intChooseRate,
			[IsCoupon] = @tnyIsCoupon
		WHERE
			UserID = @intUserID
			AND [ServerID] = @intServerID
	END

	IF(@@ROWCOUNT <> 1)
	BEGIN
		SET @chvMsg = '操作失敗 : 更新個人FreeSpin失敗'
		RETURN -1
	END


	IF NOT EXISTS (SELECT TOP 1 1 FROM [Game].[GameWinMoney] WITH (NOLOCK) WHERE UserID = @intUserID AND GameType = @GameType)
	BEGIN
		INSERT INTO [Game].[GameWinMoney]
			([UserID],[GameType],[WinMoney])
		VALUES
			(@intUserID,@GameType,@lngWinMoney)
	END
	ELSE
	BEGIN
		UPDATE [Game].[GameWinMoney]
		SET [WinMoney] = @lngWinMoney
		WHERE
			UserID = @intUserID
			AND GameType = @GameType
	END


	IF(@@ROWCOUNT <> 1)
	BEGIN
		SET @chvMsg = '操作失敗 : 更新個人FreeSpin失敗'
		RETURN -1
	END

	
	SET @chvMsg = '操作成功'
	
	RETURN 1	
	
	
END



