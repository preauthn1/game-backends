-- =============================================
-- Author:  Eddie/White
-- Create date: 2019/10/09
-- Description:	更新Slot_LightningGameRecord資料
-- =============================================

CREATE PROCEDURE [dbo].[PrGs_Slot_LightningGameRecord]
	@intUserID			INT,		-- 用户ID
	@intGameID		    INT,		-- GameID
    @intFlagID		    INT,		-- FlagID
    @txtData		    TEXT,		-- Json Data
	@chvMsg				VARCHAR(100)	OUTPUT
AS
BEGIN
    -- Task
    -- Check Record Exist, Not Exist -> Insert, Exist -> Check OriginID is Equal then Update Times
    IF NOT EXISTS (SELECT TOP 1 1 FROM [Game].[Slot_LightningGameRecord] WITH (NOLOCK) WHERE UserID = @intUserID AND GameID = @intGameID AND FlagID = @intFlagID)
        BEGIN
            IF datalength(@txtData) = 0
                BEGIN
                    SET @chvMsg = '無輸入資料'
                    RETURN -1
                END
            ELSE
                BEGIN
                    -- INSERT the record
                    INSERT INTO [Game].[Slot_LightningGameRecord]
                        ([UserID], [GameID], [FlagID], [Data])
                    VALUES
                        (@intUserID, @intGameID, @intFlagID, @txtData)
                END
        END
    ELSE
        BEGIN
            IF datalength(@txtData) = 0
                -- No Data, DELETE the record
                BEGIN
                    DELETE FROM [Game].[Slot_LightningGameRecord]
                    WHERE
                        [UserID] = @intUserID
                    AND
                        [GameID] = @intGameID
                    AND
                        [FlagID] = @intFlagID
                END
            ELSE
                -- Has Data, UPDATE the record
                BEGIN
                    UPDATE [Game].[Slot_LightningGameRecord]
                    SET
                        [Data] = @txtData
                    WHERE
                        [UserID] = @intUserID
                    AND
                        [GameID] = @intGameID
                    AND
                        [FlagID] = @intFlagID
                END
        
        END
    

SET @chvMsg = '操作成功'
RETURN 1

END