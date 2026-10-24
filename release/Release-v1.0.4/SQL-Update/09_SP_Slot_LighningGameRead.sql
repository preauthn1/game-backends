-- =============================================
-- Author:  Eddie/White
-- Create date: 2019/10/09
-- Description:	讀取Slot_LightningGameRecord資料
-- =============================================

CREATE PROCEDURE [dbo].[PrGs_Slot_LightningGameRead]
	@intUserID			INT,		-- 用户ID
	@intGameID		    INT,		-- GameID
    @intFlagID		    INT,		-- FlagID
	@chvMsg				VARCHAR(100)	OUTPUT
AS
BEGIN

    -- Task
    -- Return Data From Slot_LightningGameRecord
    SELECT 
        [UserID],
        [GameID],
        [FlagID],
        [Data]
    FROM
        [Game].[Slot_LightningGameRecord]
    WHERE
        [UserID] = @intUserID
        AND
        [GameID] = @intGameID
        AND
        [FlagID] = @intFlagID


SET @chvMsg = '操作成功'
RETURN 1

END