-- =============================================
-- Author:  jason
-- Create date: 2019/05/13
-- Description:	寫入FreeSpin紀錄
-- 2019/06/25 增加是否禮碼投注
-- 2019/08/22	Eddie	調整對應欄位和參數(ServerID -> GameID, add FlagID)
-- 2019/11/07 White 增加FreeSpinRecord的欄位(Data)
-- =============================================
ALTER PROCEDURE [dbo].[PrGs_Slot_FreeSpinRecord]
	@intUserID				INT,						-- 用户ID
	@intGameID				INT,		  			-- GameID
	@intFlagID				INT,		  			-- FlagID
	@intUserBet				INT,						-- 玩家下注金額
	@intFreeSpinGroup	INT,						-- FreeSpin群組
	@intTimes					INT,						-- FreeSpin次數
	@intChooseRate		INT,						-- 玩家選擇賠率
	@lngWinMoney			BIGINT,					-- 玩家輸贏金額
	@tnyIsCoupon			TINYINT = NULL, -- 是否禮碼投注
	@intOriginID			INT, 						-- 機率表ID
	@intWinTimes			INT, 						-- 累計贏的次數
	@txtData					TEXT = '',      -- 額外資訊
	@chvMsg						VARCHAR(100)	OUTPUT
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @GameType INT = 1 --1:freespin 2:dfdc
	
	IF NOT EXISTS (SELECT TOP 1 1 FROM [Game].[Slot_FreeSpinRecord] WITH (NOLOCK) WHERE UserID = @intUserID AND [GameID] = @intGameID AND [FlagID] = @intFlagID)
	BEGIN
		INSERT INTO [Game].[Slot_FreeSpinRecord]
			([UserID],[GameID],[FlagID],[UserBet],[FreeSpinGroup],[Times],[ChooseRate],[IsCoupon],[OriginID],[WinTimes],[Data])
		VALUES
			(@intUserID,@intGameID,@intFlagID,@intUserBet,@intFreeSpinGroup,@intTimes,@intChooseRate,@tnyIsCoupon,@intOriginID,@intWinTimes,@txtData)
	END
	ELSE
	BEGIN
		UPDATE [Game].[Slot_FreeSpinRecord]
		SET 
			[UserBet] = @intUserBet,
			[FreeSpinGroup] = @intFreeSpinGroup,
			[Times] = @intTimes,
			[ChooseRate] = @intChooseRate,
			[IsCoupon] = @tnyIsCoupon,
			[OriginID] = @intOriginID,
			[WinTimes] = @intWinTimes,
			[Data] = @txtData
		WHERE
			[UserID] = @intUserID
			AND 
			[GameID] = @intGameID
			AND 
			[FlagID] = @intFlagID
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
			[UserID] = @intUserID
			AND 
			[GameType] = @GameType
	END


	IF(@@ROWCOUNT <> 1)
	BEGIN
		SET @chvMsg = '操作失敗 : 更新個人FreeSpin失敗'
		RETURN -1
	END

	
	SET @chvMsg = '操作成功'
	
	RETURN 1	
	
END