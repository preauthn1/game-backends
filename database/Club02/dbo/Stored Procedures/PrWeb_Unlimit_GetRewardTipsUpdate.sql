
-- =============================================
-- Author:  
-- Create date: 20190604
-- Description: 无极限-奖励说明列表
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Unlimit_GetRewardTipsUpdate]
(
	@intPlatformID	INT,
	@intLevel		INT,
	@chvLevelName	NVARCHAR(20),
	@lngMinAmount	BIGINT,
	@lngMaxAmount	BIGINT,
	@lngBonusAmount	BIGINT
)
AS
BEGIN

	SET NOCOUNT ON

	IF NOT EXISTS (SELECT TOP 1 1 FROM [dbo].[Unlimit_SystemRate] WITH (NOLOCK) WHERE PlatformID = @intPlatformID AND [Level] = @intLevel)
	BEGIN
		INSERT INTO [dbo].[Unlimit_SystemRate]
			([PlatformID],[Level],[LevelName],[MinAmount],[MaxAmount],[BonusAmount])
		VALUES
			(@intPlatformID,@intLevel,@chvLevelName,@lngMinAmount,@lngMaxAmount,@lngBonusAmount)
	END
	ELSE
	BEGIN
		UPDATE [dbo].[Unlimit_SystemRate]
		SET [LevelName] = CASE WHEN [LevelName] = @chvLevelName THEN [LevelName] ELSE @chvLevelName END,
			[MinAmount] = CASE WHEN [MinAmount] = @lngMinAmount THEN [MinAmount] ELSE @lngMinAmount END,
			[MaxAmount] = CASE WHEN [MaxAmount] = @lngMaxAmount THEN [MaxAmount] ELSE @lngMaxAmount END,
			[BonusAmount] = CASE WHEN [BonusAmount] = @lngBonusAmount THEN [BonusAmount] ELSE @lngBonusAmount END
		WHERE
			PlatformID = @intPlatformID
			AND [Level] = @intLevel
	END
	
	RETURN 1
END


