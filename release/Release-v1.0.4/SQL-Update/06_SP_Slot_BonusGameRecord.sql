-- =============================================
-- Author:  Eddie/White
-- Create date: 2019/10/09
-- Description:	更新Slot_BonusGameRecord資料
-- =============================================

CREATE PROCEDURE [dbo].[PrGs_Slot_BonusGameRecord]
	@intUserID			INT,		-- 用户ID
	@intGameID		    INT,		-- GameID
    @intFlagID		    INT,		-- FlagID
    @intOriginID        INT,        -- 規則ID
    @intTurnID          INT,        -- 第幾次選擇
    @intLocation        INT,        -- 選擇位置
    @intBetMoney        INT,        -- 下注金額
    @intBetRate         INT,        -- 下注倍率
    @txtData		    TEXT = '',		-- Json Data
	@chvMsg				VARCHAR(100)	OUTPUT
AS
BEGIN
    -- Check Record Exist, Not Exist -> Insert, Exist -> Check OriginID is Equal then Update Times
    IF NOT EXISTS (SELECT TOP 1 1 FROM [Game].[Slot_BonusGameRecord] WITH (NOLOCK) WHERE UserID = @intUserID AND GameID = @intGameID AND FlagID = @intFlagID AND OriginID = @intOriginID AND TurnID = @intTurnID)
        BEGIN
             -- Check Argument
            IF EXISTS (SELECT TOP 1 1 FROM [Game].[Slot_BonusGameRecord] WITH (NOLOCK) WHERE UserID = @intUserID AND GameID = @intGameID AND FlagID = @intFlagID AND OriginID = @intOriginID AND Location = @intLocation AND @intLocation <> 0)
                BEGIN
                    SET @chvMsg = '新增失敗，位置已被選擇'
                    RETURN -1
                END

            IF @intBetMoney = 0
                BEGIN
                    SET @chvMsg = '新增失敗，下注金額為0'
                    RETURN -1
                END
            
            IF @intBetRate = 0
                BEGIN
                    SET @chvMsg = '新增失敗，下注金額為0'
                    RETURN -1
                END

            -- INSERT the record
            INSERT INTO [Game].[Slot_BonusGameRecord]
                ([UserID], [GameID], [FlagID], [OriginID], [TurnID], [Location], [BetMoney], [BetRate], [Data])
            VALUES
                (@intUserID, @intGameID, @intFlagID, @intOriginID, @intTurnID, @intLocation, @intBetMoney, @intBetRate, @txtData)

            SET @chvMsg = '新增成功'
        END
    ELSE
        BEGIN
            -- 只更新位置
            UPDATE [Game].[Slot_BonusGameRecord]
            SET
                [Location] = @intLocation
            WHERE
                [UserID] = @intUserID
            AND
                [GameID] = @intGameID
            AND
                [FlagID] = @intFlagID
            AND
                [OriginID] = @intOriginID
            AND
                [TurnID] = @intTurnID

            SET @chvMsg = '更新成功'
        END
    
RETURN 1

END