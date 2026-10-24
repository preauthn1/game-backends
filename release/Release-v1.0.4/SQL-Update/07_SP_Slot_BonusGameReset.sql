-- =============================================
-- Author:  Eddie/White
-- Create date: 2019/10/30
-- Description:	重置Slot_BonusGameRecord資料
-- =============================================

CREATE PROCEDURE [dbo].[PrGs_Slot_BonusGameReset]
	@intUserID			INT,		-- 用户ID
	@intGameID		    INT,		-- GameID
    @intFlagID		    INT,		-- FlagID
    @intOriginID		INT = 0,    -- 規則ID
	@chvMsg				VARCHAR(100)	OUTPUT
AS
BEGIN

    IF @intOriginID = 0 
        BEGIN 
            -- Return Data From Slot_BonusGameRecord
            DELETE FROM
                [Game].[Slot_BonusGameRecord]
            WHERE
                [UserID] = @intUserID
                AND
                [GameID] = @intGameID
                AND
                [FlagID] = @intFlagID
        END
    ELSE
        BEGIN
            -- Return Data From Slot_BonusGameRecord
            DELETE FROM
                [Game].[Slot_BonusGameRecord]
            WHERE
                [UserID] = @intUserID
                AND
                [GameID] = @intGameID
                AND
                [FlagID] = @intFlagID
                AND
                [OriginID] = @intOriginID
        END

SET @chvMsg = '操作成功'
RETURN 1

END