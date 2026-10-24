-- =============================================
-- Author:  Eddie/White
-- Create date: 2019/10/30
-- Description:	讀取Slot_BonusGameRecord資料
-- =============================================

CREATE PROCEDURE [dbo].[PrGs_Slot_BonusGameRead]
	@intUserID			INT,		-- 用户ID
	@intGameID		    INT,		-- GameID
    @intFlagID		    INT,		-- FlagID
	@chvMsg				VARCHAR(100)	OUTPUT
AS
BEGIN

    -- Task
    -- Return Data From Slot_BonusGameRecord
    SELECT 
        [UserID],
        [GameID],
        [FlagID],
        [OriginID],
        [TurnID],
        [Location],
        [BetMoney],
        [BetRate],
        [Data]
    FROM
        [Game].[Slot_BonusGameRecord]
    WHERE
        [UserID] = @intUserID
        AND
        [GameID] = @intGameID
        AND
        [FlagID] = @intFlagID


SET @chvMsg = '操作成功'
RETURN 1

END