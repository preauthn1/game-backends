-- =============================================
-- Author:  jason
-- Create date: 2019/05/15
-- Description:	非連線型Jackpot計算紀錄
-- 2019/07/17	Eddie	調整對應欄位和參數(ServerID -> GameID, add FlagID)
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_Slot_JackpotPool_NoLink]
	@intGameID			INT,   --Game ID
	@intFlagID			INT,   --Flag ID
	@intTableNo				INT,	--桌子編號
	@intLevelID				INT,	-- level id
	@lngContribution		BIGINT = NULL,	--玩家貢獻值
	@lngTotalContribution	BIGINT = NULL, --累積貢獻值
	@lngAwardTotal			BIGINT = NULL,	--已發送獎金總和
	@chvMsg				VARCHAR(100)	OUTPUT
AS
BEGIN

	SET NOCOUNT ON


	DECLARE @Temp_Table TABLE ([GameID] INT,[FlagID] INT,[TableNo] INT,[LevelID] INT,[Contribution] BIGINT,[TotalContribution] BIGINT,[AwardTotal] BIGINT)

	SET @lngContribution = ISNULL(@lngContribution,0)
	SET @lngTotalContribution = ISNULL(@lngTotalContribution,0)
	SET @lngAwardTotal = ISNULL(@lngAwardTotal,0)


	IF NOT EXISTS (SELECT TOP 1 1 FROM [ServerInfo].[GameServerList] WITH (NOLOCK) WHERE [GameID] = @intGameID AND [ShowName] = @intFlagID)
	BEGIN
		SET @chvMsg = '操作失敗 :  查無此服務器'
		RETURN -1
	END

	
	IF NOT EXISTS (SELECT TOP 1 1 FROM [Game].[JackpotPool_NoLink] WITH (NOLOCK) WHERE [GameID] = @intGameID AND [FlagID] = @intFlagID AND [TableNo] = @intTableNo AND [LevelID] = @intLevelID)
	BEGIN
		INSERT INTO [Game].[JackpotPool_NoLink]
			([GameID],[FlagID],[TableNo],[LevelID],[Contribution],[TotalContribution],[AwardTotal])
		VALUES
			(@intGameID,@intFlagID,@intTableNo,@intLevelID,@lngContribution,@lngTotalContribution,@lngAwardTotal)

		IF(@@ROWCOUNT <> 1)
		BEGIN
			SET @chvMsg = '操作失敗 : 新增記錄失敗'
			RETURN -1
		END
		ELSE
		BEGIN

			SELECT	@intGameID AS [GameID],
					@intFlagID AS [FlagID],
					@intTableNo AS [TableNo],
					@intLevelID AS [LevelID],
					@lngContribution AS [Contribution],
					@lngTotalContribution AS [TotalContribution],
					@lngAwardTotal AS [AwardTotal]

			SET @chvMsg = '操作完成'
			RETURN 1
		END
	END


	IF((@lngContribution + @lngTotalContribution + @lngAwardTotal) <> 0)
	BEGIN
		UPDATE [Game].[JackpotPool_NoLink]
		SET [Contribution] = [Contribution] + @lngContribution,
			[TotalContribution] = [TotalContribution] + @lngTotalContribution,
			[AwardTotal] = [AwardTotal] + @lngAwardTotal
		OUTPUT INSERTED.[GameID],INSERTED.[FlagID],INSERTED.[TableNo],INSERTED.[LevelID],INSERTED.[Contribution],INSERTED.[TotalContribution],INSERTED.[AwardTotal] INTO @Temp_Table
		WHERE
			[GameID] = @intGameID
			AND [FlagID] = @intFlagID
			AND [TableNo] = @intTableNo
			AND [LevelID] = @intLevelID
	END
	ELSE
	BEGIN
		INSERT INTO @Temp_Table
		SELECT	[GameID],
				[FlagID],
				[TableNo],
				[LevelID],
				[Contribution],
				[TotalContribution],
				[AwardTotal]
		FROM [Game].[JackpotPool_NoLink] WITH (NOLOCK)
		WHERE
			[GameID] = @intGameID
			AND [FlagID] = @intFlagID
			AND [TableNo] = @intTableNo
	END


	IF NOT EXISTS (SELECT TOP 1 1 FROM @Temp_Table WHERE [GameID] = @intGameID AND [FlagID] = @intFlagID AND [TableNo] = @intTableNo AND [LevelID] = @intLevelID)
	BEGIN
		SET @chvMsg = '操作失敗 : 查無紀錄'
		RETURN -1
	END

	SELECT	[GameID],
			[FlagID],
			[TableNo],
			[LevelID],
			[Contribution],
			[TotalContribution],
			[AwardTotal]
	FROM @Temp_Table AS A
	WHERE
		A.[GameID] = @intGameID
		AND A.[FlagID] = @intFlagID
		AND A.[TableNo] = @intTableNo
	UNION ALL
	SELECT	[GameID],
			[FlagID],
			[TableNo],
			[LevelID],
			[Contribution],
			[TotalContribution],
			[AwardTotal]
	FROM [Game].[JackpotPool_NoLink] WITH (NOLOCK)
	WHERE
		[GameID] = @intGameID
		AND [FlagID] = @intFlagID
		AND [TableNo] = @intTableNo
		AND LevelID NOT IN (SELECT LevelID FROM @Temp_Table)


	SET @chvMsg = '操作成功'
	
	RETURN 1	
	
	
END
GO